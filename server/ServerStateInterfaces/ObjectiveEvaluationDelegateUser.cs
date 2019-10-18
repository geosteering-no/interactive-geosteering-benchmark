using System.Collections.Generic;

namespace ServerStateInterfaces
{
    public static class ObjectiveEvaluationDelegateUser<TUserData, TWellPoint, TUserEvaluation>
    {
        public delegate TUserEvaluation ObjectiveEvaluationFunction(TUserData userData, IList<TWellPoint> trajectory);
    }
}