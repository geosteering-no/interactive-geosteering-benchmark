using System;
using System.Collections.Generic;
using ServerDataStructures;
using ServerStateInterfaces;

namespace UserState
{
    public class UserStatePairWithBotFull : UserScorePairGenericWithBot<UserState, UserData,
        TrueModelState, WellPoint,
        UserEvaluation, RealizationData>
    {
        protected override void RunBot(
            TrueModelState trueState,
            ObjectiveEvaluatorDelegateTruth<RealizationData, WellPoint>.ObjectiveEvaluationFunction evaluatorTruth, 
            RealizationData trueRealization)
        {
            while (true)
            {
                var userData = UserData;
                var userPointsCount = userData.wellPoints.Count;
                var totalLeft = userData.TotalDecisionPoints - userPointsCount;
                if (totalLeft <= 0 || userData.stopped)
                {
                    break;
                }
                Bot.Init(0.9,
                    userData.Xdist,
                    totalLeft,
                    userData.MaxAngleChange,
                    userData.MinInclination,
                    Objective);
                var lastPoint = userData.wellPoints[userPointsCount - 1];
                var nextChoice = Bot.ComputeBestChoice(userData.realizations, lastPoint, totalLeft);
                throw new NotImplementedException();
                //UpdateUserLocked(nextChoice, trueState, evaluatorTruth, trueRealization);
            }

            throw new NotImplementedException();
            //StopUserLocked(evaluatorTruth, 
                //trueRealization);
        }

        public UserStatePairWithBotFull(string userName, 
            ObjectiveEvaluationDelegateUser<UserData, WellPoint, UserEvaluation>.ObjectiveEvaluationFunction EvaluatorUser, 
            ObjectiveEvaluatorDelegateTruth<RealizationData, WellPoint>.ObjectiveEvaluationFunction evaluatorTruth, 
            ObjectiveEvaluatorDelegateTruth<RealizationData, WellPoint>.ObjectiveEvaluationFunctionSimple evaluatorSimple, 
            IList<RealizationData> trueRealizations) 
            : base(userName, EvaluatorUser, evaluatorTruth, evaluatorSimple, trueRealizations)
        {
        }
    }
}