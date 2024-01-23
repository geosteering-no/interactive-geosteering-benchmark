using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ResistivitySimulator
{
    public class EarthModel2Fault : IResistivityModel
    {
        //faultY0 is always set to 0
        private static MathNet.Numerics.LinearAlgebra.VectorBuilder<double> _vBuild =
            MathNet.Numerics.LinearAlgebra.Vector<double>.Build;
        Fault[] faults;
        int[] permutation;
        bool sorted = false;
        double layerThickness;
        double resistivityInLayer;
        double resistivityOutsideLayer;



        public EarthModel2Fault(double layerH, double resistIn, double resistOut, Fault[] faults)
        {
            this.layerThickness = layerH;
            this.resistivityInLayer = resistIn;
            this.resistivityOutsideLayer = resistOut;
            this.faults = faults;
        }


        public EarthModel2Fault(double layerH, double resistIn, double resistOut, int numberOfFaults)
        {
            this.layerThickness = layerH;
            this.resistivityInLayer = resistIn;
            this.resistivityOutsideLayer = resistOut;
            faults = new Fault[numberOfFaults];
            for (int i = 0; i < numberOfFaults; ++i)
            {
                faults[i] = new Fault();
            }
        }

        /// <summary>
        /// Model vector has length 3 and consists of:
        /// x0 - position of the fault
        /// heave - positive number for = x1-x0
        /// throw - positive when directed upwards = y1-yo
        /// </summary>
        public Vector Vector
        {
            get {
                IList<double> x = new List<double>() ;
                foreach (Fault f in faults)
                {
                    Vector v = f.vector;
                    for (int i = 0; i < v.Count; ++i)
                    {
                        x.Add(v[i]);
                    }
                }

                return (Vector) _vBuild.DenseOfEnumerable(x);
            }
            set {
                //TODO add size check
                int fSize = Fault.VectorSize;
                int ind = 0;
                for (int fInd = 0; fInd<faults.Length; ++fInd)
                {
                    Vector v = new DenseVector(fSize);
                    for (int i = 0; i < fSize; ++i)
                    {
                        v[i] = value[ind];
                        ++ind;
                    }
                    faults[fInd].vector = v;
                }
                sorted = false;
            }
        }


        #region generatingFaultOrder
        private void swapIndeces(int i, int j)
        {
            int tmp = permutation[i];
            permutation[i] = permutation[j];
            permutation[j] = tmp;
        }

        private double faultOrder(int i)
        {
            return faults[permutation[i]].faultingTime;
        }

        private void ComputePermutation()
        {
            int n = faults.Length;
            permutation = new int[n];
            for (int i = 0; i < n; ++i)
            {
                permutation[i] = i;
            }

            //some sorting method e.g. bubble 
            //optimized taken from wikipedia page
            int m = n;
            do
            {
                int newn = 0;
                for (int i = 1; i < m; ++i)
                {
                    if (faultOrder(i - 1) > faultOrder(i))
                    {
                        swapIndeces(i - 1, i);
                        newn = i;
                    }
                }
                m = newn;
            } while (m > 0);
            sorted = true;
        }
        #endregion

        public Fault GetFaultByTimeIndex(int ind)
        {
            if (!sorted)
            {
                ComputePermutation();
            }
            return faults[permutation[faults.Length - 1 - ind]];
        }

        /// <summary>
        /// This returns resistivity in the current point
        /// </summary>
        /// <param name="x">directed right</param>
        /// <param name="y">directed up</param>
        /// <returns></returns>
        public double GetResistivity(double x, double y)
        {
            if (!sorted)
            {
                ComputePermutation();
            }
            int n = faults.Length;
            double shiftX = 0;
            double shiftY = 0;

            for (int i = 0; i < n; ++i)
            {
                //determine if we are on the left of the line
                //find direwction to the point
                
                Fault f = faults[permutation[i]];
                //Fault f = faults[i];

                //check for existence
                if (!f.FaultExists)
                {
                    continue;
                }


                double startX = f.faultStartX;
                double startY = f.faultStartY;
                shiftX = 0;
                shiftY = 0;

                //for (int j = 0; j < i; ++j)
                //{
                //    Fault oldF = faults[permutation[j]];
                //    if (oldF.ToTheRight(startX, startY, shiftX, shiftY) > 0)
                //    {
                //        shiftX -= oldF.faultHeave;
                //        shiftY -= oldF.faultThrow;
                //    }
                //}

                int toTheRight = f.ToTheRight(x, y, shiftX, shiftY);

                if (toTheRight == 0)
                {
                    return (resistivityInLayer + resistivityOutsideLayer) / 2;
                }

                if (toTheRight > 0)
                {
                    //height edjustment
                    y -= f.faultThrow;

                    //x adjustment
                    x -= f.faultHeave;

                    //TODO check this
                }

            }
            if (y >= 0  && y <= layerThickness)
            {
                return resistivityInLayer;
            }
            else
            {
                return resistivityOutsideLayer;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public double GetResistivity(Point2DDouble position)
        {
            //Vector = modelVector;
            return GetResistivity(position.X, position.Y);
        }

        //public IList<Pair<double,double>> GetXYValuesAsPairs()
        //{
        //    IList<Pair<double,double>> list = new List<Pair<double,double>>();
        //    list.Add(new Pair<double,double>(0,0));
        //    list.Add(new Pair<double,double>(faultX0,0));
        //    list.Add(new Pair<double,double>(faultX0 + faultHeave,faultThrow));
        //    list.Add(new Pair<double,double>(MAX_X,faultThrow));
        //    return list;
        //}




        public int VectorSize
        {
            get {
                return faults.Length * Fault.VectorSize;
            }
        }
    }
}
