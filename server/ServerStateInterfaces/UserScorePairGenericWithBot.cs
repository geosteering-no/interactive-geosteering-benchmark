using System;
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

        private readonly int TotalLevelsOnServer;

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
            TSecretState secretState,
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth,
            TRealizationData trueRealization)
        {
            if (Bot != null)
            {
                var thread = new Thread(() =>
                {
                    RunBot(secretState, evaluatorTruth, trueRealization);
                });
                thread.Priority = ThreadPriority.BelowNormal;
                thread.Start();
            }
        }

        protected abstract void RunBot
            (TSecretState trueState,
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth, 
            TRealizationData trueRealization);


        public UserScorePairGenericWithBot(string userName, 
            ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, TUserEvaluation>.ObjectiveEvaluationFunction EvaluatorUser, 
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth,
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunctionSimple evaluatorSimple,
            IList<TRealizationData> trueRealizations) : base(userName, EvaluatorUser, evaluatorTruth, trueRealizations)
        {
            Objective = evaluatorSimple;
        }

        private UserScorePairGenericWithBot(string userName, ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, TUserEvaluation>.ObjectiveEvaluationFunction EvaluatorUser, ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth, 
            IList<TRealizationData> trueRealizations) :
            base(userName, EvaluatorUser, evaluatorTruth, trueRealizations)
        {
            TotalLevelsOnServer = trueRealizations.Count;
        }
    }
}
