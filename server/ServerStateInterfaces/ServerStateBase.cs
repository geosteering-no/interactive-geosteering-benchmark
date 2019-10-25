using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public abstract class ServerStateBase<
        TWellPoint, TUserDataModel, TUserModel,
        TSecretState, TRealizationData> :
        IFullServerStateGeocontroller<
            TWellPoint, TUserDataModel, UserResultFinal<TWellPoint>, PopulationScoreData<TWellPoint>>
        where TUserModel : IUserImplementaion<
            TUserDataModel, TWellPoint, TSecretState, UserResultFinal<TWellPoint>, TRealizationData>, new()

    {
        private ConcurrentDictionary<string, UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint, TRealizationData>> _users =
            new ConcurrentDictionary<string, UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint, TRealizationData>>();
        //protected ConcurrentDictionary<string, TUserModel> _users = new ConcurrentDictionary<string, TUserModel>();
        //protected ConcurrentDictionary<string, UserResultFinal<TWellPoint>> _userResults = new ConcurrentDictionary<string, UserResultFinal<TWellPoint>>();

        private object _restartLock = new object();


        protected TSecretState _secret = default;


        protected PopulationScoreData<TWellPoint> _scoreData;

        protected abstract ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, UserResultFinal<TWellPoint>>.ObjectiveEvaluationFunction
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

        public void DumpScoreBoardToFile(PopulationScoreData<TWellPoint> scoreBoard)
        {
            var dirId = "scoreLog/";
            if (!Directory.Exists(dirId))
            {
                Directory.CreateDirectory(dirId);
            }
            var jsonStr = JsonConvert.SerializeObject(scoreBoard);
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



        /// <summary>
        /// Gets or ad
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private TUserModel GetOrAddUser(string userId)
        { 
            throw new NotImplementedException();
        }

        private UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint,
            TRealizationData> DefaultNewUserPair(string userKey)
        {
            return new UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint,
                TRealizationData>(
                userKey,
                EvaluatorUser);
        }

        public void RestartServer(int seed = -1)
        {
            lock (_restartLock)
            {
                
            
            if (seed == -1)
            {
                seed = NextSeed();
            }

            InitializeNewSyntheticTruth(seed);
            Parallel.ForEach(_users.Keys, userKey =>
                {
                    _users.GetOrAdd(userKey, DefaultNewUserPair)
                        .MoveToNewGame(EvaluatorTruth, GetTruthForEvaluation());
                });

            }

        }

        public void StopAllUsers()
        {
            var userList = _users.Keys.ToList();
            foreach (var userId in userList)
            {
                StopUser(userId);
            }
        }

        public void ResetServer()
        {
            _users = new ConcurrentDictionary<string, IUserScorePair<TUserDataModel, UserResultFinal<TWellPoint>>>();
        }

        public TUserDataModel UpdateUser(string userId, TWellPoint load = default)
        {
            if (!UserExists(userId))
            {
                throw new Exception("Incorrect user name " + userId);
            }

            var user = GetOrAddUser(userId);
            UserResultFinal<TWellPoint> newScore;
            TUserDataModel newUserData;

            //TODO unlock
            lock (user)
            {
                if (user.Stopped)
                {
                    return user.UserData;
                }

                var ok = user.UpdateUser(load, _secret);
                if (ok)
                {
                    newScore = GetDefaultUserResult(userId, user);
                    newUserData = user.UserData;
                    //var userResult = _userResults.
                    DumpUserStateToFile(userId, user.UserData, "Update");
                    return newUserData;
                }
            }
            //TODO unlock
            var userScore = _userResults.GetOrAdd(userId, newScore);
            lock (userScore)
            {
                userScore.TrajectoryWithScore = newScore.TrajectoryWithScore;
            }

            throw new Exception("User point was not accepted ");
        }

        public TUserDataModel StopUser(string userId)
        {
            var user = GetOrAddUser(userId);
            //TODO unlock
            lock (user)
            {
                user.StopDrilling();
                // TODO unlock
                var newScore = GetDefaultUserResult();
                var userScore = _userResults.GetOrAdd(userId, newScore);
                lock (userScore)
                {
                    userScore.Stopped = true;
                }

                DumpUserStateToFile(userId, user.UserData, "Stop");
                return user.UserData;
            }
        }


        public bool UserExists(string userId)
        {
            return _users.ContainsKey(userId);
        }

        public TUserDataModel GetOrAddUserState(string userId)
        {
            var user =  GetOrAddUser(userId);
            //TODO locking user when accessing the data
            lock (user)
            {
                var userData = user.UserData;
                DumpUserStateToFile(userId, userData);
                return userData;
            }
            
        }

        public TUserResult GetUserEvaluationData(string userId, IList<TWellPoint> trajectory)
        {
            var user = GetOrAddUser(userId);
            // TODO unlock
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

                //TODO check that it does not crash here
                _scoreData.UserResults = scores;
            }
            DumpScoreBoardToFile(_scoreData);
            return _scoreData;
        }
    }
}
