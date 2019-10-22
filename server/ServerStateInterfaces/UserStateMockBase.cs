﻿using System;
using System.Collections.Generic;
using System.Linq;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public class UserStateMockBase : IUserImplementaion<UserData, WellPoint, RealizationData, UserEvaluation, RealizationData>
    {
        private UserData _userData;
        private ObjectiveEvaluationDelegateUser<UserData, WellPoint, UserEvaluation>.ObjectiveEvaluationFunction _evaluator;
        const int DISCRETIZATION_POINTS = 10;
        private const int TOTAL_DECISION_POINTS = 9;
        const double X_TOP_LEFT = 10.0;
        const double Y_TOP_LEFT = 10.0;
        private const double X_WIDTH = 100;
        



        public ObjectiveEvaluationDelegateUser<UserData, WellPoint, UserEvaluation>.ObjectiveEvaluationFunction
            Evaluator
        {
            get
            {
                return _evaluator;
            }
            set { _evaluator = value; }
        }

        public static UserData CreateUserData()
        {
            Random r = new Random();
            var userData = new UserData()
            {
                Width = X_WIDTH,
                Xdist = X_WIDTH / 10.0,
                Height = 10.0,
                wellPoints = new List<WellPoint>() { GetNextStateDefault() },
                Xtopleft = X_TOP_LEFT,
                Ytopleft = Y_TOP_LEFT,
                stopped = false,
                TotalDecisionPoints = TOTAL_DECISION_POINTS
            };
            var xs = new List<double>();

            for (double x = userData.Xtopleft; x <= userData.Xtopleft + userData.Width; x += userData.Width / DISCRETIZATION_POINTS)
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

                for (double x = userData.Xtopleft; x <= userData.Xtopleft + userData.Width; x += userData.Width / DISCRETIZATION_POINTS)
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

        public UserStateMockBase()
        {
            _userData = CreateUserData();
        }

        public UserData UserData
        {
            get
            {
                return _userData;
            }
        }

        public bool Stopped
        {
            get { return UserData.stopped; }

        }

        public bool UpdateUser(WellPoint updatePoint, RealizationData secret)
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
                    var minJ = Math.Min(userDataRealization.YLists.Count, secret.YLists.Count);
                    for (var j = 0; j < minJ; ++j)
                    {
                        userDataRealization.YLists[j][prevIndex] = secret.YLists[j][prevIndex];
                    }
                }

                UserData.wellPoints.Add(updatePoint);
                return true;

            }


        }



        public void StopDrilling()
        {

            {
                _userData.stopped = true;
            }
        }

        WellPoint IUserImplementaion<UserData, WellPoint, RealizationData, UserEvaluation, RealizationData>.GetNextStateDefault()
        {
            return GetNextStateDefault();
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

        public UserEvaluation GetEvaluation(IList<WellPoint> trajectory)
        {

            {
                //convert to avoid erroers
                var userData = UserData;

                var result = Evaluator(userData, trajectory);

                return result;
            }
        }

        public IList<WellPointWithScore<WellPoint>> GetEvaluationForTruth(
            ObjectiveEvaluatorDelegateTruth<RealizationData, WellPoint>.ObjectiveEvaluationFunction
            evaluator,
            RealizationData secretData)
        {

            {
                var trajectory = UserData.wellPoints;
                var resultList = new List<WellPointWithScore<WellPoint>>(trajectory.Count);
                var sum = 0.0;

                for (var i = 0; i < trajectory.Count; i++)
                {
                    var pt = trajectory[i];
                    if (i == 0)
                    {
                        resultList.Add(new WellPointWithScore<WellPoint>()
                        {
                            wellPoint = pt,
                            Score = sum
                        });
                    }
                    else
                    {
                        var twoPoints = new List<WellPoint>() {trajectory[i - 1], pt};
                        sum += evaluator(secretData, twoPoints);
                        resultList.Add(new WellPointWithScore<WellPoint>()
                        {
                            wellPoint = pt,
                            Score = sum
                        });

                    }
                }

                return resultList;
            }
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