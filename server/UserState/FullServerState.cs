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

        private const int TotalTruths = 4;
        private const double ExtraHeight = 5;
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
            InitializeNewSyntheticTruths(0);


            _scoreDataAll = new PopulationScoreData<WellPoint, RealizationData>()
            {
                Height = _dummyUserData.Height + ExtraHeight,
                Width = _dummyUserData.Width,
                Xtopleft = _dummyUserData.Xtopleft,
                Ytopleft = _dummyUserData.Ytopleft - ExtraHeight,
                xList = _dummyUserData.xList,
                secretRealizations = _secrets.Select(secret => 
                    UserState.convertToRealizationData(secret.TrueSubsurfaseModel1)).ToList(),
                TotalDecisionPoints = _dummyUserData.TotalDecisionPoints,
                Xdist = _dummyUserData.Xdist,
            };

            var bestTrajectoryWithScore = GetBestTrajectoryWithScore(GetTruthsForEvaluation(),
                GetInitialPoint(),
                EvaluatorTruth);
            _scoreDataAll.BestPossible = bestTrajectoryWithScore;
        }

        protected override TrueModelState[] InitializeNewSyntheticTruths(int seed = 0)
        {
            Console.WriteLine("Initialized synthetic truth with seed: " + seed);
            Console.WriteLine("\n\n\n Seed: " + seed + "\n\n\n");
            _secrets = new TrueModelState[TotalTruths];
                new TrueModelState(seed); 
            DumpSectetStateToFile(seed);
            return _secrets;
        }

        public override UserData LossyCompress(UserData data)
        {
            
            //the data is a freshly generated object so we can feedle with it in place
            foreach (var realization in data.realizations)
            {
                for (var i = 0; i < realization.YLists.Count; i++)
                {
                    var realizationYList = realization.YLists[i];
                    var newList = new List<double>(realizationYList.Count);
                    foreach (var d in realizationYList)
                    {
                        newList.Add(Math.Round(d, 1));
                    }

                    realization.YLists[i] = newList;
                }

                //realization.XList = null;
            }
            

            return data;
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
                _scoreDataAll.Xdist,
                _scoreDataAll.TotalDecisionPoints - 1, 
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
            throw new NotImplementedException();
            //var botPair = new UserStatePairWithBotFull(
            //    userKey,
            //    EvaluatorUser, 
            //    EvaluatorTruth, 
            //    _evaluatorClass.EvaluatorDelegate,
            //    GetTruthForEvaluation());
            //botPair.Bot = new DssBotGeneric<RealizationData>();
            //return botPair;
        }

        public override void AddBotUserDefault()
        {
            var userId = BotUserName;
            
            var user = _users.GetOrAdd(userId, GetBotUserPair);
            var botUser = (UserStatePairWithBotFull) user;
            if (botUser != null)
            {
                throw new NotImplementedException();
                //botUser.StartBot(
                //    _secret,
                //    EvaluatorTruth,
                //    GetTruthForEvaluation()
                //    );
            }

        }

        private RealizationData GetSingleTruthForEvaluation(TrueModelState secret)
        {
            var secretModel = secret.TrueSubsurfaseModel1;
            var result = UserState.convertToRealizationData(secretModel);
            return result;
        }

        //TODO convert to be a static function of _secret
        protected override IList<RealizationData> GetTruthsForEvaluation()
        { 
            var result = _secrets.Select(secret => GetSingleTruthForEvaluation(secret)).ToList();
            return result;
        }





        //public bool UpdateUserLocked(string userId, IContinousState updatePoint = default)
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

