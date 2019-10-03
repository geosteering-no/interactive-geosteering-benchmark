using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vectorizable;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ResistivitySimulator
{
 
    public class ResistivityData2DFull : IVectorizable
    //TODO consider having not all available
    {
        private static MathNet.Numerics.LinearAlgebra.VectorBuilder<double> _vBuild =
            MathNet.Numerics.LinearAlgebra.Vector<double>.Build;
        public ResistivityData2DFull()
        {
        }

        public ResistivityData2DFull(double up, double side, double down)
        {
            this[-1] = up;
            this[0] = side;
            this[1] = down;
        }

        public ResistivityData2DFull(IList<double> list)
        {
            if (list.Count != VectorSize)
            {
                throw new IndexOutOfRangeException("dimentions shoul agree");
            }
            int i = 0;
            foreach (double d in list)
            {
                data[i] = d;
                i++;
            }
        }


        public ResistivityData2DFull(Vector vec)
        {
            if (vec.Count != VectorSize)
            {
                throw new IndexOutOfRangeException("dimentions shoul agree");
            }
            for (int i = 0; i < VectorSize; ++i)
            {
                data[i] = vec[i];
            }
        }


        private double[] data = new double[3];
        //private bool[] defined = new bool[3];

        public double this[int i]
        {
            get
            {
                if (i < -1 || i > 1)
                {
                    throw new IndexOutOfRangeException("Can be up -1 side 0 or down 1");
                }
                int index = i + 1;
                return data[index];
            }
            set
            {
                if (i < -1 || i > 1)
                {
                    throw new IndexOutOfRangeException("Can be up -1 side 0 or down 1");
                }
                int index = i + 1;
                data[index] = value;
            }
        }



        public Vector Vector
        {
            get
            {
                return (Vector)_vBuild.DenseOfArray(data);
            }
            set
            {
                data = value.ToArray();
            }
        }

        public int VectorSize => 3;
    }
}
