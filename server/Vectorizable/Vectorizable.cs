using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;


//TODO move to IRIS.Matrix   !!!
namespace IRIS.Matrix
{
    public interface Vectorizable
    {
        Vector Vector {get; set;}
        //bool FromVector(IRIS.Matrix.Vector vec);
        //IList<double> ToDoubleList();
        int VectorSize { get; }
    }
}
