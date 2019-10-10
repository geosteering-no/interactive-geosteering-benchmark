using System;
using System.Collections.Generic;

namespace GameServer
{
    public class Realization
    {
        public Realization()
        {
            polygons = new List<List<Tuple<double, double>>>();
        }
        public List<List<Tuple<Double, Double>>> polygons { get; }

    }
}
