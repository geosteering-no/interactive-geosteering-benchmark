using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Newtonsoft.Json;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public abstract class ServerStateBase<
        TWellPoint, TUserDataModel, TUserModel, 
        TSecretState, TUserResult, TRealizationData> :
        IFullServerStateGeocontroller<
            TWellPoint, TUserDataModel, TUserResult, PopulationScoreData<TWellPoint>>
        where TUserModel : IUserImplementaion<
            TUserDataModel, TWellPoint, TSecretState, TUserResult, TRealizationData>, new()

    {

        protected ConcurrentDictionary<string, TUserModel> _users = new ConcurrentDictionary<string, TUserModel>();
        protected ConcurrentDictionary<string, UserResultFinal<TWellPoint>> _userResults = new ConcurrentDictionary<string, UserResultFinal<TWellPoint>>();
        protected TSecretState _secret = default;

        protected PopulationScoreData<TWellPoint> _scoreData;

        protected abstract ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, TUserResult>.ObjectiveEvaluationFunction
            EvaluatorUser
        { get; }

        protected abstract ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction
            EvaluatorTruth
        { get; }



        private Random rnd = new Random();
        private int[] seeds = { 0, 91, 10, 100, 3, 1, 4, 4, 5, 6, 7, 7, 8, 8, 8 };
        private int seedInd = 0;


        private int NextSeed()
        {
            var res = rnd.Next();
            if (seedInd < seeds.Length)
            {
                res = seeds[seedInd];
            }

            seedInd++;
            return res;
        }
        public ServerStateBase()
        {
            InitializeNewSyntheticTruth(0);
        }

        public bool AddUser(string userId)
        {
            var newUser = GetDefaultNewUser();
            var res = _users.TryAdd(userId, newUser);
            if (!res)
            {
                return false;
            }

            var user = GetOrAddUser(userId);
            lock (user)
            {
                DumpUserStateToFile(userId, user.UserData);
            }

            return res;
        }

        public TUserModel GetDefaultNewUser()
        {
            var newUser = new TUserModel()
            {
                //TODO here is a bunch of hard-coded things
                Evaluator = EvaluatorUser
            };
            return newUser;
        }

        public UserResultFinal<TWellPoint> GetDefaultUserResult()
        {
            var result = new UserResultFinal<TWellPoint>()
            {
                AccumulatedScoreFromPreviousGames = 0,
                Stopped = false,
                TrajectoryWithScore = new List<WellPointWithScore<TWellPoint>>()
            };
            return result;
            //throw new NotImplementedException();
        }


        public void DumpUserStateToFile(string userId, TUserDataModel data)
        {
            var dirId = "userLog/" + userId;
            if (!Directory.Exists(dirId))
            {
                Directory.CreateDirectory(dirId);
            }
            var jsonStr = JsonConvert.SerializeObject(data);
            System.IO.File.WriteAllText(dirId + "/" + DateTime.Now.Ticks, jsonStr);
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
        protected abstract void InitializeNewSyntheticTruth(int seed = 0);
        //{
        //    DumpSectetStateToFile(seed);
        //    //Console.WriteLine("Initialized synthetic truth with seed: " + seed);
        //    //_syntheticTruth = new TrueModelState(seed);
        //}

        protected WellPointWithScore<TWellPoint> EvalueateSegmentAgainstTruth(TWellPoint p1, TWellPoint p2)
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

        protected UserResultFinal<TWellPoint> GetDefaultUserResult(TUserModel user)
        {
            var newUserResult = GetDefaultUserResult();
            var trueRealization = GetTruthForEvaluation();
            newUserResult.TrajectoryWithScore = user.GetEvaluationForTruth(
                EvaluatorTruth,
                trueRealization);
            return newUserResult;
        }

        protected TUserModel GetOrAddUser(string userId)
        {
            //TODO check if default works
            var doLog = !UserExists(userId);
            var user = _users.GetOrAdd(userId, GetDefaultNewUser());
            lock (user)
            {
                if (doLog)
                {
                    //var userData = user.UserData;
                    var newUserResult = GetDefaultUserResult(user);
                    for (var i = 0; i < 100; ++i)
                    {
                        var res = _userResults.TryAdd(userId, newUserResult);
                        if (res)
                        {
                            break;
                        }
                    }

                    //log
                    DumpUserStateToFile(userId, user.UserData);
                }
            }

            return user;
        }


        public void RestartServer(int seed = -1)
        {
            if (seed == -1)
            {
                seed = NextSeed();
            }
            var newDict = new ConcurrentDictionary<string, TUserModel>();
            foreach (var userId in _users.Keys)
            {
                var newUserState = GetDefaultNewUser();
                var newUserResult = GetDefaultUserResult(newUserState);
                _userResults.AddOrUpdate(userId, GetDefaultUserResult(), (key, oldUserResult) =>
                {
                    double prevGameScore = 0;
                    if (oldUserResult.TrajectoryWithScore.Count > 0)
                    {
                        prevGameScore = oldUserResult.TrajectoryWithScore[oldUserResult.TrajectoryWithScore.Count - 1]
                            .Score;
                    }

                    oldUserResult.AccumulatedScoreFromPreviousGames += prevGameScore;
                    oldUserResult.TrajectoryWithScore = newUserResult.TrajectoryWithScore;
                    return oldUserResult;
                });
                for (var i = 0; i< 100; ++i)
                {
                    //will add everything eventually
                    var res = newDict.TryAdd(userId, newUserState);
                    //TODO throw something
                    if (res)
                    {
                        break;
                    }
                }
                DumpUserStateToFile(userId, newUserState.UserData);
            }

            //put a clean user stateGeocontroller
            _users = newDict;
            //put a new truth
            InitializeNewSyntheticTruth(seed);
        }

        public TUserDataModel UpdateUser(string userId, TWellPoint load = default)
        {
            if (!UserExists(userId))
            {
                throw new Exception("Incorrect user name " + userId);
            }

            var user = GetOrAddUser(userId);
            lock (user)
            {
                var ok = user.UpdateUser(load, _secret);
                if (ok)
                {
                    var newScore = GetDefaultUserResult(user);
                    var userScore = _userResults.GetOrAdd(userId, newScore);
                    lock (userScore)
                    {
                        userScore.TrajectoryWithScore = newScore.TrajectoryWithScore;
                    }

                    var newUserData = user.UserData;
                    //var userResult = _userResults.
                    DumpUserStateToFile(userId, user.UserData);
                    return newUserData;
                }
            }

            throw new Exception("User point was not accepted ");
        }

        public TUserDataModel StopUser(string userId)
        {
            var user = GetOrAddUser(userId);
            lock (user)
            {
                user.StopDrilling();
                var newScore = GetDefaultUserResult();
                var userScore = _userResults.GetOrAdd(userId, newScore);
                lock (userScore)
                {
                    userScore.Stopped = true;
                }

                DumpUserStateToFile(userId, user.UserData);
                return user.UserData;
            }
        }


        public bool UserExists(string userId)
        {
            return _users.ContainsKey(userId);
        }

        public TUserDataModel GetOrAddUserState(string userId)
        {
            return GetOrAddUser(userId).UserData;
        }

        public TUserResult GetUserEvaluationData(string userId, IList<TWellPoint> trajectory)
        {
            var user = GetOrAddUser(userId);
            lock (user)
            {
                var result = user.GetEvaluation(trajectory);
                return result;
            }
        }

        //protected abstract TRealizationData GetSecretUserState(TSecretState secret);

        //public abstract PopulationScoreData GetScoreboard();
        public PopulationScoreData<TWellPoint> GetScoreboard()
        {
            //TODO check if can be moved to base class
            if (_userResults != null)
            {
                var scores = _userResults.Values.ToList();
                _scoreData.UserResults = scores;
            }

            return _scoreData;
        }
    }
}
