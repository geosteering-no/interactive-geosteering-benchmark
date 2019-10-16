using System;
using System.Collections.Generic;
using System.Text;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public class ServerStateMock : ServerStateBase<WellPoint, UserData, UserStateMockBase, RealizationData, UserEvaluation>
    {
        private readonly ObjectiveEvaluator _evaluatorClass = new ObjectiveEvaluator();

        protected override ObjectiveEvaluationDelegate<UserData, WellPoint, UserEvaluation>.ObjectiveEvaluationFunction 
            Evaluator => _evaluatorClass.EvaluateDefault;

        protected override void InitializeNewSyntheticTruth(int seed = 0)
        {
            var defaultUser = GetDefaultNewUser();
            _secret = defaultUser.UserData.realizations[seed % defaultUser.UserData.realizations.Count];
            DumpSectetStateToFile(seed);
        }
    }
}
