using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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


        private const int TOTAL_DECISION_STEPS = 10;




        protected UserData _dummyUserData;

        public ServerStateMock() : base()
        {
            _dummyUserData = UserStateMockBase.CreateUserData();
            _scoreDataAll = new PopulationScoreData<WellPoint, RealizationData>()
            {
                Height = _dummyUserData.Height,
                Width = _dummyUserData.Width,
                Xtopleft = _dummyUserData.Xtopleft,
                Ytopleft = _dummyUserData.Ytopleft,
                xList = _dummyUserData.xList,
                secretRealizations = _secrets,
                TotalDecisionPoints = TOTAL_DECISION_STEPS,
                Xdist = _dummyUserData.Xdist
            };
        }

        protected override RealizationData[] InitializeNewSyntheticTruths(int seed = 0)
        {
            throw new NotImplementedException();
            //var defaultUserData = GetNewDefaultUserPair("").UserData;
            //_secret = defaultUserData.realizations[seed % defaultUserData.realizations.Count];
            //DumpSectetStateToFile(seed);
            //return _secret;
        }

        protected override IList<RealizationData> GetTruthsForEvaluation()
        {
            throw new NotImplementedException();
            //return _secret;
        }

        protected override WellPoint GetInitialPoint()
        {
            return _dummyUserData.wellPoints[0];
        }

        public override void AddBotUserDefault()
        {
            throw new NotImplementedException();
        }
    }
}
