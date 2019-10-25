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
        ServerStateBase<WellPoint, UserData, UserState, TrueModelState, UserEvaluation, RealizationData>
    {


        private readonly ObjectiveEvaluator _evaluatorClass = new ObjectiveEvaluator();

        protected override ObjectiveEvaluationDelegateUser<UserData, WellPoint, UserEvaluation>
            .ObjectiveEvaluationFunction
            EvaluatorUser => _evaluatorClass.EvaluateDefault;

        protected override ObjectiveEvaluatorDelegateTruth<RealizationData, WellPoint>.ObjectiveEvaluationFunction
            EvaluatorTruth => _evaluatorClass.EvaluateOneRealizationDefault;
        
        protected UserData _dummyUserData;

        public FullServerState()
        {
            _dummyUserData = GetNewDefaultUserPair("").UserData;
            InitializeNewSyntheticTruth(0);
            _scoreData = new PopulationScoreData<WellPoint>()
            {
                Height = _dummyUserData.Height,
                Width = _dummyUserData.Width,
                Xtopleft = _dummyUserData.Xtopleft,
                Ytopleft = _dummyUserData.Ytopleft,
                xList = _dummyUserData.xList,
                secretRealization = UserState.convertToRealizationData(_secret.TrueSubsurfaseModel1),
                TotalDecisionPoints = _dummyUserData.TotalDecisionPoints
            };
        }

        protected sealed override void InitializeNewSyntheticTruth(int seed = 0)
        {
            
            Console.WriteLine("Initialized synthetic truth with seed: " + seed);
            _secret = new TrueModelState(seed);
            
            DumpSectetStateToFile(seed);
        }

        protected override RealizationData GetTruthForEvaluation()
        {
            var secretModel = _secret.TrueSubsurfaseModel1;
            var result = UserState.convertToRealizationData(secretModel);
            return result;
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

