using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ResistivitySimulator
{
    public class ResistivityModel
    {
        private Point2DDouble position;
        private IResistivityModel model;
        private double instrumentSize;
        private double deltaX;
        private double alphaCoefficient;

        private const int RESOLUTION_RELATIVE_TO_INTRUMENT = 40;
        private const double CUT_OFF = 1e-2;
        private const double EPS = 1E-7; 

        public ResistivityModel(IResistivityModel eModel, double instrumentSize)
        {
            this.model = eModel;
            this.instrumentSize = instrumentSize;
            deltaX = instrumentSize / RESOLUTION_RELATIVE_TO_INTRUMENT;
            ComputeAlphaCoefficient();
        }

        private double ReturnOne(double x, double y)
        {
            return 1;
        }

        private void ComputeAlphaCoefficient()
        {
            alphaCoefficient = 1.0 / EvaluateFunctionInDirection(ReturnOne, 1);
        }

        public Point2DDouble Position
        {
            get { return position; }
            set { position = value; }
        }

        public static int DataSize
        {
            get
            {
                return 2;
            }
        }



        //TODO consider putting avaluation to sub-classes
        #region dataEvaluationImplementation

        public static double Hypot(double a, double b)
        {
            return Math.Sqrt(a * a + b * b);
        }

        private delegate double GetResistivity(double x, double y);

        private double GetWeight(double r, double x)
        {
            double root1 = Hypot(r, instrumentSize / 2.0 - x);
            double root2 = Hypot(r, instrumentSize / 2.0 + x);
            double res = instrumentSize / 2.0 * Math.Pow(r / root1 / root2, 3);
            return res;
        }

        private double EvaluateFunctionInDirection( GetResistivity getResistivity, double direction)
        {
            //loop for distances
            double sum = 0;
            for (double r = deltaX; ; r += deltaX)
            {
                //loop in between measurment points
                double max = 0;
                for (double x = - instrumentSize / 2.0; x < instrumentSize / 2.0 + EPS; x += deltaX)
                {
                    double curG = GetWeight(r, x);
                    if (curG > max)
                    {
                        max = curG;
                    }
                    sum += curG * getResistivity(position.X + x, position.Y + r*direction);
                }

                if (max < CUT_OFF)
                {
                    break;
                }

                //go left and right
                for (double x =  - instrumentSize / 2.0 - deltaX; ;x-=deltaX)
                {
                    double curG = GetWeight(r, x);
                    if (curG < CUT_OFF)
                    {
                        break;
                    }
                    sum += curG * getResistivity(position.X + x, position.Y + r * direction);
                    sum += curG * getResistivity(position.X - x, position.Y + r * direction);
                }
            }
            return sum;
        }

        public double GetModelResistivity(Vector modelVector, double x, double y)
        {
            model.Vector = modelVector;
            return model.GetResistivity(x, y);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private double EvaluateInDirection(double direction)
        {
            return EvaluateFunctionInDirection(model.GetResistivity, direction)*alphaCoefficient;
        }

        #endregion

        /// <summary>
        /// This should be the interface to EnKF
        /// Generates data for current position
        /// measurement to the side
        /// measurement up
        /// measurement down
        /// </summary>
        /// <param name="modelVector"></param>
        /// <param name="modelVector">result vector</param>
        /// <returns></returns>
        public Vector ModelToData(Vector modelVector, Vector res = null)
        {
            if (res == null)
            {
                res = new DenseVector(DataSize);
            }
            model.Vector = modelVector;

            //to the side would be equal to current resistivity
            //res[0] = model.GetResistivity(position);
            //up
            res[0] = EvaluateInDirection(1);
            //down
            res[1] = EvaluateInDirection(-1);
            //the other sige
            //res[3] = res[0];
            //average
            //res[3] = (res[1] + res[2])/2.0;

            return res;
            //throw new NotImplementedException();
        }
    }
}
