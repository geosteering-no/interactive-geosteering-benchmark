using System;
using System.Collections.Generic;
using System.Text;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public class ServerStateMock : ServerStateBase<WellPoint, UserData, UserStateMockBase, RealizationData, UserEvaluation, RealizationData>
    {
        private readonly ObjectiveEvaluator _evaluatorClass = new ObjectiveEvaluator();

        protected override ObjectiveEvaluationDelegate<UserData, WellPoint, UserEvaluation>.ObjectiveEvaluationFunction 
            Evaluator => _evaluatorClass.EvaluateDefault;

        private UserData _dummyUserData;

        public ServerStateMock() : base()
        {
            _dummyUserData = UserStateMockBase.CreateUserData();
        }

        protected override void InitializeNewSyntheticTruth(int seed = 0)
        {
            var defaultUser = GetDefaultNewUser();
            _secret = defaultUser.UserData.realizations[seed % defaultUser.UserData.realizations.Count];
            DumpSectetStateToFile(seed);
        }

        public override PopulationScoreData GetScoreboard()
        {
            PopulationScoreData scoreData = new PopulationScoreData()
            {
                Height = _dummyUserData.Height,
                Width = _dummyUserData.Width,
                Xtopleft = _dummyUserData.Xtopleft,
                Ytopleft = _dummyUserData.Ytopleft,
                xList = _dummyUserData.xList,
                secretRealization = _secret
            };
            var scores = new List<UserResultFinal>();
            foreach (var user in _users)
            {
                
            }
            throw new NotImplementedException();
        }

    }
}
