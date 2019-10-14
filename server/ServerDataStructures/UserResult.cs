using System;
using System.Collections.Generic;
using System.Text;

namespace ServerDataStructures
{
    public class UserResult
    {
        public string UserName { get; set; }
        public IList<WellPointWithScore> TrajectoryWithScore { get; set; }
        public bool Stopped { get; set; }
    }
}
