using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public abstract class ServerStateBase<
        TWellPoint, TUserDataModel, TUserModel,
        TSecretState, TUserResult, TRealizationData> :
        IFullServerStateGeocontroller<
            TWellPoint, TUserDataModel, TUserResult, LevelDescription<TWellPoint, TRealizationData, TSecretState>>
        where TUserModel : IUserImplementaion<
            TUserDataModel, TWellPoint, TSecretState, TUserResult, TRealizationData>, new()

    {
        protected ConcurrentDictionary<string, UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint, TUserResult, TRealizationData>> 
            _users =
                new ConcurrentDictionary<string, UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint, TUserResult, TRealizationData>>();
        //protected IList<LevelDescription<TWellPoint, TRealizationData>> _scoreDataAll;
        protected ConcurrentDictionary<UserResultId, UserResultFinal<TWellPoint>> 
            _resultingTrajectories = 
                new ConcurrentDictionary<UserResultId, UserResultFinal<TWellPoint>>();
        /// <summary>
        /// We never write to this one
        /// </summary>
        protected readonly LevelDescription<TWellPoint, TRealizationData, TSecretState>[] _levelDescriptions =
            new LevelDescription<TWellPoint, TRealizationData, TSecretState>[TOTAL_LEVELS];

        /// <summary>
        /// 
        /// </summary>
        protected readonly TSecretState[] _secrets = new TSecretState[TOTAL_LEVELS];
            

        protected const string BotUserName = "DasBot 1030";

        protected const int TOTAL_LEVELS = 5;
        //protected TSecretState _secret = default;

        //TODO Generate secret states
        //TODO update code for many secrets
        //TODO make a funciton that fatches secret for a user given their game number
        private int _seedInd = 0;


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


        protected int[] seeds = {0,
            //202,
            //105, //105,
            //103, //103,
            214, //214,
            209,
            //214,
            213,
            215,
            227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 240, 241 };
        //private int[] seeds = {0, 91, 91, 10, 100};
        //private int[] seeds = { 0, 1, 91, 91, 10, 100, 3, 1, 4, 4, 5, 6, 7, 7, 8, 8, 8 };
        


        private int NextSeed()
        {
            var res = rnd.Next();
            _seedInd++;
            if (_seedInd < seeds.Length)
            {
                res = seeds[_seedInd];
            }


            return res;
        }

        public ServerStateBase()
        {
            //TODO check if works without
            InitializeNewSyntheticTruths();
        }

        public void DumpScoreBoardToFile(LevelDescription<TWellPoint, TRealizationData, TSecretState> scoreBoard, 
            string dirId="scoreLog/")
        {
            if (!Directory.Exists(dirId))
            {
                Directory.CreateDirectory(dirId);
            }
            var jsonStr = JsonConvert.SerializeObject(scoreBoard);
            System.IO.File.WriteAllText(dirId + "/" + DateTime.Now.Ticks, jsonStr);
        }

        public void DumpScoreBoardToFile(IList<LevelDescription<TWellPoint, TRealizationData, TSecretState>> scoreBoardMulty)
        {
            var dirId = "scoreLog/";
            if (!Directory.Exists(dirId))
            {
                Directory.CreateDirectory(dirId);
            }

            var i = 0;
            //TODO make it to random seed that generates truth
            foreach (var scoreData in scoreBoardMulty)
            {
                DumpScoreBoardToFile(scoreData, dirId+i+"/");
                i++;
            }

        }


        public LevelDescription<TWellPoint, TRealizationData, TSecretState> GetScoreboardFromFile(string fileName)
        {
            var dirId = "scoreLog/";
            var fullName = dirId + fileName;
            if (!File.Exists(fullName))
            {
                return null;
            }

            var fileString = File.ReadAllText(fullName);
            var scoreBoard = JsonConvert.DeserializeObject<LevelDescription<TWellPoint, TRealizationData, TSecretState>>
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
        protected abstract TSecretState[] InitializeNewSyntheticTruths();
        //{
        //    DumpSectetStateToFile(seed);
        //    //Console.WriteLine("Initialized synthetic truth with seed: " + seed);
        //    //_syntheticTruth = new TrueModelState(seed);
        //}

        private WellPointWithScore<TWellPoint> EvalueateSegmentAgainstTruth(TWellPoint p1, TWellPoint p2)
        {
            throw new NotImplementedException();
            //var pointWithScore = new WellPointWithScore<TWellPoint>()
            //{
            //    wellPoint = p2,
            //};

            //var twoPoints = new List<TWellPoint>() { p1, p2 };
            //var localScore = EvaluatorTruth(GetTruthForEvaluation(), twoPoints);
            //pointWithScore.Score = localScore;
            //return pointWithScore;
        }

        protected abstract IList<TRealizationData> GetTruthsForEvaluation();

        protected UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint,
            TUserResult, TRealizationData> GetNewDefaultUserPair(string userKey)
        {
            //TODO here we need to create synthetic truth?
            return new UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint,
                TUserResult, TRealizationData>(
                userKey,
                EvaluatorUser, EvaluatorTruth, GetTruthsForEvaluation());
        }

        protected abstract TWellPoint GetInitialPoint();

        //protected virtual UserResultFinal<TWellPoint> GetBestTrajectoryWithScore(TRealizationData secret,
        //    TWellPoint start,
        //    ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluator)
        //{
        //    return null;
        //}

        public void ResetServer(int seed = -1)
        {
            //TODO implement the seed
            _users = new ConcurrentDictionary<string, 
                UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, 
                    TWellPoint, TUserResult, TRealizationData>>();
        }

        public TUserDataModel UpdateUser(string userId, TWellPoint load = default)
        {
            var userPair = _users.GetOrAdd(userId, GetNewDefaultUserPair);
            var updatedUser = userPair.UpdateUserLocked(load, _secrets, EvaluatorTruth, GetTruthsForEvaluation());
            var pair = userPair.GetUserResultScorePairLocked();
            _resultingTrajectories.AddOrUpdate(pair.Key, pair.Value,
                (key, value) => pair.Value);
            return updatedUser;
        }


        public static double GetFinalScore(UserResultFinal<TWellPoint> userResult)
        {
            return userResult.TrajectoryWithScore[userResult.TrajectoryWithScore.Count - 1].Score;
        }

        public IList<UserResultFinal<TWellPoint>> GetUserResultsForGame(int serverGameIndex)
        {
            serverGameIndex %= _levelDescriptions.Length;
            var selectedKeys = _resultingTrajectories.Keys.Where(key => key.GameId == serverGameIndex);
            var currentScores = new List<UserResultFinal<TWellPoint>>();
            foreach (var userResultId in selectedKeys)
            {
                UserResultFinal<TWellPoint> curTrajectory;
                var result = _resultingTrajectories.TryGetValue(userResultId, out curTrajectory);
                if (result)
                {
                    currentScores.Add(curTrajectory);
                }
            }

            return currentScores;
        }

        public double GetPercentile100ForGame(int serverGameIndex, double myScore)
        {
            serverGameIndex %= _levelDescriptions.Length;
            var results = GetUserResultsForGame(serverGameIndex);
            double lower = results.Select(GetFinalScore).Count(value => value < myScore);
            var total = results.Count;
            return lower / total * 100.0;
        }

        public double GetScorePercentForGame(int serverGameIndex, double myScore)
        {
            serverGameIndex %= _levelDescriptions.Length;
            var best = GetFinalScore(_levelDescriptions[serverGameIndex].BestPossible);
            return myScore / best * 100.0;
        }

        public MyScore GetMyFullScore(int serverGameIndex, double valueScore)
        {
            var score = new MyScore()
            {
                ScorePercent = GetScorePercentForGame(serverGameIndex, valueScore),
                ScoreValue = valueScore,
                YouDidBetterThan = GetPercentile100ForGame(serverGameIndex, valueScore)
            };
            return score;
        }

        public MyScore StopUser(string userId)
        {
            var userPair = _users.GetOrAdd(userId, GetNewDefaultUserPair);
            var updatedUser = userPair.StopUserLocked(EvaluatorTruth, GetTruthsForEvaluation());
            var pair = userPair.GetUserResultScorePairLocked();
            _resultingTrajectories.AddOrUpdate(pair.Key, pair.Value,
                (key, value) => pair.Value);
            return GetMyFullScore(pair.Key.GameId, GetFinalScore(pair.Value));
        }

        public int MoveUserToNewGame(string userId)
        {
            return _users.GetOrAdd(userId, GetNewDefaultUserPair)
                .MoveUserToNewGameLocked(EvaluatorTruth, GetTruthsForEvaluation());
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
                .UserDataLocked;
        }

        public TUserResult GetUserEvaluationData(string userId, IList<TWellPoint> trajectory)
        {
            //TODO copy to the global map
            return _users.GetOrAdd(userId, GetNewDefaultUserPair)
                .GetEvalaution(trajectory);
        }

        public LevelDescription<TWellPoint, TRealizationData, TSecretState> GetScoreboard(int serverGameIndex)
        {
            throw new NotImplementedException();
            //if (_users != null)
            //{
            //    //var results = _users.Values.AsParallel()
            //    //    .Select(userValue => userValue.Score)
            //    //    .ToList();
            //    //var userList = _users.Values.ToList();
            //    var userArray = _users.ToArray();
            //    var results = new List<UserResultFinal<TWellPoint>>(userArray.Length);
            //    foreach (var user in userArray)
            //    {
            //        var userPair = user.Value;
            //        results.Add(userPair.Score);
            //    }
            //    _scoreDataAll[i].UserResults = results;
            //    if (UserExists(BotUserName))
            //    {
            //        _scoreDataAll[i].BotResult = _users[BotUserName].Score;
            //    }
            //}
            //DumpScoreBoardToFile(_scoreDataAll);
            //return _scoreDataAll[i];
        }
    }
}
