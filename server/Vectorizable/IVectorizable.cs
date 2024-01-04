using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//TODO move to .NetNumeric...
namespace Vectorizable
{
    /// <summary>
    /// This gives a double vector representation for a model and accepts update as the same-size vector
    /// </summary>
    public interface IVectorizable
    {
        /// <summary>
        /// Gets the double vector representing the matrxi
        /// Updates the model given new vector of the same size
        /// </summary>
        MathNet.Numerics.LinearAlgebra.Double.Vector Vector {get; set;}
        //bool FromVector(IRIS.Matrix.Vector vec);
        //IList<double> ToDoubleList();

        /// <summary>
        /// The vector size that remains the same before/after updates
        /// </summary>
        int VectorSize { get; }
    }
}
