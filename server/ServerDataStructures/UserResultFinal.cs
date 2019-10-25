using System;
using System.Collections.Generic;
using System.Text;

namespace ServerDataStructures
{
    public class UserResultFinal<TWellPoint> : ICloneable
    {
        public string UserName { get; set; }
        public IList<WellPointWithScore<TWellPoint>> TrajectoryWithScore { get; set; }
        public bool Stopped { get; set; }
        public double AccumulatedScoreFromPreviousGames { get; set; }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
