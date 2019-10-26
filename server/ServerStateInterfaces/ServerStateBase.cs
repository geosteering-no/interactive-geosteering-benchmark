﻿using System;
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
        TSecretState, TUserResult, TRealizationData> :
        IFullServerStateGeocontroller<
            TWellPoint, TUserDataModel, TUserResult, PopulationScoreData<TWellPoint, TRealizationData>>
        where TUserModel : IUserImplementaion<
            TUserDataModel, TWellPoint, TSecretState, TUserResult, TRealizationData>, new()

    {
        private ConcurrentDictionary<string, UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint, TUserResult, TRealizationData>> _users =
            new ConcurrentDictionary<string, UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint, TUserResult, TRealizationData>>();
        //protected ConcurrentDictionary<string, TUserModel> _users = new ConcurrentDictionary<string, TUserModel>();
        //protected ConcurrentDictionary<string, UserResultFinal<TWellPoint>> _userResults = new ConcurrentDictionary<string, UserResultFinal<TWellPoint>>();

        private object _restartLock = new object();


        protected TSecretState _secret = default;


        protected PopulationScoreData<TWellPoint, TRealizationData> _scoreData;

        protected abstract ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, TUserResult>.ObjectiveEvaluationFunction
            EvaluatorUser
        { get; }

        protected abstract ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction
            EvaluatorTruth
        { get; }



        private Random rnd = new Random();
        private int[] seeds = { 0, 4, 91, 91, 10, 100, 3, 1, 4, 4, 5, 6, 7, 7, 8, 8, 8 };
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
            var users = _users;
            Parallel.ForEach(users.Keys, userKey =>
            {
                users.GetOrAdd(userKey, GetNewDefaultUserPair)
                    .StopUser(EvaluatorTruth, GetTruthForEvaluation());
            });
        }

        public void RestartServer(int seed = -1)
        {
            lock (_restartLock)
            {
                if (seed == -1)
                {
                    seed = NextSeed();
                }

                var users = _users;
                InitializeNewSyntheticTruth(seed);
                _scoreData.secretRealization = GetTruthForEvaluation();
                Parallel.ForEach(users.Keys, userKey =>
                    {
                        users.GetOrAdd(userKey, GetNewDefaultUserPair)
                            .MoveToNewGame(EvaluatorTruth, GetTruthForEvaluation());
                    });

            }

        }



        public void ResetServer()
        {
            _users = new ConcurrentDictionary<string, UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint, TUserResult, TRealizationData>>();
        }

        public TUserDataModel UpdateUser(string userId, TWellPoint load = default)
        {
            return _users.GetOrAdd(userId, GetNewDefaultUserPair)
                .UpdateUser(load, _secret, EvaluatorTruth, GetTruthForEvaluation());
        }

        public TUserDataModel StopUser(string userId)
        {
            return _users.GetOrAdd(userId, GetNewDefaultUserPair)
                .StopUser(EvaluatorTruth, GetTruthForEvaluation());
        }

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
                var results = _users.Values.AsParallel()
                    .Select(userValue => userValue.Score)
                    .ToList();
                _scoreData.UserResults = results;
            }
            DumpScoreBoardToFile(_scoreData);
            return _scoreData;
        }
    }
}
