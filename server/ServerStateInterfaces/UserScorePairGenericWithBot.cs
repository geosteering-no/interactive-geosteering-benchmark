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

        protected IDssBotGeneric<TWellPoint, TRealizationData> Bot { get; set; }
        protected ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunctionSimple Objective { get; set; }


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



        public UserScorePairGenericWithBot(string userName, ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, TUserEvaluation>.ObjectiveEvaluationFunction EvaluatorUser, ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth, TRealizationData trueRealization) : base(userName, EvaluatorUser, evaluatorTruth, trueRealization) 
        {
        }
    }
}
