using System;
using System.Collections.Generic;
using System.Text;

namespace ServerDataStructures
{
    public class UserResultFinal<TWellPoint> 
    {
        public string UserName { get; set; }
        public IList<WellPointWithScore<TWellPoint>> TrajectoryWithScore { get; set; }
        public IList<TWellPoint> PlannedTrajectory { get; set; }
        public bool Stopped { get; set; }
        public long TimeTicks { get; set; }
        ///// <summary>
        ///// Marked for removal
        ///// </summary>
        //public double AccumulatedScoreFromPreviousGames { get; set; }
        ///// <summary>
        ///// Marked for removal
        ///// </summary>
        //public double AccumulatedScorePercentFromPreviousGames { get; set; }

        public UserResultFinal()
        {
            TimeTicks = DateTime.Now.Ticks;
        }

    }
}
