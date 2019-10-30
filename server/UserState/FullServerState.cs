using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerDataStructures;
using ServerStateInterfaces;
using TrajectoryInterfaces;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace UserState
{
    public sealed class FullServerState : 
        ServerStateBase<WellPoint, UserData, UserState, TrueModelState, UserEvaluation, RealizationData>
    {
        private const string BotUserName = "DasBot-90-19-10-30";

        private readonly ObjectiveEvaluator _evaluatorClass = new ObjectiveEvaluator();
        private readonly DssBotGeneric<RealizationData> _bestSolutionFinder = 
            new DssBotGeneric<RealizationData>();
        //private readonly DssBotGeneric _dssSolutionFinderDiscounted = new DssBotGeneric();

        protected override ObjectiveEvaluationDelegateUser<UserData, WellPoint, UserEvaluation>
            .ObjectiveEvaluationFunction
            EvaluatorUser => _evaluatorClass.EvaluateDefault;

        protected override ObjectiveEvaluatorDelegateTruth<RealizationData, WellPoint>.ObjectiveEvaluationFunction
            EvaluatorTruth => _evaluatorClass.EvaluateOneRealizationDefault;

        private UserData _dummyUserData;

        public FullServerState()
        {
            _dummyUserData = GetNewDefaultUserPair("").UserData;
            InitializeNewSyntheticTruth(0);


            _scoreData = new PopulationScoreData<WellPoint, RealizationData>()
            {
                Height = _dummyUserData.Height,
                Width = _dummyUserData.Width,
                Xtopleft = _dummyUserData.Xtopleft,
                Ytopleft = _dummyUserData.Ytopleft,
                xList = _dummyUserData.xList,
                secretRealization = UserState.convertToRealizationData(_secret.TrueSubsurfaseModel1),
                TotalDecisionPoints = _dummyUserData.TotalDecisionPoints,
                Xdist = _dummyUserData.Xdist,
            };
            var bestTrajectoryWithScore = GetBestTrajectoryWithScore(GetTruthForEvaluation(),
                GetInitialPoint(),
                EvaluatorTruth);
            _scoreData.BestPossible = bestTrajectoryWithScore;
        }

        protected override TrueModelState InitializeNewSyntheticTruth(int seed = 0)
        {
            Console.WriteLine("Initialized synthetic truth with seed: " + seed);
            _secret = new TrueModelState(seed);
            DumpSectetStateToFile(seed);
            return _secret;
        }

        protected override WellPoint GetInitialPoint()
        {
            return _dummyUserData.wellPoints[0];
        }

        protected override UserResultFinal<WellPoint> GetBestTrajectoryWithScore(RealizationData secret,
            WellPoint start,
            ObjectiveEvaluatorDelegateTruth<RealizationData, WellPoint>.ObjectiveEvaluationFunction evaluator)
        {
            
            _bestSolutionFinder.Init(1.0,
                _scoreData.Xdist,
                _scoreData.TotalDecisionPoints - 1, 
                _dummyUserData.MaxAngleChange, 
                _dummyUserData.MinInclination,
                _evaluatorClass.EvaluatorDelegate
                );
            //_dssSolutionFinderDiscounted.Init(0.9,
            //    _scoreData.Xdist,
            //    _scoreData.TotalDecisionPoints,
            //    _dummyUserData.MaxAngleChange,
            //    _dummyUserData.MinInclination);
            var result = _bestSolutionFinder.ComputeBestDeterministicTrajectory(secret, start);
            
            var resultWithScore = UserState.GetEvaluationForTrajectoryAgainstTruth(result, evaluator, secret);
            var bestResult = new UserResultFinal<WellPoint>()
            {
                Stopped = true,
                TrajectoryWithScore = resultWithScore,
                UserName = "Best deterministic"
            };
            return bestResult;
        }

        protected UserScorePairLockedGeneric<UserState, UserData, TrueModelState, WellPoint,
            UserEvaluation, RealizationData> GetBotUserPair(string userKey)
        {
            var botPair = new UserStatePairWithBotFull(
                userKey,
                EvaluatorUser, 
                EvaluatorTruth, 
                _evaluatorClass.EvaluatorDelegate,
                GetTruthForEvaluation());
            botPair.Bot = new DssBotGeneric<RealizationData>();
            return botPair;
        }

        public override void AddBotUserDefault()
        {
            var userId = BotUserName;

            var user = _users.GetOrAdd(userId, GetNewDefaultUserPair);


        }

        protected override RealizationData GetTruthForEvaluation()
        {
            var secretModel = _secret.TrueSubsurfaseModel1;
            var result = UserState.convertToRealizationData(secretModel);
            return result;
        }





        //public bool UpdateUser(string userId, IContinousState updatePoint = default)
        //{
        //    if (!UserExists(userId))
        //    {
        //        return false;
        //    }

        //    var curUser = GetUser(userId);
        //    //TODO update at a ref point
        //    //TODO accept only certain points

        //    //var updatePoint = (ContinousState)load ?? curUser.GetNextStateDefault();


        //    Console.WriteLine("Using Default next point for the update: " + updatePoint);
        //    var result = curUser.OfferUpdatePoint(updatePoint, _syntheticTruth.GetData);
        //    //Console.WriteLine("Update successful: " + result);
        //    return result;
        //}


    }
}

