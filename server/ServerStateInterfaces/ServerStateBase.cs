using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public abstract class ServerStateBase<TWellPoint, TUserDataModel, TUserModel, TSecretState, TUserResult> : 
        IFullServerState<
            TWellPoint, TUserDataModel, TUserResult, PopulationScoreData>
        where TUserModel : IUserImplementaion<
            TUserDataModel, TWellPoint, TSecretState, TUserResult>, new()
       
    {

        private ConcurrentDictionary<string, TUserModel> _users = new ConcurrentDictionary<string, TUserModel>();
        protected TSecretState _secret = default;

        protected abstract ObjectiveEvaluationDelegate<TUserDataModel, TWellPoint, TUserResult>.ObjectiveEvaluationFunction
            Evaluator { get; }


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
                Evaluator = Evaluator
            };
            return newUser;
        }

        public void DumpUserStateToFile(string userId, TUserDataModel data)
        {
            var dirId = "userLod_" + userId;
            if (!Directory.Exists(dirId))
            {
                Directory.CreateDirectory(dirId);
            }
            var jsonStr = JsonConvert.SerializeObject(data);
            System.IO.File.WriteAllText(dirId + "/" + DateTime.Now.Ticks, jsonStr);
        }

        public void DumpSectetStateToFile(int data)
        {
            var dirId = "secret";
            if (!Directory.Exists(dirId))
            {
                Directory.CreateDirectory(dirId);
            }
            var jsonStr = JsonConvert.SerializeObject(data);
            System.IO.File.WriteAllText(dirId + "/" + DateTime.Now.Ticks, jsonStr);
        }


        protected virtual void InitializeNewSyntheticTruth(int seed = 0)
        {
            DumpSectetStateToFile(seed);
            //TODO fix
            //Console.WriteLine("Initialized synthetic truth with seed: " + seed);
            //_syntheticTruth = new TrueModelState(seed);
        }

        protected TUserModel GetOrAddUser(string userId)
        {
            //TODO check if default works
            return _users.GetOrAdd(userId, GetDefaultNewUser());
        }

        public void RestartServer(int seed = 0)
        {
            var newDict = new ConcurrentDictionary<string, TUserModel>();
            foreach (var user in _users)
            {
                var newUserState = new TUserModel();
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

        public bool UpdateUser(string userId, TWellPoint load = default)
        {
            if (!UserExists(userId))
            {
                return false;
            }

            var user = GetOrAddUser(userId);
            var res = user.UpdateUser(load, _secret);
            DumpUserStateToFile(userId, user.UserData);
            return res;
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

        public PopulationScoreData GetScoreboard()
        {
            throw new NotImplementedException();
        }
    }
}
