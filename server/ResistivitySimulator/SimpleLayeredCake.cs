using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ResistivitySimulator
{
    public class SimpleLayeredCake : IResistivityModel
    {
        private int _numLayers;
        private List<double> layerStart;
        private List<double> layerValue;
        /// <summary>
        /// Creates n random layers
        /// </summary>
        /// <param name="n"></param>
        public SimpleLayeredCake(int n, int seed = 0)
        {
            layerStart = new List<double>(n);
            layerValue = new List<double>(n);
            _numLayers = n;
            Random rnd = new Random(seed);
            layerStart.Add(0.0);
            layerValue.Add(rnd.Next(255));
            for (int i = 1; i < n; i++)
            {
                layerStart.Add(layerStart[layerStart.Count-1]+rnd.Next(5,25));
                layerValue.Add(rnd.Next(255));
            }
        }

        public double GetResistivity(Point2DDouble position)
        {
            return GetResistivity(position.X, position.Y);
        }

        public double GetResistivity(double x, double y)
        {
            int i = 0;
            for (; i < _numLayers-1 && layerStart[i] < y; ++i)
            {
                
            }
            return layerValue[i];
        }

        public int GetLayerNumber(double x, double y)
        {
            int i = 0;
            for (; i < _numLayers - 1 && layerStart[i] < y; ++i)
            {

            }
            return i;
        }

        public Vector Vector
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int VectorSize
        {
            get { throw new NotImplementedException(); }
        }
    }
}
