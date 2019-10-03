using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ResistivitySimulator
{
    public class Fault
    {
        public double faultHeave = 0 , faultThrow = 0;

        const double FAULT_THICKNESS = 0.1;

        public double FaultDisplacement
        {
            get
            {
                double sumSq = faultThrow * faultThrow + faultHeave * faultHeave;
                return System.Math.Sqrt(sumSq);
            }
            set
            {
                double oldDisp = FaultDisplacement;
                if (oldDisp == 0)
                {
                    return;
                }
                double mult = value / oldDisp;
                faultHeave *= mult;
                faultThrow *= mult;
            }
        }

        /// <summary>
        /// start is the upper point
        /// </summary>
        public double faultStartX = 0, faultStartY = 0;
        public double faultingTime = 0;
        public double faultThickness = FAULT_THICKNESS;

        public Fault(double x, double y, double fHeave, double xThrow, double faultingTime = 0, double faultThickness = FAULT_THICKNESS)
        {
            this.faultStartX = x;
            this.faultStartY = y;
            this.faultHeave = fHeave;
            this.faultThrow = xThrow;
            this.faultingTime = faultingTime;
            this.faultThickness = faultThickness;
        }

        public Fault()
        {
        }

        public bool FaultExists
        {
            get
            {
                return faultThickness > 0;
            }
            set
            {
                faultThickness = FAULT_THICKNESS;
            }
        }

        public double TruncatedFaultThickness
        {
            get
            {
                return System.Math.Max(faultThickness, 0.0);
            }
        }

        /// <summary>
        /// 1 - to the right
        /// -1 to the left
        /// 0 hits the fault
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int ToTheRight(double x, double y, double xShift = 0, double yShift = 0)
        {
            double dx = x - faultStartX - xShift;
            double dy = y - faultStartY - yShift;
            double crossDFaultZ = dx * faultThrow - dy * faultHeave;

            //TODO think of adding faultThickness - does not work easily
            if (System.Math.Abs(crossDFaultZ) < FAULT_THICKNESS * FaultDisplacement)
            {
                return 0;
            }

            bool toTheRight = crossDFaultZ * System.Math.Sign(faultThrow) > 0;
            if (toTheRight)
            {
                return 1;
            }
            else
            {
                return -1;
            }

        }

        public static int VectorSize
        {
            get
            {
                return 6;
            }
        }

        public Vector vector
        {
            get
            {
                DenseVector v = new DenseVector(VectorSize);
                v[0] = faultStartX;
                v[1] = faultStartY;
                v[2] = faultHeave;
                v[3] = faultThrow;
                v[4] = faultingTime;
                v[5] = faultThickness;
                return v;
            }
            set
            {
                if (value.Count != VectorSize)
                {
                    throw new IndexOutOfRangeException("Dimension mismatch");
                }
                faultStartX = value[0];
                faultStartY = value[1];
                faultHeave = value[2];
                faultThrow = value[3];
                faultingTime = value[4];
                faultThickness = value[5];
            }
        }
    }
}
