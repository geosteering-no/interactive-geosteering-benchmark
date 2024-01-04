using System.Collections.Generic;

namespace ServerDataStructures
{
    public class ManyWells<TWellPoint>
    {
        public IList<TrajectoryOutputSingle<TWellPoint>> UserResults { get; set; }

        public ManyWells()
        {
            UserResults = new List<TrajectoryOutputSingle<TWellPoint>>();
        }
    }
}