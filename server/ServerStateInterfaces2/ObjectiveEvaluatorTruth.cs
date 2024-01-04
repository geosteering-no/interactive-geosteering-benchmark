using System;
using System.Collections.Generic;
using System.Text;

namespace ServerStateInterfaces
{
    public static class ObjectiveEvaluatorDelegateTruth<TTruthRealization, TWellPoint>
    {
        public delegate double ObjectiveEvaluationFunction(TTruthRealization userData, IList<TWellPoint> trajectory);
        public delegate double ObjectiveEvaluationFunctionSimple(TTruthRealization realization,
            double x0,
            double y0,
            double x1,
            double y1);
    }

}
