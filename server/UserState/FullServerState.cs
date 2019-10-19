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
    public class FullServerState : 
        ServerStateBase<IContinousState, UserData, UserState, TrueModelState, UserEvaluation, RealizationData>,
        IFullSerrverExtended<IContinousState, UserData, UserEvaluation, PopulationScoreData<IContinousState>>
    {
       

        public FullServerState()
        {
            InitializeNewSyntheticTruth(0);
        }

        protected override ObjectiveEvaluationDelegateUser<UserData, IContinousState, UserEvaluation>.ObjectiveEvaluationFunction EvaluatorUser
        {
            get
            {
                throw new NotImplementedException();
                //TODO look up in mock implementation
            }
        }

        protected override ObjectiveEvaluatorDelegateTruth<RealizationData, IContinousState>.ObjectiveEvaluationFunction EvaluatorTruth
        {
            get;
        }

        protected sealed override void InitializeNewSyntheticTruth(int seed = 0)
        {
            
            Console.WriteLine("Initialized synthetic truth with seed: " + seed);
            _secret = new TrueModelState(seed);
            
            DumpSectetStateToFile(seed);
        }

        protected override RealizationData GetTruthForEvaluation()
        {
            throw new NotImplementedException();
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
