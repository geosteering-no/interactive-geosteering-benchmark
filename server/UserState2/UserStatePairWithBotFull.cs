﻿using System;
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
            IList<TrueModelState> trueStates,
            ObjectiveEvaluatorDelegateTruth<RealizationData, WellPoint>.ObjectiveEvaluationFunction evaluatorTruth, 
            IList<RealizationData> trueRealizations,
            RegisterScorePairCallback pushToResultingTrajectories)
        {
            System.Console.WriteLine("Bot Started");
            for (var index = 0; index < trueRealizations.Count; index++)
            {
                System.Console.WriteLine("Bot started Level: " + index);
                while (true)
                {
                    var userData = UserDataLocked;
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
                    UpdateUserLocked(nextChoice, trueStates, evaluatorTruth, trueRealizations);
                }

                StopUserLocked(evaluatorTruth, trueRealizations);

                KeyValuePair<UserResultId, UserResultFinal<WellPoint>> pair;
                pair = GetUserResultScorePairLocked(trueRealizations.Count);
                pushToResultingTrajectories(pair);

                MoveUserToNewGameLocked(evaluatorTruth, trueRealizations);
            }
            System.Console.WriteLine("Bot finished");
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