using System.Collections.Generic;

namespace ServerDataStructures
{
    public class ManyWells<TWellPoint>
    {
        public IList<TrajectoryOutputSingle<TWellPoint>> Wells { get; set; }

    }
}