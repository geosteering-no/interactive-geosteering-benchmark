using System;
using System.Collections.Generic;
using System.Linq;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public abstract class UserStateBase : IUserImplementaion<UserData, WellPoint, RealizationData, UserEvaluation, RealizationData>
    {
        protected ObjectiveEvaluationDelegateUser<UserData, WellPoint, UserEvaluation>.ObjectiveEvaluationFunction _evaluator;
        public ObjectiveEvaluationDelegateUser<UserData, WellPoint, UserEvaluation>.ObjectiveEvaluationFunction
            Evaluator
        {
            get
            {
                return _evaluator;
            }
            set { _evaluator = value; }
        }

        public abstract UserData UserData { get; }

        public bool Stopped
        {
            get { return UserData.stopped; }

        }

        public abstract bool UpdateUser(WellPoint updatePoint, RealizationData secret);
        public abstract void StopDrilling();


        WellPoint IUserImplementaion<UserData, WellPoint, RealizationData, UserEvaluation, RealizationData>.GetNextStateDefault()
        {
            return UserStateMockBase.GetNextStateDefault();
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
    }
}