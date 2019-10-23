using System;
using System.Collections.Generic;
using System.Linq;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public class UserStateMockBase : UserStateBase
    {
        private UserData _userData;
        private ObjectiveEvaluationDelegateUser<UserData, WellPoint, UserEvaluation>.ObjectiveEvaluationFunction _evaluator;
        const int DISCRETIZATION_POINTS = 10;
        private const int TOTAL_DECISION_POINTS = 9;
        const double X_TOP_LEFT = 10.0;
        const double Y_TOP_LEFT = 10.0;
        private const double X_WIDTH = 100;


        public UserStateMockBase()
        {
            _userData = CreateUserData();
        }

        public override void StopDrilling()
        {

            {
                _userData.stopped = true;
            }
        }


        public override bool UpdateUser(WellPoint updatePoint, RealizationData secret)
        {
            if (updatePoint.X - 1e-7 <= UserData.wellPoints[UserData.wellPoints.Count - 1].X)
            {
                return false;
            }

            {
                int prevIndex;
                for (prevIndex = 0; prevIndex < secret.XList.Count - 1; ++prevIndex)
                {
                    if (updatePoint.X < secret.XList[prevIndex])
                    {
                        break;
                    }
                }


                foreach (var userDataRealization in _userData.realizations)
                {
                    var minJ = Math.Min((int)userDataRealization.YLists.Count, secret.YLists.Count);
                    for (var j = 0; j < minJ; ++j)
                    {
                        userDataRealization.YLists[j][prevIndex] = secret.YLists[j][prevIndex];
                    }
                }

                UserData.wellPoints.Add(updatePoint);
                return true;

            }


        }

        public static UserData CreateUserData()
        {
            Random r = new Random();
            var userData = new UserData()
            {
                Width = UserStateMockBase.X_WIDTH,
                Xdist = UserStateMockBase.X_WIDTH / 10.0,
                Height = 10.0,
                wellPoints = new List<WellPoint>() { UserStateMockBase.GetNextStateDefault() },
                Xtopleft = UserStateMockBase.X_TOP_LEFT,
                Ytopleft = UserStateMockBase.Y_TOP_LEFT,
                stopped = false,
                TotalDecisionPoints = UserStateMockBase.TOTAL_DECISION_POINTS
            };
            var xs = new List<double>();

            for (double x = userData.Xtopleft; x <= userData.Xtopleft + userData.Width; x += userData.Width / UserStateMockBase.DISCRETIZATION_POINTS)
            {
                xs.Add(x);
            }

            userData.xList = xs;
            List<RealizationData> rs = Enumerable.Range(0, 100).Select(i =>
            {
                var realization = new RealizationData()
                {
                    XList = xs
                };

                var y1 = new List<double>();
                var y2 = new List<double>();

                for (double x = userData.Xtopleft; x <= userData.Xtopleft + userData.Width; x += userData.Width / UserStateMockBase.DISCRETIZATION_POINTS)
                {
                    y1.Add(userData.Ytopleft + userData.Height * 0.2 - (r.NextDouble() * userData.Height / 2.0) / 2);
                    y2.Add(userData.Ytopleft + userData.Height * 0.5 + (r.NextDouble() * userData.Height / 2.0) / 2);
                }

                realization.YLists.Add(y1);
                realization.YLists.Add(y2);

                //realization.polygons.Add(points);
                return realization;
            }).ToList();
            userData.realizations = rs;
            return userData;
        }


        public override UserData UserData
        {
            get
            {
                return _userData;
            }
        }

        public static WellPoint GetNextStateDefault()
        {
            //TODO consider a better implementation, but this is not needed functionality once client is good
            var point = new WellPoint()
            {
                X = X_TOP_LEFT,
                Y = Y_TOP_LEFT,
                Angle = 10.0 / 180 * 3.1415,
            };
            return point;
        }

        private WellPoint GetNextStateDefault2()
        {
            var point = new WellPoint()
            {
                X = X_TOP_LEFT + X_WIDTH / 10.0,
                Y = Y_TOP_LEFT + 1.0,
                Angle = 10.0 / 180 * 3.1415,
            };
            return point;
        }
    }
}