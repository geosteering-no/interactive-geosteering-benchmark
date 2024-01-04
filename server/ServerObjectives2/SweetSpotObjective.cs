using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ServerObjectives
{
    public class SweetSpotObjective
    {
        public SweetSpotObjective()
        {
            NumPoint = 100;
        }
        public SweetSpotObjective(int numPoint)
        {
            NumPoint = numPoint;
        }

        public int NumPoint { get; }
        public double SweetSpotMult = 2.0;
        public double OtherReservoirMult = 1.0;
        public bool FollowBottom = true;
        public double SweetSpotOffset = 0.5;
        //NOTE was 1.5
        public double SweetSpotEnd = 1.5;

        public delegate Tuple<double, double> ThicknessAndDistenceAboveDelegate<T>(IList<double> xs, T model, double x, double z);

        /// <summary>
        /// Function should be
        /// 0 outside
        /// dReservoir value at Above value
        /// decaying fast below Above value
        /// decaying slowly above Above value
        /// </summary>
        /// <param name="thicknessAndDistanceAbove"></param>
        /// <returns></returns>
        private double _reservoirValueFunction(Tuple<double, double> thicknessAndDistanceAbove, double dReservoirValue)
        {
            double currThickness = thicknessAndDistanceAbove.Item1;
            double currAbove = thicknessAndDistanceAbove.Item2;
            if (FollowBottom)
            {
                currAbove = currThickness - currAbove;
            }
            //within hot spot
            var positionValue = 0.0;
            if (currAbove < 0 || currAbove > currThickness)
            {
                positionValue = 0.0;
            }
            else if (currAbove >= SweetSpotOffset 
                     && currThickness - currAbove >= SweetSpotOffset
                     && currAbove <= SweetSpotEnd)
            {
                positionValue = SweetSpotMult;
            }
            else
            {
                positionValue = OtherReservoirMult;
            }
            return positionValue * currThickness * dReservoirValue;

        }


        public double ComputeReservoirValue<T>(IList<double> xs, T model, ThicknessAndDistenceAboveDelegate<T> thicknessAndDistenceAbove, double x0, double y0, double x1, double y1)
        {
            //double length = Hypot(DX, DY * nextState.AngleJumpInY);
            //double length = Utils.Hypot(x1 - x0, y1 - x0);
            //this should be a horizontal length intentionally
            double dLength = (x1 - x0) / NumPoint;
            var sum = 0.0;
            for (var lambda = 1.0 / NumPoint / 2; lambda < 1.0; lambda += 1.0 / NumPoint)
            {
                var x = x0 * lambda + x1 * (1.0 - lambda);
                var y = y0 * lambda + y1 * (1.0 - lambda);
                var thicknessAndDistance = thicknessAndDistenceAbove(xs, model, x, y);
                sum += _reservoirValueFunction(thicknessAndDistance, dLength);
            }
            return sum;
        }
    }
}