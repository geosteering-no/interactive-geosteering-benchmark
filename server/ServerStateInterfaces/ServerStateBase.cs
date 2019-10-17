﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public abstract class ServerStateBase<TWellPoint, TUserDataModel, TUserModel, TSecretState, TUserResult, TRealizationData> : 
        IFullServerState<
            TWellPoint, TUserDataModel, TUserResult, PopulationScoreData>
        where TUserModel : IUserImplementaion<
            TUserDataModel, TWellPoint, TSecretState, TUserResult>, new()
       
    {

        protected ConcurrentDictionary<string, TUserModel> _users = new ConcurrentDictionary<string, TUserModel>();
        protected TSecretState _secret = default;

        protected abstract ObjectiveEvaluationDelegate<TUserDataModel, TWellPoint, TUserResult>.ObjectiveEvaluationFunction
            Evaluator { get; }

        private Random rnd = new Random();
        private int[] seeds = {0, 91, 10, 100, 3, 1, 4, 4, 5, 6, 7, 7, 8, 8, 8};
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
            DumpUserStateToFile(userId, user.UserData);
            return res;
        }

        public TUserModel GetDefaultNewUser()
        {
            var newUser = new TUserModel()
            {
                //TODO here is a bunch of hard-coded things
                Evaluator = Evaluator
            };
            return newUser;
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
        /// this should call dump secret state to file
        /// </summary>
        /// <param name="seed"></param>
        protected abstract void InitializeNewSyntheticTruth(int seed = 0);
        //{
        //    DumpSectetStateToFile(seed);
        //    //TODO fix
        //    //Console.WriteLine("Initialized synthetic truth with seed: " + seed);
        //    //_syntheticTruth = new TrueModelState(seed);
        //}

        protected TUserModel GetOrAddUser(string userId)
        {
            //TODO check if default works
            var doLog = !UserExists(userId);
            var user = _users.GetOrAdd(userId, GetDefaultNewUser());
            if (doLog)
            {
                DumpUserStateToFile(userId, user.UserData);
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
            foreach (var user in _users)
            {
                var newUserState = GetDefaultNewUser();
                while (!newDict.TryAdd(user.Key, newUserState))
                {
                    //will add everything eventually
                }
                DumpUserStateToFile(user.Key, newUserState.UserData);
            }

            //put a clean user state
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
            var ok = user.UpdateUser(load, _secret);
            if (ok)
            {
                var newUserData = user.UserData;
                DumpUserStateToFile(userId, user.UserData);
                return newUserData;
            }
            throw new Exception("User point was not accepted ");
        }

        public TUserDataModel StopUser(string userId)
        {
            var user = GetOrAddUser(userId);
            user.StopDrilling();
            DumpUserStateToFile(userId, user.UserData);
            return user.UserData;
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
            var result = user.GetEvaluation(trajectory);
            return result;
        }

        //protected abstract TRealizationData GetSecretUserState(TSecretState secret);

        public abstract PopulationScoreData GetScoreboard();
    }
}
