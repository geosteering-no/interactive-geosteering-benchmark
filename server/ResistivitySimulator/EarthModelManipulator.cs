using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyFunctions.Default.RandomNumberGenerator;
using MathNet.Numerics.LinearAlgebra.Double;
using Variogram;

namespace ResistivitySimulator
{
    public class EarthModelManipulator
    {
        //private double _resIn = 250;     // 250
        //private double _resOut = 50;    //  50
        private List<double> _xPositions;
        private List<IEarthModelRealization> _realizations = new List<IEarthModelRealization>();
        private int _numRealizations = 30;      // Only to be changed in the c'tor
        private RandomNumberGenerator rNumGen;



        private EarthModelManipulator() { }

        public EarthModelManipulator(int numRealizations, int randomSeed = 0)
        {
            _numRealizations = numRealizations;
            rNumGen = new RandomNumberGenerator(randomSeed);
        }

        //double deviation;
        //TODO limit visibility

        public List<double> XPositions
        {
            get { return _xPositions; }
        }

        public double IntervalLength(int ind)
        {
            return _xPositions[ind + 1] - _xPositions[ind];
        }

        public EarthModelManipulator Clone()
        {
            EarthModelManipulator clone = new EarthModelManipulator();
            clone._xPositions = _xPositions;
            clone._numRealizations = _numRealizations;
            foreach (var model in Realizations)
            {
                //EarthModelRelization newModel = new EarthModelRelization(clone, model.
                throw new NotImplementedException();
                //TODO fix
                //clone.Realizations.Add(new EarthModelRelization(clone, model));
            }
            return clone;
            //clone.
        }

        public double IntervalStart(int ind)
        {
            return _xPositions[ind];
        }

        public int IndexOfX(double x)
        {
            int left = 0;
            int right = _xPositions.Count - 1;
            while (right - left > 1)
            {
                int mid = (right + left) / 2;
                if (_xPositions[mid] > x)
                {
                    right = mid;
                }
                else
                {
                    left = mid;
                }
            }
            //TODO check
            return left;
        }

        public int ParameterCount
        {
            get
            {
                //TODO fix
                return 10;
            }
        }

        public int TotalComponentsCount
        {
            get
            {
                //TODO fix
                return 12;
            }
        }

