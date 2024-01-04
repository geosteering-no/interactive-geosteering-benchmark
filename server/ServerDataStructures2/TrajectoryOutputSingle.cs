using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerDataStructures
{
    public class TrajectoryOutputSingle<TWellPoint>
    {
        public string UserName { get; set; }
        public long TimeTicks { get; set; }
        public IList<TWellPoint> TrajectoryWithScore { get; set; }
        public IList<TWellPoint> PlannedTrajectory { get; set; }

        public static TrajectoryOutputSingle<TWellPoint> FromUserResult(UserResultFinal<TWellPoint> userResult)
        {
            return new TrajectoryOutputSingle<TWellPoint>(userResult);
        }

        public TrajectoryOutputSingle(UserResultFinal<TWellPoint> userResult)
        {
            UserName = userResult.UserName;
            TimeTicks = userResult.TimeTicks;
            TrajectoryWithScore = userResult.TrajectoryWithScore.Select(pWithScore => pWithScore.wellPoint).ToList();
            PlannedTrajectory = userResult.PlannedTrajectory;
        }
    }
}
