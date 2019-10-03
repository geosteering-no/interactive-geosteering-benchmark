using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ResistivitySimulator
{
    /// <summary>
    /// 
    /// </summary>
    public class EarthModelRelization : IEarthModelRealization
    {
        
        bool consistent;
        private EarthModelManipulator _parent;
        private IList<double> _upperBoundary;
        private IList<double> _lowerBoundary;
        //measured in depth!
        private IList<double> _resistivityData;
        private double _resistivityInLayer;
        private double _resistivityOutsideLayer;
        private List<Fault> _faults;
        //private IValueModel _valueModel;

        public EarthModelRelization(EarthModelManipulator parent, IList<double> up, IList<double> low, double resIn, double resOut)
        {
            _parent = parent;
            _upperBoundary = up;
            _lowerBoundary = low;
            _resistivityInLayer = resIn;
            _resistivityOutsideLayer = resOut;
            _faults = new List<Fault>();
            //_valueModel = new InReservoirValueModel(this);
        }

        public EarthModelRelization(EarthModelManipulator parent, EarthModelRelization original)
        {
            _parent = parent;
            _upperBoundary = original._upperBoundary.ToArray<double>();
            _lowerBoundary = original._lowerBoundary.ToArray<double>();
            _resistivityInLayer = original._resistivityInLayer;
            _resistivityOutsideLayer = original._resistivityOutsideLayer;
            _faults = new List<Fault>();
            foreach (Fault f in original._faults)
            {
                Fault newFault = new Fault();
                newFault.vector = f.vector;
                _faults.Add(newFault);
            }
            //_valueModel = new InReservoirValueModel(this);
        }
        
        /// <summary>
        /// Vector representation of a model
        /// </summary>
        public Vector Vector
        {
            get
            {
                List<double> output = new List<double>(VectorSize);
                output.AddRange(_upperBoundary);
                output.AddRange(_lowerBoundary);
                foreach (Fault f in _faults)
                {
                    Vector fault = f.vector;
                    output.AddRange(fault.ToArray());
                    //foreach (VectorEntry entry in fault)
                    //{
                    //    output.Add(entry.get());
                    //}
                }
                return new DenseVector(output.ToArray());
            }
            set
            {
                int ind = 0;
                for (int i = 0; i < _upperBoundary.Count; ++i, ++ind)
                {
                    _upperBoundary[i] = value[ind];
                }
                for (int i = 0; i < _lowerBoundary.Count; ++i, ++ind)
                {
                    _lowerBoundary[i] = value[ind];
                }

                foreach (Fault f in _faults)
                {
                    Vector fVector = f.vector;
                    for (var index = 0; index < fVector.Count; index++)
                    {
                        fVector[index] = value[ind];
                        ind++;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int VectorSize
        {
            get
            {
                return _lowerBoundary.Count + _upperBoundary.Count + _faults.Count * Fault.VectorSize;
                //TODO consider rewriting
            }
        }

//        /// <summary>
//        /// 
//        /// </summary>
//        public IValueModel ValueModel
//        {
//            get
//            {
//                return _valueModel;
//            }
//            set
//            {
//                if (value != null)
//                {
//                    _valueModel = value;
//                }
//            }
//        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public virtual double GetResistivity(Point2DDouble position)
        {
            return GetResistivity(position.X, position.Y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public double GetResistivity(double x, double z)
        {

            if (NumberOfFaults > 0)
            {
                int n = NumberOfFaults;
                //now assuming faults are sorted
                for (int i = 0; i < n; ++i)
                {
                    //check whether it is influenced by the fault

                    Fault f = _faults[i];

                    //check for existence
                    if (!f.FaultExists)
                    {
                        continue;
                    }


                    int toTheRight = f.ToTheRight(x, z);

                    if (toTheRight == 0)
                    {
                        return (_resistivityInLayer + _resistivityOutsideLayer) / 2;
                    }

                    if (toTheRight > 0)
                    {
                        //height edjustment
                        z -= f.faultThrow;

                        //x adjustment
                        x -= f.faultHeave;

                        //TODO check this
                    }


                }
            }

            //TODO implement
            int indexPrev = _parent.IndexOfX(x);
            double len = _parent.IntervalLength(indexPrev);
            x -= _parent.IntervalStart(indexPrev);
            double lambda = x / len;
            //get value for mid point
            double zTop = _upperBoundary[indexPrev] * (1 - lambda) + _upperBoundary[indexPrev + 1] * lambda;
            double zBot = _lowerBoundary[indexPrev] * (1 - lambda) + _lowerBoundary[indexPrev + 1] * lambda;
            //T

            if (z >= zTop && z <= zBot)
            {
                return _resistivityInLayer;
            }
            else
            {
                return _resistivityOutsideLayer;
            }

        }

        public IList<double> GetXs()
        {
            return _parent.XPositions;
        }

        public IList<IList<double>> GetBoundaryLists()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        public void AddFault(Fault f)
        {
            _faults.Add(f);
        }

        /// <summary>
        /// 
        /// </summary>
        public int NumberOfFaults
        {
            get
            {
                return _faults.Count;
            }
        }

    }
}
