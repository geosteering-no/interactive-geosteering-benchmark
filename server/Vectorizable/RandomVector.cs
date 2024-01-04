using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Vectorizable
{
    public class RandomVector 
    {

        static MatrixBuilder<double> _mBuild = Matrix<double>.Build;
        static VectorBuilder<double> _vBuild = Vector<double>.Build;

        //private Vector mean;
        public Vector Deviation
        {
            get { return new DenseVector(deviation); }
            private set
            {
                deviation = value.ToArray();
                rowCovarience = (Matrix) _mBuild.DiagonalOfDiagonalVector(value);
                mean = (Matrix) _mBuild.Sparse(value.Count, 1);
                columnCovarience1 = (Matrix) _mBuild.SparseDiagonal(value.Count, 1.0);
            }
        }
        private Matrix mean;
        private Matrix rowCovarience;
        private Matrix columnCovarience1;
        private double[] deviation;

        private Random _rnd = new Random(0);
        //private RandomNumberGenerator myRnd = new RandomNumberGenerator(0);

        public RandomVector(Vector deviation)
        {
            this.Deviation = deviation;
            //RandomNumberGenerator myRnd = new RandomNumberGenerator();
        }

        public RandomVector(Vector deviation, int randomSeed)
        {
            this.Deviation = deviation;
            _rnd = new Random(randomSeed);
        }
       

        //public override void set(int index, double value)
        //{
        //    deviation.set(index, value);
        //}

        //public override double get(int index)
        //{
        //    return myRnd.NextNormalRandomValue(0, deviation.get(index));
        //}

        public Vector Realize()
        {
            //var matrixResulr = MatrixNormal.Sample(_rnd, mean, rowCovarience, columnCovarience1);
            //TODO check if transpose is required/
            //return (Vector)matrixResulr.Column(0);
            double[] array = new double[deviation.Length];
            for (var i = 0; i < deviation.Length; ++i)
            {
                array[i] = Normal.Sample(_rnd, 0.0, deviation[i]);
            }

            return (Vector)_vBuild.DenseOfArray(array);

        }


    }
}
