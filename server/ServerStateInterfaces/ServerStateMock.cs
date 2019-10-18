using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public class ServerStateMock : ServerStateBase<WellPoint, UserData, UserStateMockBase, RealizationData, UserEvaluation, RealizationData>
    {
        private readonly ObjectiveEvaluator _evaluatorClass = new ObjectiveEvaluator();

        protected override ObjectiveEvaluationDelegateUser<UserData, WellPoint, UserEvaluation>.ObjectiveEvaluationFunction
            EvaluatorUser => _evaluatorClass.EvaluateDefault;

        protected override ObjectiveEvaluatorDelegateTruth<RealizationData, WellPoint>.ObjectiveEvaluationFunction
            EvaluatorTruth => _evaluatorClass.EvaluateOneRealizationDefault;

        PopulationScoreData _scoreData;

        private UserData _dummyUserData;

        public ServerStateMock() : base()
        {
            _dummyUserData = UserStateMockBase.CreateUserData();
            _scoreData = new PopulationScoreData()
            {
                Height = _dummyUserData.Height,
                Width = _dummyUserData.Width,
                Xtopleft = _dummyUserData.Xtopleft,
                Ytopleft = _dummyUserData.Ytopleft,
                xList = _dummyUserData.xList,
                secretRealization = _secret
            };
        }

        protected override void InitializeNewSyntheticTruth(int seed = 0)
        {
            var defaultUser = GetDefaultNewUser();
            _secret = defaultUser.UserData.realizations[seed % defaultUser.UserData.realizations.Count];
            DumpSectetStateToFile(seed);
        }

        protected override RealizationData GetTruthForEvaluation()
        {
            return _secret;
        }

        public override PopulationScoreData GetScoreboard()
        {
            //TODO check if can be moved to base class
            var scores = _userResults.Values.ToList();
            _scoreData.UserResults = scores;
            return _scoreData;
        }

    }
}
