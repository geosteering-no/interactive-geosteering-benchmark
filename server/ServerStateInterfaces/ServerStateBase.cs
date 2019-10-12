using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ServerStateInterfaces
{
    public class ServerStateBase<TWellPoint, TUserDataModel, TUserModel, TSecretState> : 
        IFullServerState<TWellPoint, TUserDataModel>
        where TUserModel : IUserImplementaion<TUserDataModel, TWellPoint, TSecretState>, new()
       
    {
        private ConcurrentDictionary<string, TUserModel> _users = new ConcurrentDictionary<string, TUserModel>();
        protected TSecretState _secret = default;

        public bool AddUser(string userId)
        {
            return _users.TryAdd(userId, new TUserModel());
        }

        protected virtual void InitializeNewSyntheticTruth(int seed = 0)
        {
            //Console.WriteLine("Initialized synthetic truth with seed: " + seed);
            //_syntheticTruth = new TrueModelState(seed);
        }

        protected TUserModel GetUser(string userId)
        {
            //TODO check if default works
            return _users.GetOrAdd(userId, new TUserModel());
        }

        public void RestartServer(int seed = 0)
        {
            var newDict = new ConcurrentDictionary<string, TUserModel>();
            foreach (var user in _users)
            {
                while (!newDict.TryAdd(user.Key, new TUserModel()))
                {
                    //will add everything eventually
                }
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

            var user = GetUser(userId);
            return user.UpdateUser(load, _secret);
        }

        public bool UserExists(string userId)
        {
            return _users.ContainsKey(userId);
        }

        public TUserDataModel GetUserState(string userId)
        {
            return GetUser(userId).UserData;
        }
    }
}
