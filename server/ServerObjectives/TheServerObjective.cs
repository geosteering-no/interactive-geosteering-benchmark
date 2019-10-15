using System;
using System.Collections.Generic;
using System.Text;
using ServerDataStructures;

namespace ServerObjectives
{
    public class TheServerObjective
    {
        private DrillingCostObjective _drillingCostObjective;
        private SweetSpotObjective _sweetSpotObjective;
        public TheServerObjective()
        {
            _drillingCostObjective = new DrillingCostObjective();
            _sweetSpotObjective = new SweetSpotObjective();
        }
        public double TheObjective(
            RealizationData realization,
            double x0,
            double y0,
            double x1,
            double y1)
        {
            var xs = realization.XList;
            var sum = 0.0;
            sum += _drillingCostObjective.ComputeDrillingCost(x0, y0, x1, y1);
            sum += _sweetSpotObjective.ComputeReservoirValue(xs, realization, Utils.TnDRealizationData,
                x0, y0, x1, y1);
            return sum;
        }
    }
}
