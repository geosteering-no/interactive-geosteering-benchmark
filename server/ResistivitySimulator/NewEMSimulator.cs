using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnKFLib;
using MathNet.Numerics.LinearAlgebra.Double;
using TrajectoryInterfaces;
using TrajectoryOptimization;


namespace ResistivitySimulator
{
    public class NewEMSimulator : IDataGenerator<IResistivityModel, IContinousState, ResistivityData2DFull>
    {
        private readonly double instrumentSize;
        private readonly double deltaX;
        private readonly double alphaCoefficient;

        private const int RESOLUTION_RELATIVE_TO_INTRUMENT = 40;
        private const double CUT_OFF = 1e-2;
        private const double EPS = 1E-7;


        public NewEMSimulator(double instrumentSize, double deltaX = 0)
        {
            if (deltaX == 0)
            {
                this.deltaX = instrumentSize / RESOLUTION_RELATIVE_TO_INTRUMENT;
            }
            else
            {
                this.deltaX = deltaX;
            }
            this.instrumentSize = instrumentSize;
            alphaCoefficient = 1.0 / EvaluateFunctionInDirection(ReturnOne, new ContinousState(0, 0), 1);
        }

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

        private double ReturnOne(double x, double y)
        {
            return 1;
        }

        private double EvaluateFunctionInDirection(GetResistivity getResistivity, IContinousState position, int direction)
        {

            if (direction == 0)
            {
                return getResistivity(position.X, position.Y)/alphaCoefficient;
            }
            //loop for distances
            double sum = 0;
            for (double r = deltaX; ; r += deltaX)
            {
                //loop in between measurment points
                double max = 0;
                for (double x = -instrumentSize / 2.0; x < instrumentSize / 2.0 + EPS; x += deltaX)
                {
                    double curG = GetWeight(r, x);
                    if (curG > max)
                    {
                        max = curG;
                    }
                    sum += curG * getResistivity(position.X + x, position.Y + r * direction);
                }

                if (max < CUT_OFF)
                {
                    break;
                }

                //go left and right
                for (double x = -instrumentSize / 2.0 - deltaX; ; x -= deltaX)
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

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        //private double EvaluateInDirection(Point2D<double, DoubleCalculator> position, double direction)
        //{
        //    return EvaluateFunctionInDirection(model.GetResistivity, position, direction) * alphaCoefficient;
        //}

        #endregion

        public IList<ResistivityData2DFull> GenerateData(IResistivityModel m, IList<IData<IContinousState, ResistivityData2DFull>> ps)
        {
            IList<ResistivityData2DFull> result = new List<ResistivityData2DFull>(ps.Count);
            foreach (var dataPoint in ps)
            {
                ResistivityData2DFull res = new ResistivityData2DFull();
                for (int direction = -1; direction <= 1; ++direction)
                {
                    res[direction] = EvaluateFunctionInDirection(m.GetResistivity, dataPoint.Parameter, direction) * alphaCoefficient;
                }
                result.Add(res);
            }
            return result;
        }
    }
}
