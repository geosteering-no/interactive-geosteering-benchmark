using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using Vectorizable;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.Statistics;

namespace EnKFLib
{
    public class EnKF2<Model>
        where Model: IVectorizable
    {
        IList<Model> modelRealizations = new List<Model>();
        MathNet.Numerics.LinearAlgebra.MatrixBuilder<double> _mBuild = Matrix<double>.Build;
        MathNet.Numerics.LinearAlgebra.VectorBuilder<double> _vBuild = Vector<double>.Build;

        private IList<Model> ModelRealizations
        {
            get { return modelRealizations; }
            set { modelRealizations = value; }
        }

        public void SetRealizations<T>(IEnumerable<T> reaslizations)
            where T: Model
        {
            List<Model> modelRealizations = new List<Model>();
            foreach (T t in reaslizations)
            {
                modelRealizations.Add(t);
            }
            ModelRealizations = modelRealizations;
        }

        IList<IDataProvider<Model>> _dataProviders = new List<IDataProvider<Model>>();

        public IList<IDataProvider<Model>> DataProviders
        {
            get { return _dataProviders; }
            set { _dataProviders = value; }
        }

        //IList<int> lastProcessedDataPosition = new List<int>();

        RandomVector _dataVarience;
        Vector _observation;

        Matrix _dataCovarience, predictedDataCovarience_, crossCovarienceParametersToData_; // for tmp use
        Vector presictedDataMean;
        Matrix _ensembleOfModels; //for tmp use
        Matrix _modelledData; //for tmp use

        Matrix _newEnsemble; //for tmp use
        Matrix _newData; //for tmp use

        public Matrix NewData
        {
            get { return (Matrix) _newData.Clone(); }
        }

        public Matrix OldData
        {
            get { return (Matrix) _modelledData.Clone(); }
        }


        /// <summary>
        /// Should be of a matrix now scala
        /// </summary>
        /// <returns></returns>
        private double ZeroMeanGaussian_dev(Vector v, Matrix covarienceMatrix)
        {
            double c = covarienceMatrix[0, 0];
            double x = v[0];
            double gaussianNoScaling = System.Math.Pow(c, -0.5) * System.Math.Exp(-0.5 * x / c *x);
            return gaussianNoScaling;
        }

        /// <summary>
        /// Computes multiplyer
        /// TODO: make it controllable
        /// </summary>
        private void ComputeWeightMultiplier_dev()
        {
            var averageDataResponce = presictedDataMean;
            var negativeResponce = averageDataResponce.Multiply(-1.0);
            var misfit = negativeResponce.Add(_observation);

            Vector residual = (Vector) misfit; //renaming
            weightMultiplier_dev_ = ZeroMeanGaussian_dev(residual, predictedDataCovarience_);

        }

        private double weightMultiplier_dev_ = Double.NaN; 

        /// <summary>
        /// Weight multiplier for the component based on AGM formulas
        /// FIXME: This one is simpler is done fore testing
        /// </summary>
        public double WeightMultiplier_dev
        {
            get
            {
                return weightMultiplier_dev_;
            }
        }


        //TODO remove from this class
        #region utils
        Random rnd;

        public double NextNormalRandomValue(double mean, double stdDev)
        {
            double u1 = rnd.NextDouble(); //these are uniform(0,1) random doubles
            double u2 = rnd.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }

        #endregion


        public EnKF2()
        {
            rnd = new Random();
        }


        public int EnsembleCount
        {
            get{
                return ModelRealizations.Count;
            }
        }

        /// <summary>
        /// GEts n different indences for current ensemble size
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public int[] NextRandomIndeces(int n)
        {
            int[] res = new int[n];
            for (int i = 0; i < n; ++i)
            {
                int cur = rnd.Next(EnsembleCount);
                bool conflict = true;
                while (conflict)
                {
                    conflict = false;
                    for (int j = 0; j < i; ++j)
                    {
                        if (cur == res[j])
                        {
                            conflict = true;
                            break;
                        }
                    }
                    if (conflict)
                    {
                        cur = (cur + 1) % EnsembleCount;
                    }
                }
                res[i] = cur;
            }
            return res;
        }

        /// <summary>
        /// kept public temporarily for back compatibility
        /// </summary>
        public int DataSize
        {
            get
            {
                return _modelledData.RowCount;
            }
        }


        /// <summary>
        /// kept public temporarily for back compatibility
        /// </summary>
        public int ParameterSize
        {
            get
            {
                return _ensembleOfModels.RowCount;
            }

        }


        ////TODO consider doing elsewhere
        ///// <summary>
        ///// assumes uncorelated noise
        ///// </summary>
        ///// <returns></returns>
        //private Vector GetDataNoise(double multiplier = 1)
        //{
        //    Vector res = new DenseVector(DataSize);
        //    for (int i = 0; i < DataSize; ++i)
        //    {
        //        res[i] = NextNormalRandomValue(0, _dataCovarience[i, i] * multiplier);
        //    }
        //    return res;
        //}

        //public Vector GetMeanVector(IList<Vector> dataVectors)
        //{
        //    if (dataVectors.Count == 0)
        //    {
        //        return null;
        //    }
        //    Vector res = new DenseVector(dataVectors[0].size());
        //    foreach (Vector v in dataVectors)
        //    {
        //        res.add(v);
        //    }
        //    res.scale(1.0 / dataVectors.Count);
        //    return res;
        //}


        ////TODO consider changing to matrix notation
        ///// <summary>
        ///// Computes covarience between components for the given ensemble of vectors
        ///// </summary>
        ///// <param name="vectors"></param>
        ///// <returns></returns>
        //private Matrix GetVectorCovarience(IList<Vector> vectors)
        //{
        //    int vectorSize = vectors[0].size();
        //    int vectorsCount = vectors.Count;

        //    //Matrix cov = new DenseMatrix(vectorsCount, vectorsCount);
        //    //C^e_DD
        //    predictedDataCovarience_ = new DenseMatrix(vectorSize, vectorSize);
        //    //List<Vector> dataVectors = new List<Vector>(EnsembleCount);
        //    //foreach (Vector v in ensemble)
        //    //{
        //    //    dataVectors.Add(modelToData(v));
        //    //}
        //    Vector meanData = GetMeanVector(vectors);
        //    DenseMatrix ddBar = new DenseMatrix(vectorSize, vectorsCount);
        //    for (int i = 0; i < vectorsCount; ++i)
        //    {
        //        Vector tmpI = vectors[i].copy();
        //        tmpI.add(-1.0, meanData);
        //        for (int j = 0; j < vectorSize; ++j)
        //        {
        //            ddBar[j, i] = tmpI[j];
        //        }
        //        //for (int j = 0; j < EnsembleCount; ++j)
        //        //{
        //        //    Vector tmpJ = dataVectors[j].copy();
        //        //    tmpJ.add(-1.0,meanData);
        //        //    predictedDataCovarience_[i, j] = 1.0 / (EnsembleCount - 1) * tmpI.dot(tmpJ);
        //        //}
        //    }
        //    Matrix cov = ddBar.transBmult(1.0 / (vectorsCount - 1), ddBar, predictedDataCovarience_);
        //    return cov;
        //}


        /// <summary>
        /// TODO: utilize GetVectorCovarience function
        /// </summary>
        /// <param name="dataVectors"></param>
        private void UpdateCovarienceOfPredictedData(IList<Vector> dataVectors)
        {
            //C^e_DD

            predictedDataCovarience_ = new DenseMatrix(DataSize, DataSize);
            var ddBar = _mBuild.DenseOfColumnVectors(dataVectors);
            //ColumnListMatrix ddBar = new ColumnListMatrix(dataVectors);
            //TODO cehck rows vs columns here
            var rowSums = ddBar.RowSums();
            var means = rowSums.Divide(dataVectors.Count);
            presictedDataMean = (Vector)means;
            predictedDataCovarience_ = (Matrix)GetCovarianceMatrix(ddBar);
            //predictedDataCovarience_.rank1(1.0 / (EnsembleCount - 1), ddBar);
        }

        /// <summary>
        /// computes data covarience of a matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static Matrix<double> GetCovarianceMatrix(Matrix<double> matrix)
        {
            var columnAverages = matrix.ColumnSums() / matrix.RowCount;
            var centeredColumns = matrix.EnumerateColumns().Zip(columnAverages, (col, avg) => col - avg);
            var centered = DenseMatrix.OfColumnVectors(centeredColumns);
            var normalizationFactor = matrix.RowCount == 1 ? 1 : matrix.RowCount - 1;
            return centered.TransposeThisAndMultiply(centered) / normalizationFactor;
        }

        private void UpdateCrossCovarienceModelToData(IList<Vector> dataVectors, IList<Vector> modelVectors)
        {
            //C^e_md
            crossCovarienceParametersToData_ = new DenseMatrix(ParameterSize, DataSize);
            //Matrix ddBar = new ColumnListMatrix(dataVectors);
            var ddBar = _mBuild.DenseOfColumnVectors(dataVectors);
            //Matrix mmBar = new ColumnListMatrix(modelVectors);
            var mmBar = _mBuild.DenseOfColumnVectors(modelVectors);
            //crossCovarienceParametersToData_.rank2(1.0 / (EnsembleCount - 1), mmBar, ddBar);
            //mmBar.transBmult(1.0 / (EnsembleCount - 1), ddBar, crossCovarienceParametersToData_);
            //TODO check matrix orientations / transpose
            ddBar.Multiply(mmBar, crossCovarienceParametersToData_);
            crossCovarienceParametersToData_ = (Matrix) crossCovarienceParametersToData_.Multiply(1.0 / (EnsembleCount - 1));
        }

        //TODO It is better to collect linearly corelated data i.e. all from one time stem into one vector
        //consider this for API implementation
        //can get in a list of models outputing vector each -> into one big vector
        //a list of measurements as a vecto or list of smaller vectors that correspond to models

        private int CalculateDataSize()
        {
            int res = 0;
            foreach (IDataProvider<Model> prov in DataProviders)
            {
                res += prov.SingleDataSize * prov.DataPointsCount;
            }
            return res;
        }

        /// <summary>
        /// Creating enbsemble of vectors
        /// </summary>
        private void GenerateEnsemble()
        {
            //Generate ensemeble reprezentation
            var ensembleOfModels = new List<Vector>(EnsembleCount);
            foreach (Model mod in ModelRealizations)
            {
                ensembleOfModels.Add(mod.Vector);
            }

            _ensembleOfModels = (Matrix)_mBuild.DenseOfColumnVectors(ensembleOfModels);
        }

        private void PutBackEnsemble(Matrix ensemble)
        {
            if (ensemble.ColumnCount != ModelRealizations.Count)
            {
                throw new IndexOutOfRangeException();
            }
            //Put the new representation in
            for (int i = 0; i < ensemble.ColumnCount; ++i)
            {
                ModelRealizations[i].Vector = (Vector)ensemble.Column(i);
            }
        }

        /// <summary>
        /// THis function generates the measuremnts observed by the algorithm
        /// And puts it to _observation field
        /// </summary>
        private void ExtractDataAndVarience()
        {
            int dataSize = CalculateDataSize();
            {
                //Vector vec = new DenseVector(dataSize);
                //IEnumerator<VectorEntry> vecEnum = vec.GetEnumerator();
                List<Vector> dataVectors = new List<Vector>();
                foreach (IDataProvider<Model> prov in DataProviders)
                {
                    dataVectors.Add(prov.DataValue);
                }
                _observation = DataUtils.CollectIntoVector(dataVectors);
            }


            {
                //Vector vec = new DenseVector(dataSize);
                //IEnumerator<VectorEntry> vecEnum = vec.GetEnumerator();
                List<Vector> dataVariences = new List<Vector>();
                foreach (IDataProvider<Model> prov in DataProviders)
                {
                    dataVariences.Add(prov.ErrorVarience);
                    //Vector res = prov.ErrorVarience;
                    //foreach (VectorEntry newEntry in res)
                    //{
                    //    vecEnum.MoveNext();
                    //    vecEnum.Current.set(newEntry.get());
                    //}
                }

                var vec = DataUtils.CollectIntoVector(dataVariences);
                _dataVarience = new RandomVector(vec);
                _dataCovarience = (Matrix)_mBuild.SparseOfDiagonalVector(vec);
            }

        }

        private void GenerateData(ref Matrix data)
        {
            //TODO switch to matrices? 
            var list = new List<Vector>(EnsembleCount);

            foreach (Model mod in ModelRealizations)
            {
                //Vector vec = new DenseVector(dataSize);
                //IEnumerator<VectorEntry> vecEnum = vec.GetEnumerator();
                List<Vector> parts = new List<Vector>();
                foreach (IDataProvider<Model> prov in DataProviders)
                {
                    parts.Add(prov.GenerateData(mod));
                }

                var vec = DataUtils.CollectIntoVector(parts);
                list.Add(vec);
            }

            data = (Matrix)_mBuild.DenseOfColumnVectors(list);

        }


        private Vector solve(Matrix m, Vector b)
        {
            Vector sol = new DenseVector(b.Count);
            var solver = new MathNet.Numerics.LinearAlgebra.Double.Solvers.BiCgStab();
            var iterator = new MathNet.Numerics.LinearAlgebra.Solvers.Iterator<double>();
            var prcond = new MathNet.Numerics.LinearAlgebra.Double.Solvers.DiagonalPreconditioner();

            solver.Solve(m, b, sol, iterator, prcond);
            
            return sol;
        }

        /// <summary>
        /// Updates a single vecrtor in the ensemble
        /// </summary>
        /// <param name="oldVec"></param>
        /// <param name="oldData"></param>
        /// <returns></returns>
        private Vector update(Vector oldVec, Vector oldData)
        {
            var residual = _vBuild.DenseOfVector(_observation);
            //                new DenseVector(_observation, true);
            //residual.add(_dataVarience).add(-1.0, oldData);
            var tmp = residual.Add(_dataVarience.Realize());
            var tmp2 = residual.Map2((x, y) => x - y, oldData);

            Vector sol = solve(predictedDataCovarience_, (Vector) residual);
            //Vector res = new DenseVector(oldVec.Count);
            //crossCovarienceParametersToData_.mult(sol, res);
            var res = crossCovarienceParametersToData_.Multiply(sol);
            res = res.Add(oldVec);
            return (Vector) res;
        }

        public void SkipUpdate()
        {
            GenerateEnsemble();
            _newEnsemble = _ensembleOfModels;
        }

        public Matrix GetOneOverN(int n)
        {
            var oneN = _mBuild.Dense(n, n, 1.0 / n);
            return (Matrix)oneN;
        }

        public Matrix GetCenteredMatrix(Matrix dataMatrix)
        {
            //TODO , Matrix storage = null
            //Each ensemble member is a column
            int n = dataMatrix.ColumnCount;
            var I = _mBuild.DiagonalIdentity(n);
            var oneN = GetOneOverN(n);
            var centeredData = dataMatrix * (I - oneN);
            return (Matrix)(centeredData * (1.0/Math.Sqrt(n-1)));
        }

        /// <summary>
        /// Returns the reduced svd wraped in a class
        /// The part of preserved enbergy in singular vectors is specified as a parameter
        /// </summary>
        /// <param name="svdOrig"></param>
        /// <param name="totalEnergyFraction"></param>
        /// <returns></returns>
        public ReducedSvd GetReducedOrderSvd(
            MathNet.Numerics.LinearAlgebra.Factorization.Svd<double> svdOrig, double totalEnergyFraction = 1.0)
        {
            //the commented part is incorrect when there is no varience in data response
            //if (totalEnergyFraction > 1.0 - 1e-7)
            //{
            //    return new ReducedSvd()
            //    {
            //        Sp = (Vector)svdOrig.S,
            //        Up = (Matrix)svdOrig.U,
            //        VT = (Matrix)svdOrig.VT
            //    };
            //}
            var eps = 1e-9;

            //We start from
            //Nd x N
            //==>
            //We end up 
            //Np x N
            //
            var fullEnergy = svdOrig.S.Sum();
            var requiredEnergy = totalEnergyFraction * fullEnergy;
            //the entries sorted in the increasing order
            var sum = 0.0;
            var listOfVecotrs = new List<Vector>();
            var listOfValues = new List<double>();
            for (int i = 0; i<svdOrig.S.Count; ++i)
            {
                if (sum + eps > requiredEnergy)
                {       
                    break;
                }
                sum += svdOrig.S[i];
                listOfValues.Add(svdOrig.S[i]);
                listOfVecotrs.Add((Vector)svdOrig.U.Column(i));
            }

            if (listOfValues.Count == 0)
            {
                return null;
            }
            Vector sp = (Vector)_vBuild.DenseOfEnumerable(listOfValues);
            Matrix up = (Matrix)_mBuild.DenseOfColumnVectors(listOfVecotrs);
            return new ReducedSvd()
            {
                Sp = sp,
                Up = up,
                VT = (Matrix)svdOrig.VT
            };

        }

        public Matrix GetPerturbedObservationsMatrix(int ensembleSize, Vector observation, RandomVector perturbationVector)
        {
            List<Vector> perturbed = new List<Vector>();
            for (int i = 0; i < ensembleSize; i++)
            {
                perturbed.Add((Vector) (observation + perturbationVector.Realize()));
                
            }

            return (Matrix)_mBuild.DenseOfColumnVectors(perturbed);
        }

        public const double EPS = 1E-9;

        /// <summary>
        /// Updates EnKF
        /// </summary>
        /// <param name="multiStepMult">Data relyability multiplayer might be used to achieve consistency in multistep</param>
        /// <returns>The relative dicrese of misfit</returns>
        public double Update(double multiStepMult = 1.0, bool useRolfsTrick=false)
        {
            DateTime start = DateTime.Now;
            //Extract Data and varience
            //this is for actual data
            ExtractDataAndVarience();
            //_dataVarience
            //_dataCovarience
            //_observation

            //Generate ensemeble reprezentation
            GenerateEnsemble();
            //this results in a matrix
            //_ensembleOfModels
            var ensemble = _ensembleOfModels;
            var timePreparation = DateTime.Now - start;
            System.Console.WriteLine("Preparation        " + timePreparation.TotalMilliseconds);

            start = DateTime.Now;
            //Generate data and covarience of data
            GenerateData(ref _modelledData);
            //TODO apply Rolf's trick this is modeled data _modelledData
            var timeDataGeneration = DateTime.Now - start;
            System.Console.WriteLine("Data generation    " + timeDataGeneration.TotalMilliseconds);

            start = DateTime.Now;
            //the centered data
            var deltaD = GetCenteredMatrix(_modelledData);
            //the centered models
            var deltaM = GetCenteredMatrix(_ensembleOfModels);
            var centering = DateTime.Now - start;
            System.Console.WriteLine("Centering          " + centering.TotalMilliseconds);

            //TODO if want to do data reduction we do it here

            //we want SVD of deltaD
            start = DateTime.Now;
            var svdData = deltaD.Svd(true);
            TimeSpan timeSVD = DateTime.Now - start;
            //svdData.S  is the totalEnergyFraction , i.e. singular values
            System.Console.WriteLine("SVD                " + timeSVD.TotalMilliseconds);
            
            start = DateTime.Now;
            //Here we can set totalEnergyFraction level for retained singular vectors
            //TODO test with not 1.0
            var reducedSvd = GetReducedOrderSvd(svdData, 1.0);
            //reducedSvd is null if there is nothing in the responce to data
            TimeSpan timeReducedSvd = DateTime.Now - start;
            System.Console.WriteLine("SVD reduction      " + timeReducedSvd.TotalMilliseconds);

            start = DateTime.Now;

            var robustVarience = _dataVarience;
            if (useRolfsTrick)
            {
                var robustDataStd = _dataVarience.Deviation;
                //TODO check if we need to square
                var trickWorked = false;
                for (var i = 0; i < robustDataStd.Count; ++i)
                {
                    var predictedMin = _modelledData.Row(i).Enumerate().Minimum();
                    if (_observation[i] < predictedMin - EPS)
                    {
                        var newStd = Math.Abs(_observation[i] - predictedMin);
                        robustDataStd[i] = Math.Max(robustDataStd[i], newStd);
                        trickWorked = true;
                    }

                    var predictedMax = _modelledData.Row(i).Enumerate().Maximum();
                    if (_observation[i] > predictedMax + EPS)
                    {
                        var newStd = Math.Abs(_observation[i] - predictedMax);
                        robustDataStd[i] = Math.Max(robustDataStd[i], newStd);
                        trickWorked = true;
                    }
                }


                if (trickWorked)
                {
                    Console.WriteLine("Rolf's trick has been applied: " + robustDataStd);
                    robustVarience = new RandomVector(robustDataStd);
                }
            }

            //misfit
            var perturbedData = GetPerturbedObservationsMatrix(_modelledData.ColumnCount, _observation, robustVarience);
            //TODO fix Rolf's adaptivity 
            //_observation is the observation
            var misfit = perturbedData - _modelledData;
            var mismatchMetricBefore = misfit.L2Norm(); // TODO do L2 weighted with Cd (expected data covrience)
            TimeSpan timeExplicitDataPerturb = DateTime.Now - start;
            System.Console.WriteLine("Perturbation time " + timeExplicitDataPerturb.TotalMilliseconds);

            start = DateTime.Now;
            if (reducedSvd != null)
            {
                //find X0 projecttion of the data covarience onto ansemble usbspace

                var dataProjectionX0 = reducedSvd.InverseSigma * reducedSvd.Up.Transpose()
                                                               * _dataCovarience
                                                               * reducedSvd.Up * reducedSvd.InverseSigma.Transpose();


                var evd = dataProjectionX0.Evd(Symmetricity.Symmetric);

                var x1 = reducedSvd.Up * reducedSvd.InverseSigma.Transpose() * evd.EigenVectors;

                //now we finished all preparation
                //parts of the calman gain matrices
                var s1 = (_mBuild.DiagonalIdentity(evd.EigenValues.Count) + evd.D).Inverse() * x1.Transpose();
                var s2 = deltaD.Transpose() * x1;

                var s3 = s2 * s1;


                var tmp = s3 * misfit;

                var newEnsembleModels = _ensembleOfModels + deltaM * tmp;

                //put back the results

                ////Generate or update matrices            
                ////TODO fix the rest
                ////UpdateCovarienceOfPredictedData(_modelledData);

                //predictedDataCovarience_ = (Matrix)predictedDataCovarience_.Add(_dataCovarience);
                //ComputeWeightMultiplier_dev();

                ////UpdateCrossCovarienceModelToData(_modelledData, _ensembleOfModels);



                ////_newEnsemble = new List<Vector>(_ensembleOfModels.Count);
                ////IEnumerator<Vector> dataEnum = _modelledData.GetEnumerator();
                ////foreach (Vector v in _ensembleOfModels)
                ////{
                ////    dataEnum.MoveNext();
                ////    _newEnsemble.Add(update(v, dataEnum.Current));
                ////}
                ////dataEnum.Dispose();

                _newEnsemble = (Matrix) newEnsembleModels;
                PutBackEnsemble(_newEnsemble);
            }
            TimeSpan timeUpdate = DateTime.Now - start;
            System.Console.WriteLine("Time update        " + timeUpdate.TotalMilliseconds);

            //generate again to check the update quality
            GenerateData(ref _newData);
            //misfit
            var misfit3 = perturbedData - _newData;
            var mismatchMetricAfter = misfit3.L2Norm(); // TODO do L2 weighted with Cd (expected data covrience)
            var relativeDecrease =  -(mismatchMetricAfter - mismatchMetricBefore) / mismatchMetricBefore;
            return relativeDecrease;
        }

        public void AcceptUpdate()
        {
            //PutBackEnsemble(_newEnsemble);
        }

        public double distanceToData()
        {
            //double sum = 0;
            //foreach (Vector generatedData in _newData)
            //{
            //    Vector diff = generatedData.copy();
            //    diff.add(-1.0, _observation);
            //    sum += diff.norm(AbstractVector.NormType.Two);
            //}
            //return sum/_newData.Count;
            throw new NotImplementedException();
        }



        /// <summary>
        /// Multi-step update
        /// </summary>
        /// <param name="dataPoint"></param>
        /// <param name="measuredData"></param>
        /// <param name="steps"></param>
        /// <param name="a">multiplier to create geometric progression</param>
        public void UpdateMultiStep(int steps)
        {
            //We need to make this work
            //defined as in geometric progression
            double a = 2.0;

            double mult = System.Math.Pow(a, steps - 1);
            Update(mult);

            for (int i = 1; i < steps; ++i)
            {
                Update(mult);
                mult /= a;
            }

        }


        //claster lloyd was here

        //and another claster


    }
}
