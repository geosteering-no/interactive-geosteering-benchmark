using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerDataStructures;
using ServerStateInterfaces;

namespace UserState
{
    public class DssBotRunner
    {
        private readonly DssBotGeneric _dssSolutionFinderDiscounted = new DssBotGeneric();
        private readonly UserScorePairLockedGeneric<UserState, UserData, 
            TrueModelState, WellPoint, UserEvaluation, RealizationData> _userPair;
        //IUserImplementaion<UserData, WellPoint, TSecretState, UserEvaluation, RealizationData>

        public DssBotRunner(UserScorePairLockedGeneric<UserState, UserData,
                TrueModelState, WellPoint, UserEvaluation, RealizationData> userPair)
        {
            _userPair = userPair;

        }

        public void Run()
        {
            new Thread(() =>
            {
                RunDSS();
            });
        }

        public void RunDSS()
        {
            while (true)
            {
                //var userData = _userPair.UserData;
                //var totalLeft = userData.TotalDecisionPoints - userData.wellPoints.Count;
                //if (totalLeft <= 0)
                //{
                //    _userPair.StopUser();
                //}
                //_dssSolutionFinderDiscounted.Init(0.9,
                //    userData.Xdist,
                //    totalLeft,
                //    userData.MaxAngleChange,
                //    userData.MinInclination);
            }
        }

    }
}
