using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TrajectoryOptimization;

namespace ResistivitySimulator
{
    public class MetersAboveLowerBoundaryValueModel : IValueModel<EarthModelRealizationBaseClass>, IEquatable<MetersAboveLowerBoundaryValueModel>
    {
        //private readonly EarthModelRealizationBaseClass _parent;
        //private int _reservoirSamplingResolution = 10;

        public double MetersAbove { get; set; } = 1.0;
        public bool FollowBottom = true;


        public MetersAboveLowerBoundaryValueModel(double metersAbove)
        {
            //_parent = earthModelRelizationMultiLayer;
            MetersAbove = metersAbove;
        }

        //public double DX => TrajectoryOptimizerDP.DX;

        /// <summary>
        /// A multiplyer for reservoir value
        /// </summary>
        public double dReservoirValue = 1.0;


        /// <summary>
        /// Function should be
        /// 0 outside
        /// dReservoir value at Above value
        /// decaying fast below Above value
        /// decaying slowly above Above value
        /// </summary>
        /// <param name="thicknessAndDistanceAbove"></param>
        /// <returns></returns>
        private double _reservoirValueFunction(Tuple<double, double> thicknessAndDistanceAbove)
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
            else if (currAbove <= MetersAbove * 1.5 && currAbove >= MetersAbove * 0.5)
            {
                positionValue = 2.0;
            }
            else
            {
                positionValue = 1.0;
            }
            return positionValue * currThickness * dReservoirValue;

            //            if (currAbove <= MetersAbove)
            //            {
            //                // High thickness is favoured (thicker layer contains more oil)
            //                return currAbove / MetersAbove * currThickness * dReservoirValue;
            //            }
            //            else
            //            {
            //                // High thickness is favoured
            //                return Math.Exp(-currAbove + MetersAbove) * currThickness * dReservoirValue;
            //            }
        }

        public int PointsPerSegment = 10;

        /// <summary>
        /// A multiplyer applied to reservoir exit
        /// </summary>
        public double ExitMultiplyer = 2.0;


        private int getZone(double dist)
        {
            if (dist <= 0)
            {
                return -5;
            }
            if (dist <= 0.5 * MetersAbove)
            {
                return -1;
            }
            return 0;
        }


        public double ComputeExitValue(EarthModelRealizationBaseClass model, Tuple<ContinousState, ContinousState> segemntFromTo)
        {
            // This is where more objectives will be included ("sum +=" below).
            // Could e.g. use resistivity, porosity, facies type, etc

            int mult = 0;
            int zone = -5;
            // 0 for normal zone
            // -1 danger zone
            // -5 exit

            for (int i = 0; i <= PointsPerSegment; ++i)
            {
                var x = (segemntFromTo.Item2.X * i + segemntFromTo.Item1.X * (PointsPerSegment - i)) / PointsPerSegment;
                var z = (segemntFromTo.Item2.Y * i + segemntFromTo.Item1.Y * (PointsPerSegment - i)) / PointsPerSegment;

                if (model.ConsiderSpecificTargetGeobodies())
                {
                    if (model.IsTargetGeobody(model.GetGeobodyIndex(x, z)))
                    {
                        // Only contributes if this is a target layer
                        Tuple<double, double> thicknessAndDistanceAbove = model.ThicknessAndDistanceAbove(x, z);
                        var distanceAbove = thicknessAndDistanceAbove.Item2;
                        var distanceBelow = thicknessAndDistanceAbove.Item1 - distanceAbove;
                        var dist = Math.Min(distanceAbove, distanceBelow);
                        var newZone = getZone(dist);
                        if (newZone < zone)
                        {
                            mult = Math.Min(mult, newZone);
                        }
                        zone = newZone;
                    }
                    else
                    {
                        var newZone = -5;
                        if (newZone < zone)
                        {
                            mult = Math.Min(mult, newZone);
                        }
                        zone = newZone;

                    }
                }
                else
                {
                    Tuple<double, double> thicknessAndDistanceAbove = model.ThicknessAndDistanceAbove(x, z);
                    var distanceAbove = thicknessAndDistanceAbove.Item2;
                    var distanceBelow = thicknessAndDistanceAbove.Item1 - distanceAbove;
                    var dist = Math.Min(distanceAbove, distanceBelow);
                    var newZone = getZone(dist);
                    if (newZone < zone)
                    {
                        mult = Math.Min(mult, newZone);
                    }
                    zone = newZone;
                }
            }

            var result = mult * ExitMultiplyer * TrajectoryOptimizerDP<double>.Hypot(segemntFromTo.Item2.X - segemntFromTo.Item1.X, segemntFromTo.Item2.Y - segemntFromTo.Item1.Y);
            return result;
        }

        //T model, IncompleteTrajectoryState state
        public double ComputeReservoirValue(EarthModelRealizationBaseClass model, double x, double z)
        {
            // This is where more objectives will be included ("sum +=" below).
            // Could e.g. use resistivity, porosity, facies type, etc


            var sum = 0.0;

            if (model.ConsiderSpecificTargetGeobodies())
            {
                if (model.IsTargetGeobody(model.GetGeobodyIndex(x, z)))
                {
                    // Only contributes if this is a target layer
                    sum += _reservoirValueFunction(model.ThicknessAndDistanceAbove(x, z));
                }
            }
            else
            {
                sum += _reservoirValueFunction(model.ThicknessAndDistanceAbove(x, z));
            }


            //length adjustment
            //sum *= Hypot(DX, DY * state.angleJumpInY);
            //taking only horizontal component
            //sum *= DX;
            return sum;
        }

        public bool Equals(MetersAboveLowerBoundaryValueModel other)
        {
            return other != null && Equals(MetersAbove, other.MetersAbove);
        }
    }
}
