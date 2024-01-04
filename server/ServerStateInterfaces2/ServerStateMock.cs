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
            for (int i = 0; i < _levelDescriptions.Length; ++i)
            {
                _levelDescriptions[i] = new LevelDescription<WellPoint, RealizationData, RealizationData>()
                {
                    Height = _dummyUserData.Height,
                    Width = _dummyUserData.Width,
                    Xtopleft = _dummyUserData.Xtopleft,
                    Ytopleft = _dummyUserData.Ytopleft,
                    xList = _dummyUserData.xList,
                    secretRealization = _secrets[i],
                    TotalDecisionPoints = TOTAL_DECISION_STEPS,
                    Xdist = _dummyUserData.Xdist
                };
            }
        }

        protected override RealizationData[] InitializeNewSyntheticTruths()
        {
            throw new NotImplementedException();
            //var defaultUserData = GetNewDefaultUserPair("").UserDataLocked;
            //_secret = defaultUserData.realizations[seed % defaultUserData.realizations.Count];
            //DumpSectetStateToFile(seed);
            //return _secret;
        }

        protected override IList<RealizationData> GetTruthsForEvaluation()
        {
            throw new NotImplementedException();
            //return _secret;
        }

        protected WellPoint GetInitialPoint()
        {
            return _dummyUserData.wellPoints[0];
        }


        public override void AddBotUserDefault()
        {
            throw new NotImplementedException();
        }
    }
}