        public void AddFaultNear(Fault f, double deviation = 1)
        {
            throw new NotImplementedException();
            ////Shift left-right; increase displacement
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberOfPoints"></param>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="bottomOfLayer1"></param>
        /// <param name="topOfLayer1"></param>
        /// <param name="bottomOfLayer2"></param>
        /// <param name="topOfLayer2"></param>
        /// <param name="deviation"></param>
        /// <param name="resistivityLayerBottom"></param>
        /// <param name="resistivityLayerTop"></param>
        /// <param name="resistivityOutsideLayer"></param>
        /// <param name="sizeReduction">!NOT USED!</param>
        /// <param name="deviationDepthOfReservoir"></param>
        /// <param name="corelationOfHorizons">The corelation arameter between horizons between zero and one. Refering to Rolf's paper</param>
        public void GenerateRealizationsTwoLayerFromVariogramWithKrigging(int numberOfPoints, double minX, double maxX,
            double bottomOfLayer1, double topOfLayer1, double bottomOfLayer2, double topOfLayer2, double deviation,
            double resistivityLayerBottom, double resistivityLayerTop, double resistivityOutsideLayer,
            bool sizeReduction = false, double deviationDepthOfReservoir = 0.0, double corelationOfHorizons = 0.7,
            double range = 350.0)
        {
            // Layer1 is the bottom reservoir layer, Layer2 is the top reservoir layer

            _xPositions = new List<double>(numberOfPoints);
            double pos = minX;
            double deltaX = (maxX - minX) / (numberOfPoints - 1);
            for (int i = 0; i < numberOfPoints; ++i)
            {
                _xPositions.Add(pos);

                pos += deltaX;
            }

            //TODO get the matrix from the builder
            _realizations = new List<IEarthModelRealization>(NumberOfRealizations);

            //VAriogram builder initialization
            //TODO fix the parameters
            var nugget = 0.0;
            var sill = deviation;
            //var range = (maxX - minX);
            VariogramBuilder vBuilder = new VariogramBuilder(_xPositions, new ExpVariogram(nugget, sill, range));
            for (int j = 0; j < NumberOfRealizations; ++j)
            {
                var topPosionAdjustment =
                    rNumGen.NextNormalRandomValueStatic(0.0, deviationDepthOfReservoir);
                

                Vector[] zeroMeanVectors = new Vector[4];
                for (int i = 0; i < 4; ++i)
                {
                    zeroMeanVectors[i] = vBuilder.DrawVector();
                    if (i > 0)
                    {
                        zeroMeanVectors[i] = Krigger.Krig(zeroMeanVectors[i - 1], zeroMeanVectors[i], corelationOfHorizons);
                    }
                }

                var upperBMean = rNumGen.NextNormalRandomValueStatic(bottomOfLayer1, deviation) + topPosionAdjustment;
                var upperB = zeroMeanVectors[0] + upperBMean;

                var lowerBMean = rNumGen.NextNormalRandomValueStatic(topOfLayer1, deviation) + topPosionAdjustment;
                var lowerB = zeroMeanVectors[1] + lowerBMean;

                var upperB2Mean = rNumGen.NextNormalRandomValueStatic(bottomOfLayer2, deviation) + topPosionAdjustment;
                var upperB2 = zeroMeanVectors[2] + upperB2Mean;

                var lowerB2Mean = rNumGen.NextNormalRandomValueStatic(topOfLayer2, deviation) + topPosionAdjustment;
                var lowerB2 = zeroMeanVectors[3] + lowerB2Mean;

                _realizations.Add(new EarthModelRealizationMultiLayer(this, upperB, lowerB, upperB2, lowerB2,
                resistivityLayerBottom, resistivityLayerTop, resistivityOutsideLayer));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberOfPoints"></param>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="bottomOfLayer1"></param>
        /// <param name="topOfLayer1"></param>
        /// <param name="bottomOfLayer2"></param>
        /// <param name="topOfLayer2"></param>
        /// <param name="deviation"></param>
        /// <param name="resistivityLayerBottom"></param>
        /// <param name="resistivityLayerTop"></param>
        /// <param name="resistivityOutsideLayer"></param>
        /// <param name="sizeReduction">!NOT USED!</param>
        /// <param name="deviationDepthOfReservoir"></param>
        public void GenerateRealizationsTwoLayerFromVariogram(int numberOfPoints, double minX, double maxX,
            double bottomOfLayer1, double topOfLayer1, double bottomOfLayer2, double topOfLayer2, double deviation,
            double resistivityLayerBottom, double resistivityLayerTop, double resistivityOutsideLayer,
            bool sizeReduction = false, double deviationDepthOfReservoir = 0.0)
        {
            // Layer1 is the bottom reservoir layer, Layer2 is the top reservoir layer

            _xPositions = new List<double>(numberOfPoints);
            double pos = minX;
            double deltaX = (maxX - minX) / (numberOfPoints - 1);
            for (int i = 0; i < numberOfPoints; ++i)
            {
                _xPositions.Add(pos);

                pos += deltaX;
            }

            //TODO get the matrix from the builder
            _realizations = new List<IEarthModelRealization>(NumberOfRealizations);

            //VAriogram builder initialization
            //TODO fix the parameters
            var nugget = 0.0;
            var sill = deviation;
            var range = (maxX - minX);
            var vBuilder = new VariogramBuilder(_xPositions, new ExpVariogram(nugget, sill, range));
            for (int j = 0; j < NumberOfRealizations; ++j)
            {
                var topPosionAdjustment =
                    rNumGen.NextNormalRandomValueStatic(0.0, deviationDepthOfReservoir);
                double layerAdjustment;
                layerAdjustment =
                    rNumGen.NextNormalRandomValueStatic(0,
                        deviation);
                IList<double> upperB = vBuilder.DrawVectorDifferentMean(bottomOfLayer1 + topPosionAdjustment + layerAdjustment);
                layerAdjustment =
                    rNumGen.NextNormalRandomValueStatic(0,
                    deviation);
                IList<double> lowerB = vBuilder.DrawVectorDifferentMean(topOfLayer1 + topPosionAdjustment + layerAdjustment);
                layerAdjustment =
                    rNumGen.NextNormalRandomValueStatic(0,
                    deviation);
                IList<double> upperB2 = vBuilder.DrawVectorDifferentMean(bottomOfLayer2 + topPosionAdjustment + layerAdjustment);
                layerAdjustment =
                    rNumGen.NextNormalRandomValueStatic(0,
                    deviation);
                IList<double> lowerB2 = vBuilder.DrawVectorDifferentMean(topOfLayer2 + topPosionAdjustment + layerAdjustment);

                _realizations.Add(new EarthModelRealizationMultiLayer(this, upperB, lowerB, upperB2, lowerB2,
                resistivityLayerBottom, resistivityLayerTop, resistivityOutsideLayer));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberOfPoints"></param>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="bottomOfLayer1"></param>
        /// <param name="topOfLayer1"></param>
        /// <param name="bottomOfLayer2"></param>
        /// <param name="topOfLayer2"></param>
        /// <param name="deviation"></param>
        /// <param name="resistivityLayerBottom"></param>
        /// <param name="resistivityLayerTop"></param>
        /// <param name="resistivityOutsideLayer"></param>
        /// <param name="sizeReduction"></param>
        /// <param name="deviationDepthOfReservoir">The deviation in depth that may be highly variable</param>
        public void GenerateRealizationsInclinedTwoLayers(int numberOfPoints, double minX, double maxX,
            double bottomOfLayer1, double topOfLayer1, double bottomOfLayer2, double topOfLayer2, double deviation,
            double resistivityLayerBottom, double resistivityLayerTop, double resistivityOutsideLayer,
            bool sizeReduction = false, double deviationDepthOfReservoir = 0.0)
        {

            // Layer1 is the bottom reservoir layer, Layer2 is the top reservoir layer

            _xPositions = new List<double>(numberOfPoints);
            double pos = minX;
            double deltaX = (maxX - minX) / (numberOfPoints - 1);
            for (int i = 0; i < numberOfPoints; ++i)
            {
                _xPositions.Add(pos);

                pos += deltaX;
            }

            _realizations = new List<IEarthModelRealization>(NumberOfRealizations);
            for (int j = 0; j < NumberOfRealizations; ++j)
            {
                var topPosion =
                    rNumGen.NextNormalRandomValueStatic(0.0, deviationDepthOfReservoir);
                _realizations.Add(GenerateRealizationTwoLayerNaive(numberOfPoints,
                    bottomOfLayer1 + topPosion, topOfLayer1 + topPosion, bottomOfLayer2 + topPosion, topOfLayer2 + topPosion, deviation,
                    resistivityLayerBottom, resistivityLayerTop, resistivityOutsideLayer,
                    sizeReduction));
            }

        }

        public IEarthModelRealization GenerateRealizationTwoLayerNaive(int numberOfPoints,
            double bottomOfLayer1, double topOfLayer1, double bottomOfLayer2, double topOfLayer2,
            double deviation, double resistivityLayerBottom, double resistivityLayerTop, double resistivityOutsideLayer,
            bool sizeReduction = false)
        {
            // Layer1 is the bottom reservoir layer, Layer2 is the top reservoir layer

            IList<double> upperB = new List<double>(numberOfPoints);
            IList<double> lowerB = new List<double>(numberOfPoints);
            IList<double> upperB2 = new List<double>(numberOfPoints);
            IList<double> lowerB2 = new List<double>(numberOfPoints);

            upperB.Add(rNumGen.NextNormalRandomValueStatic(bottomOfLayer1, deviation));
            lowerB.Add(rNumGen.NextNormalRandomValueStatic(topOfLayer1, deviation));
            upperB2.Add(rNumGen.NextNormalRandomValueStatic(bottomOfLayer2, deviation));
            lowerB2.Add(rNumGen.NextNormalRandomValueStatic(topOfLayer2, deviation));

            for (int i = 1; i < numberOfPoints; ++i)
            {
                if (i == 1)
                {
                    upperB.Add(upperB[i - 1] +
                               rNumGen.NextNormalRandomValueStatic(
                                   0.0, 0.1 * deviation));
                    lowerB.Add(lowerB[i - 1] +
                               rNumGen.NextNormalRandomValueStatic(
                                   0.0, 0.1 * deviation));
                    upperB2.Add(upperB2[i - 1] +
                                rNumGen.NextNormalRandomValueStatic(
                                   0.0, 0.1 * deviation));
                    lowerB2.Add(lowerB2[i - 1] +
                                rNumGen.NextNormalRandomValueStatic(
                                   0.0, 0.1 * deviation));

                }
                else
                {
                    var trend = ((upperB[i - 1] - upperB[i - 2]) + (lowerB[i - 1] - lowerB[i - 2])) / 2;
                    double sizeReductionUp = 0.0;
                    double sizeReduction2 = 0.0;
                    if (sizeReduction)
                    {
                        sizeReductionUp = (upperB[i - 1] - lowerB[i - 1]) / numberOfPoints;
                        sizeReduction2 = (upperB2[i - 1] - lowerB2[i - 1]) / numberOfPoints;
                    }
                    var trendTest =
                        Math.Abs(rNumGen.NextNormalRandomValueStatic
                            (0.0, 0.1 * deviation));
                    if (Math.Abs(trend) > trendTest)
                    {
                        trend = Math.Sign(trend) * trendTest;
                    }
                    upperB.Add(upperB[i - 1] + trend +
                               rNumGen.NextNormalRandomValueStatic(
                                   0.0, 0.1 * deviation));
                    lowerB.Add(lowerB[i - 1] +
                               trend +
                               sizeReductionUp +
                               rNumGen.NextNormalRandomValueStatic(
                                   0.0, 0.1 * deviation));
                    upperB2.Add(upperB2[i - 1] + trend +
                                rNumGen.NextNormalRandomValueStatic(
                                   0.0, 0.1 * deviation));
                    lowerB2.Add(lowerB2[i - 1] +
                               trend +
                               sizeReduction2 +
                                rNumGen.NextNormalRandomValueStatic(
                                   0.0, 0.1 * deviation));
                }
            }

            return new EarthModelRealizationMultiLayer(this, upperB, lowerB, upperB2, lowerB2,
                resistivityLayerBottom, resistivityLayerTop, resistivityOutsideLayer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberOfPoints"></param>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="topReservoir"></param>
        /// <param name="topDeviation"></param>
        /// <param name="thicknesses"></param>
        /// <param name="deviationThickness"></param>
        /// <param name="resistivities"></param>
        /// <param name="OWC"></param>
        /// <param name="OWCDeviation"></param>
        /// <param name="resistivityBelowOWC"></param>
        /// <param name="targetLayers"></param>
        /// <param name="considerSpecificTargetGeobodies"></param>
        /// <param name="sizeReduction"></param>
        /// <param name="depthOfReservoirDeviation">The deviation in depth of the entire reservoir (due to uncertainty in velocity model)</param>
        public void GenerateRealizationInclinedMultipleLayers(int numberOfPoints, double minX, double maxX,
            double topReservoir, double topDeviation, List<double> thicknesses, double deviationThickness, List<double> resistivities,
            double OWC, double OWCDeviation, double resistivityBelowOWC,
            IList<int> targetLayers, bool considerSpecificTargetGeobodies,
            bool sizeReduction = false, double depthOfReservoirDeviation = 0.0)
        {
            // Layer1 is the bottom reservoir layer, Layer2 is the top reservoir layer

            _xPositions = new List<double>(numberOfPoints);
            double pos = minX;
            double deltaX = (maxX - minX) / (numberOfPoints - 1);
            for (int i = 0; i < numberOfPoints; ++i)
            {
                _xPositions.Add(pos);

                pos += deltaX;
            }

            _realizations = new List<IEarthModelRealization>(NumberOfRealizations);
            for (int j = 0; j < NumberOfRealizations; ++j)
            {
                var topPosion =
                    rNumGen.NextNormalRandomValueStatic(0.0, depthOfReservoirDeviation);
                _realizations.Add(GenerateRealizationMultipleLayer(numberOfPoints, topReservoir + topPosion, topDeviation,
                    thicknesses, deviationThickness, resistivities, OWC, OWCDeviation, resistivityBelowOWC,
                    targetLayers, considerSpecificTargetGeobodies, sizeReduction));
            }
        }

        /// <summary>
        /// Ordered from top to bottom
        /// </summary>
        /// <param name="numberOfPoints"></param>
        /// <param name="topReservoir"></param>
        /// <param name="topDeviation"></param>
        /// <param name="thicknessLayer"></param>
        /// <param name="deviationThickness"></param>
        /// <param name="resistivities"></param>
        /// <param name="OWC"></param>
        /// <param name="OWCDeviation"></param>
        /// <param name="resistivityBelowOWC"></param>
        /// <param name="targetLayers"></param>
        /// <param name="sizeReduction"></param>
        /// <returns></returns>
        public IEarthModelRealization GenerateRealizationMultipleLayer(int numberOfPoints,
            double topReservoir, double topDeviation, List<double> thicknessLayer, double deviationThickness,
            List<double> resistivities, double OWC, double OWCDeviation, double resistivityBelowOWC,
            IList<int> targetLayers, bool considerSpecificTargetGeobody, bool sizeReduction = false)
        {
            if (thicknessLayer.Count != resistivities.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            int numLayers = thicknessLayer.Count;
            IList<double> topInterface = new List<double>();
            IList<IList<double>> thicknesses = new List<IList<double>>(numLayers);

            for (int i = 0; i < numberOfPoints; i++)
            {
                double deviationRatio = 0.1;    // What is this?

                if (i == 0)
                {
                    // Set up the first point for all stratigraphic interfaces

                    topInterface.Add(rNumGen.NextNormalRandomValueStatic(
                        topReservoir, topDeviation));

                    for (int layerNum = 0; layerNum < numLayers; layerNum++)
                    {
                        IList<double> currThickness = new List<double>(numberOfPoints);

                        // No correllation with the thickness of the layer above
                        currThickness.Add(rNumGen.NextNormalRandomValueStatic(
                            thicknessLayer[layerNum], deviationThickness));
                        thicknesses.Add(currThickness);
                    }
                }
                else if (i == 1)
                {
                    // Set up second point for all stratigraphic interfaces

                    topInterface.Add(topInterface[i - 1] +
                                     rNumGen.NextNormalRandomValueStatic(
                                   0.0, 0.1 * topDeviation));

                    for (int layerNum = 0; layerNum < numLayers; layerNum++)
                    {
                        IList<double> currThickness = thicknesses[layerNum];
                        currThickness.Add(currThickness[i - 1] +
                                          rNumGen.NextNormalRandomValueStatic(
                                       0.0, deviationRatio * deviationThickness));
                    }
                }
                else
                {
                    {
                        // Skip trend test for now - is it necessary at all when using thicknesses?
                        // Trend: reduce angle of steeply dipping surfaces
                        // Calculate trend from the top layer (is that correct?)

                        //List<double> topInterface = stratigraphicInterfaces[0];
                        //List<double> bottomInterface = stratigraphicInterfaces[1];
                        //var trend = ((topInterface[i - 1] - topInterface[i - 2]) + (bottomInterface[i - 1] - bottomInterface[i - 2])) / 2;
                        //var trendTest =
                        //    Math.Abs(MyFunctions.Default.RandomNumberGenerator.RandomNumberGenerator.NextNormalRandomValueStatic
                        //        (0.0, 0.1 * topDeviation));
                        //if (Math.Abs(trend) > trendTest)
                        //{
                        //    trend = Math.Sign(trend) * trendTest;
                        //}
                    }

                    topInterface.Add(topInterface[i - 1] +
                                     rNumGen.NextNormalRandomValueStatic(
                                   0.0, 0.1 * topDeviation));

                    // Adds one point for all interfaces: not optimal (should do one by one interface)
                    for (int layerNum = 0; layerNum < numLayers; layerNum++)
                    {

                        // The code for only two layers calculates a trend for each of the two reservoir layers
                        // Moreover, all layers are handled equally, does not differ between reservoir and non-reservoir
                        // OBS: could do that. Do not know what will happen if I do it the planned way ...
                        // sizeReduction = reduction in thickness in the Sergey's code: but that is normally set to false

                        {
                            // Skip trend test for now - is it necessary at all when using thicknesses?

                            //double sizeReductionUp = 0.0;
                            //double sizeReduction2 = 0.0;
                            //if (sizeReduction)
                            //{
                            //    throw new NotImplementedException();
                            //    sizeReductionUp = (topInterface[i - 1] - bottomInterface[i - 1]) / numberOfPoints;
                            //    sizeReduction2 = (topInterface[i - 1] - bottomInterface[i - 1]) / numberOfPoints;
                            //}
                        }

                        IList<double> currThickness = thicknesses[layerNum];
                        currThickness.Add(currThickness[i - 1] + /* trend */ +
                                              rNumGen.NextNormalRandomValueStatic(
                                       0.0, deviationRatio * deviationThickness));
                    }
                }
            }

            // Check for negative thickness. If negative, just set to zero.  This is what 710 does.
            // Do not know how this affects the uncertainty management ...
            for (int i = 0; i < numLayers; i++)
            {
                IList<double> currThickness = thicknesses[i];
                for (int j = 0; j < numberOfPoints; j++)
                {
                    if (currThickness[j] <= 0.0)
                    {
                        currThickness[j] = 0.0;
                    }
                }
            }

            double OWCRealized = rNumGen.NextNormalRandomValueStatic(
                        OWC, OWCDeviation);

            return new EarthModelRealizationMultipleLayers(this, topInterface, thicknesses,
                resistivities, OWCRealized, resistivityBelowOWC, targetLayers, considerSpecificTargetGeobody);
        }

        //public IEarthModelRealization GenerateRealizationMultipleLayer(int numberOfPoints,
        //    double topReservoir, double topDeviation, List<double> thicknessLayer, double deviationThickness,
        //    List<double> resistivities, bool sizeReduction = false)
        //{
        //    if (thicknessLayer.Count != resistivities.Count)
        //    {
        //        throw new ArgumentOutOfRangeException();
        //    }

        //    int numLayers = thicknessLayer.Count;
        //    List<List<double>> stratigraphicInterfaces = new List<List<double>>(numLayers + 1); // Add one for the reservoir bottom
        //    //for (int layerNum = 0; layerNum < numLayers + 1; layerNum++)
        //    //{
        //    //    stratigraphicInterfaces.Add(new List<double>(numberOfPoints));
        //    //}

        //    for (int i = 0; i < numberOfPoints; i++)
        //    {
        //        if (i == 0)
        //        {
        //            // Set up the first point for all stratigraphic interfaces
        //            for (int interfaceNum = 0; interfaceNum < numLayers + 1; interfaceNum++)
        //            {
        //                List<double> currInterface = new List<double>(numberOfPoints);
        //                if (interfaceNum == 0)
        //                {
        //                    currInterface.Add(MyFunctions.Default.RandomNumberGenerator.RandomNumberGenerator.NextNormalRandomValueStatic(
        //                        topReservoir, topDeviation));
        //                }
        //                else
        //                {
        //                    double topInterfaceAbove = stratigraphicInterfaces[interfaceNum - 1][i];
        //                    currInterface.Add(MyFunctions.Default.RandomNumberGenerator.RandomNumberGenerator.NextNormalRandomValueStatic(
        //                        topInterfaceAbove - thicknessLayer[interfaceNum - 1], deviationThickness));
        //                }
        //                stratigraphicInterfaces.Add(currInterface);
        //            }
        //        }
        //        else if (i == 1)
        //        {
        //            // Set up second point for all stratigraphic interfaces
        //            for (int interfaceNum = 0; interfaceNum < numLayers + 1; interfaceNum++)
        //            {
        //                List<double> currInterface = stratigraphicInterfaces[interfaceNum];
        //                if (interfaceNum == 0)
        //                {
        //                    currInterface.Add(currInterface[i - 1] +
        //                               MyFunctions.Default.RandomNumberGenerator.RandomNumberGenerator.NextNormalRandomValueStatic(
        //                                   0.0, 0.1 * topDeviation));
        //                }
        //                else
        //                {
        //                    double pointInterfaceAbove = stratigraphicInterfaces[interfaceNum - 1][i];
        //                    double pointAboveThicknessAdjusted = pointInterfaceAbove - thicknessLayer[interfaceNum - 1];
        //                    // No uncertainty in thickness: just add the point
        //                    currInterface.Add(pointAboveThicknessAdjusted);
        //                    // TODO: check if the part below should be implemented
        //                    // If uncertainty in thickness: 1) we risk overprinting, 2) need to check how it is handled vs. horizontal trends
        //                    //currInterface.Add(currInterface[pointNum - 1] +
        //                    //           MyFunctions.Default.RandomNumberGenerator.RandomNumberGenerator.NextNormalRandomValueStatic(
        //                    //               0.0, 0.1 * deviation));
        //                }
        //            }
        //        }
        //        else
        //        {
        //            // TODO: check this: how the trend should be calculated for the layers below the top

        //            // Trend: reduce angle of steeply dipping surfaces
        //            // Calculate trend from the top layer (is that correct?)
        //            List<double> topInterface = stratigraphicInterfaces[0];
        //            List<double> bottomInterface = stratigraphicInterfaces[1];
        //            var trend = ((topInterface[i - 1] - topInterface[i - 2]) + (bottomInterface[i - 1] - bottomInterface[i - 2])) / 2;
        //            var trendTest =
        //                Math.Abs(MyFunctions.Default.RandomNumberGenerator.RandomNumberGenerator.NextNormalRandomValueStatic
        //                    (0.0, 0.1 * topDeviation));
        //            if (Math.Abs(trend) > trendTest)
        //            {
        //                trend = Math.Sign(trend) * trendTest;
        //            }

        //            // Adds one point for all interfaces: not optimal (should do one by one interface)
        //            for (int interfaceNum = 1; interfaceNum < numLayers + 1; interfaceNum++)
        //            {

        //                // The code for only two layers calculates a trend for each of the two reservoir layers
        //                // Here we simple let thickness be constant: no uncertainty except in top layer
        //                // Moreover, all layers are handled equally, does not differ between reservoir and non-reservoir
        //                // OBS: could do that. Do not know what will happen if I do it the planned way ...
        //                // sizeReduction = reduction in thickness in the Sergey's code: but that is normally set to false
        //                // trendTest: only a modification of the sign?
        //                // TODO: do this trend test and update successively for each layer in downwards direction. See what happens. Easy to implement.


        //                // TODO as suggested on the previous line
        //                double sizeReductionUp = 0.0;
        //                double sizeReduction2 = 0.0;
        //                if (sizeReduction)
        //                {
        //                    throw new NotImplementedException();
        //                    sizeReductionUp = (topInterface[i - 1] - bottomInterface[i - 1]) / numberOfPoints;
        //                    sizeReduction2 = (topInterface[i - 1] - bottomInterface[i - 1]) / numberOfPoints;
        //                }

        //                // TODO: the last one of the interfaces is not updated
        //                List<double> interfaceBoundary = stratigraphicInterfaces[interfaceNum - 1];
        //                interfaceBoundary.Add(interfaceBoundary[i - 1] + trend +
        //                           MyFunctions.Default.RandomNumberGenerator.RandomNumberGenerator.NextNormalRandomValueStatic(
        //                               0.0, 0.1 * topDeviation));
        //            }
        //        }
        //    }

        //    int aa = 0;
        //    //throw new NotImplementedException();
        //    return null;

        //    // TODO: check for negative thickness. If negative, just set to zero.  This is what 710 does.

        //    // TODO: should take thicknesses as input. And model with THICKNESSES above here.
        //    // The stratigraphic interfaces are only for visualization.
        //    //return new EarthModelRealizationMultipleLayers(this, reservoirTop, thicknessLayers, resistivityPerLayer);
        //}

        public void GenerateRealizationsInclined(int numberOfPoints, double minX, double maxX,
            double upper, double lower, double deviation)
        {
            _xPositions = new List<double>(numberOfPoints);
            double pos = minX;
            double deltaX = (maxX - minX) / (numberOfPoints - 1);
            for (int i = 0; i < numberOfPoints; ++i)
            {
                _xPositions.Add(pos);

                pos += deltaX;
            }

            _realizations = new List<IEarthModelRealization>(NumberOfRealizations);
            for (int j = 0; j < NumberOfRealizations; ++j)
            {
                IList<double> upperB = new List<double>(numberOfPoints);
                IList<double> lowerB = new List<double>(numberOfPoints);

                upperB.Add(rNumGen.NextNormalRandomValueStatic(upper, deviation));
                lowerB.Add(rNumGen.NextNormalRandomValueStatic(lower, deviation));
                //upper = upperB.Last();
                //lower = lowerB.Last();
                for (int i = 1; i < numberOfPoints; ++i)
                {
                    if (i == 1)
                    {
                        upperB.Add(upperB[i - 1] +
                                   rNumGen.NextNormalRandomValueStatic(
                                       0.0, 0.1 * deviation));
                        lowerB.Add(lowerB[i - 1] +
                                   rNumGen.NextNormalRandomValueStatic(
                                       0.0, 0.1 * deviation));
                    }
                    else
                    {
                        var trend = ((upperB[i - 1] - upperB[i - 2]) + (lowerB[i - 1] - lowerB[i - 2])) / 2;
                        var trendTest =
                            Math.Abs(rNumGen.NextNormalRandomValueStatic
                                (0.0, 0.1 * deviation));
                        if (Math.Abs(trend) > trendTest)
                        {
                            trend = Math.Sign(trend) * trendTest;
                        }
                        upperB.Add(upperB[i - 1] + trend +
                                   rNumGen.NextNormalRandomValueStatic(
                                       0.0, 0.1 * deviation));
                        lowerB.Add(lowerB[i - 1] +
                                   trend +
                                   rNumGen.NextNormalRandomValueStatic(
                                       0.0, 0.1 * deviation));
                    }
                }

                double resistivityInLayer = 250;
                double resistivityOutSideLayer = 50;
                _realizations.Add(new EarthModelRelization(this, upperB, lowerB, resistivityInLayer, resistivityOutSideLayer));
            }
        }

        /// <summary>
        /// Generates "flat" realizations
        /// </summary>
        /// <param name="numberOfPoints"></param>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="upper"></param>
        /// <param name="lower"></param>
        /// <param name="deviation"></param>
        public void GenerateRealizations(int numberOfPoints, double minX, double maxX, double upper, double lower, double deviation)
        {
            _xPositions = new List<double>(numberOfPoints);
            double pos = minX;
            double deltaX = (maxX - minX) / (numberOfPoints - 1);
            for (int i = 0; i < numberOfPoints; ++i)
            {
                _xPositions.Add(pos);

                pos += deltaX;
            }

            _realizations = new List<IEarthModelRealization>(NumberOfRealizations);
            for (int j = 0; j < NumberOfRealizations; ++j)
            {
                IList<double> upperB = new List<double>(numberOfPoints);
                IList<double> lowerB = new List<double>(numberOfPoints);

                upperB.Add(rNumGen.NextNormalRandomValueStatic(upper, deviation));
                lowerB.Add(rNumGen.NextNormalRandomValueStatic(lower, deviation));
                for (int i = 1; i < numberOfPoints; ++i)
                {
                    upperB.Add(upperB[i - 1] + rNumGen.NextNormalRandomValueStatic(0.0, 0.1 * deviation));
                    lowerB.Add(lowerB[i - 1] + rNumGen.NextNormalRandomValueStatic(0.0, 0.1 * deviation));
                }

                double resistivityInLayer = 250;
                double resistivityOutSideLayer = 50;

                _realizations.Add(new EarthModelRelization(this, upperB, lowerB, resistivityInLayer, resistivityOutSideLayer));
            }
        }

        public int NumberOfRealizations
        {
            get
            {
                // return 30;
                // return 3;
                return _numRealizations;
            }

        }

        public IList<IEarthModelRealization> Realizations
        {
            get
            {
                return _realizations;
            }
        }


    }
}
