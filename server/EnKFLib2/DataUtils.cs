using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;
using Vectorizable;

namespace EnKFLib
{
    public class DataUtils
    {
        public static Vector CollectIntoVector<T>(IList<T> dataList) where T: IVectorizable
        {
            List<double> list = new List<double>();
            foreach (var vectorizableEntry in dataList)
            {
                Vector vi = vectorizableEntry.Vector;
                list.AddRange(vi.ToArray());
            }
            var vBuild = Vector.Build;
            var res = vBuild.DenseOfEnumerable(list);
            return (Vector)res;
        }

        public static Vector CollectIntoVector(IList<Vector> dataList) 
        {
            List<double> list = new List<double>();
            foreach (var vectorizableEntry in dataList)
            {
                Vector vi = vectorizableEntry;
                list.AddRange(vi.ToArray());
            }
            var vBuild = Vector.Build;
            var res = vBuild.DenseOfEnumerable(list);
            return (Vector)res;
        }

    }
}
