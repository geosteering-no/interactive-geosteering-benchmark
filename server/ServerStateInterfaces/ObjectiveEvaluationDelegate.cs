using System.Collections.Generic;

namespace ServerStateInterfaces
{
    public static class ObjectiveEvaluationDelegate<TUserData, TWellPoint, TUserEvaluation>
    {
        public delegate TUserEvaluation ObjectiveEvaluationFunction(TUserData userData, IList<TWellPoint> trajectory);
    }
}