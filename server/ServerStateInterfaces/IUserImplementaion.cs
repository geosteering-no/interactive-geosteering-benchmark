using System;
using System.Collections.Generic;
using System.Text;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public interface IUserImplementaion<TUserData, TWellPoint, TSecretState, TEvaluationResult, TRealizationData>
    {
        
        ObjectiveEvaluationDelegateUser<TUserData, TWellPoint, TEvaluationResult>.ObjectiveEvaluationFunction 
            Evaluator { get; set; }
        //double DoiX { get; set; }
        //double DoiY { get; set; }
        TUserData UserData { get; }
        bool UpdateUser(TWellPoint updatePoint, TSecretState secret);
        void StopDrilling();
        TWellPoint GetNextStateDefault();
        TEvaluationResult GetEvaluation(IList<TWellPoint> trajectory);
        IList<WellPointWithScore<TWellPoint>> GetEvaluationForTruth(
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction
                evaluator,
            TRealizationData secretData);

    }
}
