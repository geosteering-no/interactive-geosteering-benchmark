using System;
using System.Collections.Generic;
using System.Text;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public interface IUserImplementaion<TUserData, TWellPoint, TSecretState, TEvaluationResult>
    {
        
        ObjectiveEvaluationDelegate<TUserData, TWellPoint, TEvaluationResult>.ObjectiveEvaluationFunction 
            Evaluator { get; set; }
        TUserData UserData { get; }
        bool UpdateUser(TWellPoint updatePoint, TSecretState secret);
        TWellPoint GetNextStateDefault();
        TEvaluationResult GetEvaluation(IList<TWellPoint> trajectory);
    }
}
