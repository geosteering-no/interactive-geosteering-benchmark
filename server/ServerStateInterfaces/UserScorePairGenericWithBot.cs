using System;
using System.Collections.Generic;
using System.Text;

namespace ServerStateInterfaces
{
    public class UserScorePairGenericWithBot<TUserModel, TUserDataModel,
        TSecretState, TWellPoint,
        TUserEvaluation, TRealizationData> : UserScorePairLockedGeneric<TUserModel, TUserDataModel,
        TSecretState, TWellPoint,
        TUserEvaluation, TRealizationData> where TUserModel : IUserImplementaion<TUserDataModel, TWellPoint, TSecretState, TUserEvaluation, TRealizationData>, new()
    {

        public IDssBotGeneric<TWellPoint, TRealizationData> Bot { get; set; }

        public void RunBot()
        {
            if (Bot != null)
            {
                throw new NotImplementedException();

            }
        }

        public UserScorePairGenericWithBot(string userName, ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, TUserEvaluation>.ObjectiveEvaluationFunction EvaluatorUser, ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth, TRealizationData trueRealization) : base(userName, EvaluatorUser, evaluatorTruth, trueRealization) 
        {
        }
    }
}
