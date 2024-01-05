using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Matrix = MathNet.Numerics.LinearAlgebra.Double.Matrix;

namespace EnKFLib
{
    public class ReducedSvd
    {
        private static MatrixBuilder<double> _mBuild = Matrix.Build;

        public Matrix Up;
        public Matrix VT;
        public Vector Sp;

        /// <summary>
        /// The matrix of Sp
        /// </summary>
        public Matrix W
        {
            get
            {
                return (Matrix)_mBuild.SparseDiagonal(Sp.Count, VT.RowCount, i => Sp[i]);
            }
        }

        /// <summary>
        /// The matrix of Sp-inverces
        /// </summary>
        public Matrix InverseSigma
        {
            get
            {
                return (Matrix)_mBuild.SparseDiagonal(VT.RowCount, Sp.Count, i => 1.0/Sp[i]);
            }
        }


    }
}