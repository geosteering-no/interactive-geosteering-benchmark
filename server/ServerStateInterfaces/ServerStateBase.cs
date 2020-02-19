using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public abstract class ServerStateBase<
        TWellPoint, TUserDataModel, TUserModel,
        TSecretState, TUserResult, TRealizationData> :
        IFullServerStateGeocontroller<
            TWellPoint, TUserDataModel, TUserResult, PopulationScoreData<TWellPoint, TRealizationData>>
        where TUserModel : IUserImplementaion<
            TUserDataModel, TWellPoint, TSecretState, TUserResult, TRealizationData>, new()

    {

        protected ConcurrentDictionary<string, int>
            _userGameNumber = new ConcurrentDictionary<string, int>();

        protected ConcurrentDictionary<string, UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint, TUserResult, TRealizationData>> 
            _users =
            new ConcurrentDictionary<string, UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint, TUserResult, TRealizationData>>();
        //protected ConcurrentDictionary<string, TUserModel> _users = new ConcurrentDictionary<string, TUserModel>();
        //protected ConcurrentDictionary<string, UserResultFinal<TWellPoint>> _userResults = new ConcurrentDictionary<string, UserResultFinal<TWellPoint>>();

        protected const string BotUserName = "DasBot 1030";

        protected const int TOTAL_LEVELS = 5;
        //protected TSecretState _secret = default;
        protected TSecretState[] _secrets = new TSecretState[5];
        //TODO update code for many secrets
        //TODO make a funciton that fatches secret for a user given their game number


        protected PopulationScoreData<TWellPoint, TRealizationData> _scoreData;

        protected abstract ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, TUserResult>.ObjectiveEvaluationFunction
            EvaluatorUser
        { get; }

        protected abstract ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction
            EvaluatorTruth
        { get; }



        private Random rnd = new Random();
        //4 is really bad 91 is bad
        //good seeds in 100: 101, 102?, 103!, 105
        //good seeds in 200: 201, 202, 203, 204, 205, 206, 207, 208, 209
        //202 bottom good bot 1948/3000
        //105 top bot gets 1739 2200
        //101 bottom bot top 963 / 1727
        //!!!!
        //103 bottom bit top 3100 / 4400
        //!!!!
        //201 bottom only 725 / 1500
        //205 bottom bot 1168 / 2568
        //206 nope
        //207 top bot 3327 / 4318


        private int[] seeds = {0,
            202,
            105, 105,
            103, 103,
            214, 214,
            209,
            214,
            213,
            215,
            227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 240, 241 };
        //private int[] seeds = {0, 91, 91, 10, 100};
        //private int[] seeds = { 0, 1, 91, 91, 10, 100, 3, 1, 4, 4, 5, 6, 7, 7, 8, 8, 8 };
        private int seedInd = 0;


        private int NextSeed()
        {
            var res = rnd.Next();
            seedInd++;
            if (seedInd < seeds.Length)
            {
                res = seeds[seedInd];
            }


            return res;
        }

        public ServerStateBase()
        {
            InitializeNewSyntheticTruth(0);
        }

        public void DumpScoreBoardToFile(PopulationScoreData<TWellPoint, TRealizationData> scoreBoard)
        {
            var dirId = "scoreLog/";
            if (!Directory.Exists(dirId))
            {
                Directory.CreateDirectory(dirId);
            }
            var jsonStr = JsonConvert.SerializeObject(scoreBoard);
            System.IO.File.WriteAllText(dirId + "/" + DateTime.Now.Ticks, jsonStr);
        }

        public PopulationScoreData<TWellPoint, TRealizationData> GetScoreboardFromFile(string fileName)
        {
            var dirId = "scoreLog/";
            var fullName = dirId + fileName;
            if (!File.Exists(fullName))
            {
                return null;
            }

            var fileString = File.ReadAllText(fullName);
            var scoreBoard = JsonConvert.DeserializeObject<PopulationScoreData<TWellPoint, TRealizationData>>
                (fileString);
            return scoreBoard;
        }

        public void DumpSectetStateToFile(int data)
        {
            var dirId = "serverstatelog/secret";
            if (!Directory.Exists(dirId))
            {
                Directory.CreateDirectory(dirId);
            }
            var jsonStr = JsonConvert.SerializeObject(data);
            System.IO.File.WriteAllText(dirId + "/" + DateTime.Now.Ticks, jsonStr);
        }

        /// <summary>
        /// this should call dump secret stateGeocontroller to file
        /// </summary>
        /// <param name="seed"></param>
        protected abstract TSecretState InitializeNewSyntheticTruth(int seed = 0);
        //{
        //    DumpSectetStateToFile(seed);
        //    //Console.WriteLine("Initialized synthetic truth with seed: " + seed);
        //    //_syntheticTruth = new TrueModelState(seed);
        //}

        private WellPointWithScore<TWellPoint> EvalueateSegmentAgainstTruth(TWellPoint p1, TWellPoint p2)
        {
            var pointWithScore = new WellPointWithScore<TWellPoint>()
            {
                wellPoint = p2,
            };

            var twoPoints = new List<TWellPoint>() { p1, p2 };
            var localScore = EvaluatorTruth(GetTruthForEvaluation(), twoPoints);
            pointWithScore.Score = localScore;
            return pointWithScore;
        }

        protected abstract TRealizationData GetTruthForEvaluation();

        protected UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint,
            TUserResult, TRealizationData> GetNewDefaultUserPair(string userKey)
        {
            return new UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint,
                TUserResult, TRealizationData>(
                userKey,
                EvaluatorUser, EvaluatorTruth, GetTruthForEvaluation());
        }

        public void StopAllUsers()
        {
            //_users;
            var userArray = _users.ToArray();
            foreach (var user in userArray)
            {
                var userKey = user.Key;
                _users.GetOrAdd(userKey, GetNewDefaultUserPair)
                    .StopUser(EvaluatorTruth, GetTruthForEvaluation());
            };
        }

        protected abstract TWellPoint GetInitialPoint();

        protected virtual UserResultFinal<TWellPoint> GetBestTrajectoryWithScore(TRealizationData secret,
            TWellPoint start,
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluator)
        {
            return null;
        }

        public void RestartServer(int seed = -1)
        {
            //TODO resart lock was here
            if (seed == -1)
            {
                seed = NextSeed();
            }

            //var users = _users;
            InitializeNewSyntheticTruth(seed);
            _scoreData.secretRealization = GetTruthForEvaluation();
            var bestTrajectoryWithScore = GetBestTrajectoryWithScore(GetTruthForEvaluation(),
                GetInitialPoint(),
                EvaluatorTruth);
            _scoreData.BestPossible = bestTrajectoryWithScore;

            var bestValueGame = 1.0;
            if (_scoreData.BestPossible?.TrajectoryWithScore != null)
            {
                var traj = _scoreData.BestPossible.TrajectoryWithScore;
                if (traj.Count > 0)
                {
                    bestValueGame = Math.Max(bestValueGame, traj[traj.Count - 1].Score);
                }
            }

            var userArray = _users.ToArray();
            foreach (var user in userArray)
            {
                var userKey = user.Key;
                _users.GetOrAdd(userKey, GetNewDefaultUserPair)
                    .MoveToNewGame(EvaluatorTruth, GetTruthForEvaluation(),
                        bestValueGame);
            }
        }

        public void ResetServer(int seed = -1)
        {
            //TODO implement the seed
            _users = new ConcurrentDictionary<string, 
                UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, 
                    TWellPoint, TUserResult, TRealizationData>>();
        }

        public TUserDataModel UpdateUser(string userId, TWellPoint load = default)
        {
            return _users.GetOrAdd(userId, GetNewDefaultUserPair)
                .UpdateUser(load, _secrets, EvaluatorTruth, GetTruthForEvaluation());
        }

        public TUserDataModel StopUser(string userId)
        {
            return _users.GetOrAdd(userId, GetNewDefaultUserPair)
                .StopUser(EvaluatorTruth, GetTruthForEvaluation());
        }

        public virtual TUserDataModel LossyCompress(TUserDataModel data)
        {
            return data;
        }

        public abstract void AddBotUserDefault();


        public bool UserExists(string userId)
        {
            return _users.ContainsKey(userId);
        }

        public TUserDataModel GetUserData(string userId)
        {
            return _users.GetOrAdd(userId, GetNewDefaultUserPair)
                .UserData;
        }

        public TUserResult GetUserEvaluationData(string userId, IList<TWellPoint> trajectory)
        {
            return _users.GetOrAdd(userId, GetNewDefaultUserPair)
                .GetEvalaution(trajectory);
        }

        public PopulationScoreData<TWellPoint, TRealizationData> GetScoreboard()
        {
            if (_users != null)
            {
                //var results = _users.Values.AsParallel()
                //    .Select(userValue => userValue.Score)
                //    .ToList();
                //var userList = _users.Values.ToList();
                var userArray = _users.ToArray();
                var results = new List<UserResultFinal<TWellPoint>>(userArray.Length);
                foreach (var user in userArray)
                {
                    var userPair = user.Value;
                    results.Add(userPair.Score);
                }
                _scoreData.UserResults = results;
                if (UserExists(BotUserName))
                {
                    _scoreData.BotResult = _users[BotUserName].Score;
                }
            }
            DumpScoreBoardToFile(_scoreData);
            return _scoreData;
        }
    }
}
