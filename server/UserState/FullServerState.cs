using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerStateInterfaces;
using TrajectoryInterfaces;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace UserState
{
    public class FullServerState : IFullServerState<IContinousState>
    {
        private TrueModelState _syntheticTruth = null;
        private ConcurrentDictionary<string, UserState> _users = new ConcurrentDictionary<string, UserState>();

        public FullServerState()
        {
            InitializeNewSyntheticTruth();
        }

        private void InitializeNewSyntheticTruth(int seed = 0)
        {
            Console.WriteLine("Initialized synthetic truth with seed: " + seed);
            _syntheticTruth = new TrueModelState(seed);
        }

        public bool AddUser(string userId)
        {
            return _users.TryAdd(userId, new UserState());
        }

        public bool UserExists(string userId)
        {
            return _users.ContainsKey(userId);
        }

        public bool UpdateUser(string userId, IContinousState updatePoint = default)
        {
            if (!UserExists(userId))
            {
                return false;
            }

            var curUser = _users.GetOrAdd(userId, new UserState());
            //TODO update at a ref point
            //TODO accept only certain points

            //var updatePoint = (ContinousState)load ?? curUser.GetNextStateDefault();


            Console.WriteLine("Using Default next point for the update: " + updatePoint);
            var result = curUser.OfferUpdatePoint(updatePoint, _syntheticTruth.GetData);
            //Console.WriteLine("Update successful: " + result);
            return result;
        }

        public void RestartServer(int seed = 0)
        {
            var newDict = new ConcurrentDictionary<string, UserState>();
            foreach (var user in _users)
            {
                while (!newDict.TryAdd(user.Key, new UserState()))
                {
                    //will add everything eventually
                }
            }

            //put a clean user state
            _users = newDict;
            //put a new truth
            InitializeNewSyntheticTruth(seed);

        }
    }
}
