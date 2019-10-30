﻿using ServerDataStructures;
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
                if (totalLeft <= 0)
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
                UpdateUser(nextChoice, trueState, evaluatorTruth, trueRealization);
            }

            StopUser(evaluatorTruth, 
                trueRealization);
        }

        public UserStatePairWithBotFull(string userName, ObjectiveEvaluationDelegateUser<UserData, WellPoint, UserEvaluation>.ObjectiveEvaluationFunction EvaluatorUser, ObjectiveEvaluatorDelegateTruth<RealizationData, WellPoint>.ObjectiveEvaluationFunction evaluatorTruth, ObjectiveEvaluatorDelegateTruth<RealizationData, WellPoint>.ObjectiveEvaluationFunctionSimple evaluatorSimple, RealizationData trueRealization) 
            : base(userName, EvaluatorUser, evaluatorTruth, evaluatorSimple, trueRealization)
        {
        }
    }
}