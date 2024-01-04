using System;
using System.Collections.Generic;
using ServerDataStructures;

namespace ServerObjectives
{
    public class Utils
    {
        public static double Hypot(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        private static int IndexOfX(IList<double> xPositions, double x)
        {
            int left = 0;
            int right = xPositions.Count - 1;
            while (right - left > 1)
            {
                int mid = (right + left) / 2;
                if (xPositions[mid] > x)
                {
                    right = mid;
                }
                else
                {
                    left = mid;
                }
            }
            //TODO check
            return left;
        }

        public static Tuple<double, double> TnDRealizationData(
            IList<double> xs, 
            RealizationData realization, 
            double x,
            double z)
        {
            int indexPrev = IndexOfX(xs, x);
            double len = xs[indexPrev + 1] - xs[indexPrev];
            x -= xs[indexPrev];
            double lambda = x / len;

            double zTop = 0;
            double zBot = 0;
            double zTop2 = 0;
            double zBot2 = 0;
            if (realization.YLists.Count >= 2)
            { 
                var upperBoundary = realization.YLists[0];
                var lowerBoundary = realization.YLists[1];

                //get value for mid point
                zTop = upperBoundary[indexPrev] * (1 - lambda) + upperBoundary[indexPrev + 1] * lambda;
                zBot = lowerBoundary[indexPrev] * (1 - lambda) + lowerBoundary[indexPrev + 1] * lambda;
            }

            if (realization.YLists.Count >= 4)
            {
                //get second layer
                var upperBoundary2 = realization.YLists[2];
                var lowerBoundary2 = realization.YLists[3];
                zTop2 = upperBoundary2[indexPrev] * (1 - lambda) + upperBoundary2[indexPrev + 1] * lambda;
                zBot2 = lowerBoundary2[indexPrev] * (1 - lambda) + lowerBoundary2[indexPrev + 1] * lambda;
            }

            //check if one layer only (degenerate model)
            if (realization.YLists.Count >= 4 && zBot >= zTop2)
            {
                if (zTop <= z && z <= zBot2)
                {
                    return new Tuple<double, double>(zBot2 - zTop, zBot2 - z);
                }
                else
                {
                    return new Tuple<double, double>(0.0, 0.0);
                }
            }
            if (zTop <= z && z <= zBot)
            {
                return new Tuple<double, double>(zBot - zTop, zBot - z);
            }
            if (zTop2 <= z && z <= zBot2)
            {
                return new Tuple<double, double>(zBot2 - zTop2, zBot2 - z);
            }
            return new Tuple<double, double>(0.0, 0.0);
        }
    }
    
}