using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Random;


namespace Variogram
{
    public class VariogramBuilder
    {
        private Matrix _covarience;
        private Matrix _covarience2;
        private readonly MatrixNormal _mNormal;
        private readonly IVariogram _variogram;
        private MatrixBuilder<double> _mBuilder;
        private readonly double _meanValue;
        private readonly int _size;

        public int Size
        {
            get { return _size; }
        }


        private void PreInit()
        {
            _mBuilder = Matrix.Build;

        }


        /// <summary>
        /// Distance between two points of type T
        /// </summary>
        /// <typeparam name="T">Point or vector type</typeparam>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public delegate double Distance<T>(T p1, T p2);
        
        //TODO implement for vectors


        private double DoubleDistance(double p1, double p2)
        {
            return Math.Abs(p1 - p2);
        }

        private void BuildCovarience<T>(IList<T> xValues, Distance<T> dist)
        {
            _covarience2 = (Matrix) _mBuilder.Dense(1, 1, 1.0);

            var n = xValues.Count;

            _covarience = (Matrix)_mBuilder.Dense(n, n);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    _covarience[i, j] = _variogram.Sill - _variogram.GetValue(dist(xValues[i],  xValues[j]));
                }
            }
        }

        /// <summary>
        /// Creates a variogram builder
        /// </summary>
        /// <param name="xValues">The points in space</param>
        /// <param name="variogram">IVariogram class to use</param>
        /// <param name="meanValue">Mean value to use. Will be zero when not specified.</param>
        public VariogramBuilder(IList<double> xValues, IVariogram variogram, double meanValue = 0.0)
        {
            PreInit();
            _meanValue = meanValue;
            _size = xValues.Count;
            var mean = (Matrix)_mBuilder.Dense(_size, 1, _meanValue);
            _variogram = variogram;
            BuildCovarience(xValues, DoubleDistance);
            _mNormal = new MatrixNormal(mean, _covarience, _covarience2, new Random(0));
        }


        /// <summary>
        /// Draws a vector based on variogram but with a different mean
        /// </summary>
        /// <param name="mean"></param>
        /// <returns></returns>
        public Vector DrawVectorDifferentMean(double mean)
        {
            var vector = DrawVector();
            var meanVector = ((Matrix)_mBuilder.Dense(Size, 1, mean-_meanValue)).Column(0);
            if (meanVector != null)
                return (Vector) (vector + meanVector);
            return null;
        }


        /// <summary>
        /// Draws a vector based on variogram
        /// </summary>
        /// <returns></returns>
        public Vector DrawVector()
        {
            var sample = _mNormal.Sample();
            return (Vector) sample.Column(0);
        }

    }


}
