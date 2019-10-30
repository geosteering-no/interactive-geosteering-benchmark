using System.Collections.Generic;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public interface IDssBotGeneric<TWellPoint, TRealizationData>
    {
        void Init(double discountFactorOptimization, double decisionLenX, int totalOptSteps,
            double maxTurnAngle, double minInclination, 
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunctionSimple 
                simpleObjectiveDelegate);

        TWellPoint ComputeBestChoice(IList<TRealizationData> realizations, TWellPoint wp, int totalRemaining);
    }
}