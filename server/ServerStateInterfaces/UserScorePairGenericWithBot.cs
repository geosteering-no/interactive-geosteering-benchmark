using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public abstract class UserScorePairGenericWithBot<TUserModel, TUserDataModel,
        TSecretState, TWellPoint,
        TUserEvaluation, TRealizationData> : UserScorePairLockedGeneric<TUserModel, TUserDataModel,
        TSecretState, TWellPoint,
        TUserEvaluation, TRealizationData> where TUserModel : IUserImplementaion<TUserDataModel, TWellPoint, TSecretState, TUserEvaluation, TRealizationData>, new()
    {

        protected override void GenerateLevelIdsForUser()
        {
            GameIds = new List<int>(TotalLevelsOnServer);
            for (int i = 0; i < TotalLevelsOnServer; ++i)
            {
                GameIds.Add(i);
            }
        }

        public IDssBotGeneric<TWellPoint, TRealizationData> Bot { get; set; }
        protected ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunctionSimple Objective { get; }


        public void StartBot(
            IList<TSecretState> secretStates,
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth,
            IList<TRealizationData> trueRealizations,
            RegisterScorePairCallback registerScoreForAGameCallback)
        {
            if (Bot != null)
            {
                var thread = new Thread(() =>
                {
                    RunBot(secretStates, evaluatorTruth, trueRealizations, registerScoreForAGameCallback);
                });
                thread.Priority = ThreadPriority.BelowNormal;
                thread.Start();
            }
        }

        protected abstract void RunBot
            (IList<TSecretState> trueStates,
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction objectiveEvaluationFunction, 
            IList<TRealizationData> trueRealizations,
            RegisterScorePairCallback registerScoreForAGameCallback);


        public UserScorePairGenericWithBot(string userName, 
            ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, TUserEvaluation>.ObjectiveEvaluationFunction EvaluatorUser, 
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth,
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunctionSimple evaluatorSimple,
            IList<TRealizationData> trueRealizations) : base(userName, EvaluatorUser, evaluatorTruth, trueRealizations)
        {
            //GenerateLevelIdsForUser();
            Objective = evaluatorSimple;
            
        }

        private UserScorePairGenericWithBot(string userName, ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, TUserEvaluation>.ObjectiveEvaluationFunction EvaluatorUser, ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth, 
            IList<TRealizationData> trueRealizations) :
            base(userName, EvaluatorUser, evaluatorTruth, trueRealizations)
        {
            //GenerateLevelIdsForUser();
        }
    }
}
