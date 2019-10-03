using System;
using System.Collections.Generic;
using System.Text;

namespace ServerObjectives
{
    public class DrillingCostObjective
    {
        /// <summary>
        /// Drilling cost used in compute drilling cost deligate
        /// </summary>
        public double DrillingCost = 1.0;


        public double ComputeDrillingCost(double x0, double y0, double x1, double y1)
        {
            //double length = Hypot(DX, DY * nextState.AngleJumpInY);
            double length = Utils.Hypot(x1 - x0, y1 - y0);
            return -length * DrillingCost;
        }
    }
}
