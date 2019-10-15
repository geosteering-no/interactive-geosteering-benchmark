using System;
using System.Collections.Generic;
using System.Text;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public class ServerStateMock : ServerStateBase<WellPoint, UserData, UserStateMockBase<int>, int, UserEvaluation>
    {
        private readonly ObjectiveEvaluator _evaluatorClass = new ObjectiveEvaluator();

        protected override ObjectiveEvaluationDelegate<UserData, WellPoint, UserEvaluation>.ObjectiveEvaluationFunction 
            Evaluator => _evaluatorClass.EvaluateDefault;
    }
}
