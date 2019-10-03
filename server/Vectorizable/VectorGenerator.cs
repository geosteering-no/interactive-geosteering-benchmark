using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Vectorizable
{
    public class VectorGenerator
    {
        private VectorGenerator()
        {
        }

        public static Vector One(int size)
        {
            DenseVector one = new DenseVector(size);
            for (int i = 0; i < size; ++i)
            {
                one[i] = 1;
            }
            return one;
        }
    }
}
