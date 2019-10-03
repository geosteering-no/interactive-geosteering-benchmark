using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Variogram
{
    public static class Krigger
    {
        public static Vector Krig(Vector v1, Vector v2, double kriggingParameter)
        {
            var newVec = v1 * kriggingParameter + Math.Sqrt(1 - Math.Pow(kriggingParameter, 2)) * v2;
            return (Vector)newVec;
        }


        public static Vector KrigAndAssignMean(Vector v1, Vector v2, double kriggingParameter, double mean)
        {
            //reffereing to Rolf's code
            var newVec = v1 * kriggingParameter + Math.Sqrt(1 - Math.Pow(kriggingParameter, 2)) * v2 + mean;
            return (Vector)newVec;
        }
    }
}
