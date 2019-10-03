using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Variogram
{
    public class ExpVariogram : IVariogram
    {
        public double Nugget { get; }
        public double Sill { get; }
        public double Range { get; }
        private readonly double _a;

        /// <summary>
        /// Creates an exponential variogram
        /// </summary>
        /// <param name="nugget">Minimum variogram value (normally at distance 0)</param>
        /// <param name="sill">Upper bound of variogram</param>
        /// <param name="range">Distance between points which are 'almost' not realted</param>
        /// <param name="a">Variogram parameter</param>
        public ExpVariogram(double nugget, double sill, double range, double a = 1.0/3.0)
        {
            Nugget = nugget;
            Sill = sill;
            Range = range;
            _a = a;
        }

        public double GetValue(double distace)
        {
            if (distace <= 0)
            {
                return 0;
            }
            return (Sill - Nugget)*(1 - Math.Exp(-distace/(Range*_a)));
        }
    }
}
