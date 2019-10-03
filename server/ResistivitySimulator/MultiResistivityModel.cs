using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;
using TrajectoryInterfaces;

namespace ResistivitySimulator
{
    public class MultiResistivityModel
    {
        //private IList<IContinousState> positions;
        private readonly IResistivityModel resestivityModel;
        private readonly double instrumentSize;
        private readonly double deltaX;
        private readonly double alphaCoefficient;

        private const int RESOLUTION_RELATIVE_TO_INTRUMENT = 40;
        private const double CUT_OFF = 1e-2;
        private const double EPS = 1E-7; 

        public MultiResistivityModel(IResistivityModel eModel, double instrumentSize)
        {
            this.resestivityModel = eModel;
            this.instrumentSize = instrumentSize;
            deltaX = instrumentSize / RESOLUTION_RELATIVE_TO_INTRUMENT;
            alphaCoefficient = 1.0 / EvaluateOne();
        }

        private double EvaluateOne()
        {
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
                    if (curG < CUT_OFF)
                    {
                        continue;
                    }
                    sum += curG * 1.0;
                }



                //go left and right
                for (double x = -instrumentSize / 2.0 - deltaX; ; x -= deltaX)
                {
                    double curG = GetWeight(r, x);
                    if (curG < CUT_OFF)
                    {
                        break;
                    }
                    sum += curG * 1.0;
                    sum += curG * 1.0;
                }

                //check for exit
                if (max < CUT_OFF)
                {
                    break;
                }
            }
            return sum;
        }

        //public IList<IContinousState> Position
        //{
        //    get { return positions; }
        //    set { positions = value; }
        //}

        public static int GetDataSize(int positionsCount)
        {
            return  directionCount*positionsCount;
        }



        //TODO consider putting evaluation to sub-classes
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

        private Vector EvaluateFunctionInDirection(GetResistivity getResistivity, double direction, IList<IContinousState> positions)
        {
            Vector res = new DenseVector(positions.Count);
            int ind = 0;
            foreach (var position in positions)
            {
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
                        if (curG < CUT_OFF)
                        {
                            continue;
                        }
                        sum += curG * getResistivity(position.X + x, position.Y + r * direction);
                        //LastEvaluatedPoints.Add(new PointF((float) (position.X + x), (float) (position.Y + r*direction)));
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
                        //LastEvaluatedPoints.Add(new PointF((float) (position.X + x), (float) (position.Y + r * direction)));
                        sum += curG * getResistivity(position.X - x, position.Y + r * direction);
                        //LastEvaluatedPoints.Add(new PointF((float) (position.X - x), (float) (position.Y + r * direction)));
                    }
                    //check for exit
                    if (max < CUT_OFF)
                    {
                        break;
                    }

                }
                res[ind] = sum;
                ind++;
                //return sum;
            }

            return res;
        }

        public double GetModelResistivity(double x, double y)
        {
            return resestivityModel.GetResistivity(x, y);
        }

        private Vector EvaluateInDirection(double direction, IList<IContinousState> positions)
        {
            return (Vector) EvaluateFunctionInDirection(resestivityModel.GetResistivity, direction, positions)
                .Multiply(alphaCoefficient);
        }

        #endregion

        const int directionCount = 3;

        //NOTE!!! removed because itt was crashing in concurrent situation
        /// <summary>
        /// Gets the list of points used in the last update to show depth of invetigation
        /// </summary>
        //public HashSet<PointF> LastEvaluatedPoints { get; private set; } = new HashSet<PointF>();

        /// <summary>
        /// This should be the interface to EnKF
        /// Generates data for current position
        /// measurement to the side
        /// measurement up
        /// measurement down
        /// </summary>
        /// <param name="modelVector"></param>
        /// <param name="res">result vector</param>
        /// <returns></returns>
        public Vector ModelToData(IList<IContinousState> positions, Vector res = null)
        {
            if (res == null)
            {
                res = new DenseVector(GetDataSize(positions.Count));
            }
            //LastEvaluatedPoints = new HashSet<PointF>();
            //resestivityModel.Vector = modelVector;

            for (int i = 0; i < directionCount; i++)
            {
                //would be -1; 0; 1
                Vector tmp = EvaluateInDirection(i-1, positions);
                for (int j = 0; j < positions.Count; ++j)
                {
                    res[j*directionCount + i] = tmp[j];
                }
            }



            return res;
        }



        /// <summary>
        /// in principle should be moved to resistivity resestivityModel
        /// </summary>
        /// <param name="mult"></param>
        /// <returns></returns>
        public Matrix GetDataCovarience(double mult, IList<IContinousState> positions)
        {
            var dataSize = GetDataSize(positions.Count);
            var mat = new DenseMatrix(dataSize, dataSize);
            for (int i = 0; i < directionCount; i++)
            {
                double cur = 2*mult;
                //would be -1; 0; 1
                for (int j = positions.Count - 1; j >=0 ; --j)
                {
                    int index = j * directionCount + i;
                    mat[index,index] = cur;
                    cur *= 2;
                }
            }
            return mat;

        }
    }
}
