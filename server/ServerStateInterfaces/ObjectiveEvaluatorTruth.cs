using System;
using System.Collections.Generic;
using System.Text;

namespace ServerStateInterfaces
{
    public static class ObjectiveEvaluatorDelegateTruth<TTruthRealization, TWellPoint>
    {
        public delegate double ObjectiveEvaluationFunction(TTruthRealization userData, IList<TWellPoint> trajectory);
    }

}
