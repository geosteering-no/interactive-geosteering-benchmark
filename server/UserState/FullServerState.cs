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
    public class FullServerState : ServerStateBase<IContinousState, UserData, UserState, TrueModelState, UserEvaluation>
    {
       

        public FullServerState()
        {
            InitializeNewSyntheticTruth();
        }

        protected sealed override void InitializeNewSyntheticTruth(int seed = 0)
        {
            
            Console.WriteLine("Initialized synthetic truth with seed: " + seed);
            _secret = new TrueModelState(seed);
            
            base.InitializeNewSyntheticTruth(seed);
        }



        //public bool UpdateUser(string userId, IContinousState updatePoint = default)
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
