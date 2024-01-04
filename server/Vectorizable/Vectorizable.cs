using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;


namespace MyFunctions.Matrix
{
    public interface Vectorizable
    {
        Vector Vector {get; set;}
        //IList<double> ToDoubleList();
        int VectorSize { get; }
    }
}
