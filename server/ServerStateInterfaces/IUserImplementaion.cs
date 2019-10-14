using System;
using System.Collections.Generic;
using System.Text;

namespace ServerStateInterfaces
{
    public interface IUserImplementaion<TUserData, TWellPoint, TSecretState, TEvaluationResult>
    {
        TUserData UserData { get; }
        bool UpdateUser(TWellPoint updatePoint, TSecretState secret);
        TWellPoint GetNextStateDefault();
        TEvaluationResult GetEvaluation(IList<TWellPoint> trajectory);
    }
}
