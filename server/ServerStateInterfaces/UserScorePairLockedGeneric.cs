using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public class UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint, TRealizationData> 
        where TUserModel : IUserImplementaion<
        TUserDataModel, TWellPoint, TSecretState, UserResultFinal<TWellPoint>, TRealizationData>, new()
    {

        public UserScorePairLockedGeneric(string userName,
            ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, UserResultFinal<TWellPoint>>.ObjectiveEvaluationFunction EvaluatorUser)
        {
            _user = GetUserDefault(userName, EvaluatorUser);
            _score = GetResultDefault(userName);
        }

        /// <summary>
        /// locked
        /// </summary>
        public TUserDataModel UserData
        {
            get
            {
                lock (this)
                {
                    var newUserData = _user.UserData;
                    DumpUserStateToFile(_UserIdPrivate, newUserData);
                    return newUserData;
                }
            }
        }



        /// <summary>
        /// locked
        /// </summary>
        public UserResultFinal<TWellPoint> Score
        {
            get
            {
                lock (this)
                {
                    return (UserResultFinal<TWellPoint>) _score.Clone();
                }
            }
        }

        /// <summary>
        /// locked
        /// updates and evaluates user against truth 
        /// </summary>
        /// <param name="load"></param>
        /// <param name="secret"></param>
        /// <param name="evaluatorTruth"></param>
        /// <param name="trueRealization"></param>
        /// <returns></returns>
        public TUserDataModel UpdateUser(TWellPoint load, 
            TSecretState secret,
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth,
            TRealizationData trueRealization)
        {
            lock (this)
            {
                if (_Stopped)
                {
                    return _user.UserData;
                }

                var ok = _user.UpdateUser(load, secret);
                if (ok)
                {
                    var newTrajWithScore = GetUserTrajectoryWithScore(_user, evaluatorTruth, trueRealization);
                    _score.TrajectoryWithScore = newTrajWithScore;
                    var newUserData = _user.UserData;
                    //var userResult = _userResults.
                    DumpUserStateToFile(_UserIdPrivate, newUserData, "Update");
                    return newUserData;
                }
            }
            throw new Exception("The update point was not accepted");
        }

        /// <summary>
        /// locked
        /// stops and evaluates user against truth
        /// </summary>
        /// <param name="evaluatorTruth"></param>
        /// <param name="trueRealization"></param>
        /// <returns></returns>
        public TUserDataModel StopUser(
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth,
            TRealizationData trueRealization)
        {
            lock (this)
            {
                if (_Stopped)
                {
                    return _user.UserData;
                }

                _user.StopDrilling();
                _score.Stopped = true;

                var newTrajWithScore = GetUserTrajectoryWithScore(_user,evaluatorTruth, trueRealization);
                _score.TrajectoryWithScore = newTrajWithScore;
                var newUserData = _user.UserData;
                //var userResult = _userResults.
                DumpUserStateToFile(_UserIdPrivate, newUserData, "Stop");
                return newUserData;
            }
        }

        /// <summary>
        /// locked
        /// does not update user scores for current game
        /// </summary>
        /// <param name="evaluatorTruth"></param>
        /// <param name="newTrueRealization"></param>
        public void MoveToNewGame(
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth,
            TRealizationData newTrueRealization)
        {
            var newUser = GetUserDefault(_UserIdPrivate, _EvaluatorUser);
            var newTrajectory = GetUserTrajectoryWithScore(
                newUser,
                evaluatorTruth,
                newTrueRealization);

            lock (this)
            {
                var trajLen = _score.TrajectoryWithScore.Count;
                _score.AccumulatedScoreFromPreviousGames += _score.TrajectoryWithScore[trajLen - 1].Score;
                _user = newUser;
                _score.TrajectoryWithScore = newTrajectory;
            }
        }



        #region staticMembers

        private static void DumpUserStateToFile(string userId, TUserDataModel data, string suffix = "")
        {
            var dirId = "userLog/" + userId;
            if (!Directory.Exists(dirId))
            {
                Directory.CreateDirectory(dirId);
            }
            var jsonStr = JsonConvert.SerializeObject(data);
            System.IO.File.WriteAllText(dirId + "/" + DateTime.Now.Ticks + "_" + suffix, jsonStr);
        }
        private static TUserModel GetUserDefault(string userName,
            ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, UserResultFinal<TWellPoint>>.ObjectiveEvaluationFunction evaluatorUser)
        {
            var newUser = new TUserModel()
            {
                //TODO here is a bunch of hard-coded things
                Evaluator = evaluatorUser
            };
            return newUser;
        }

        private static UserResultFinal<TWellPoint> GetResultDefault(string userName)
        {
            var result = new UserResultFinal<TWellPoint>()
            {
                AccumulatedScoreFromPreviousGames = 0,
                Stopped = false,
                TrajectoryWithScore = new List<WellPointWithScore<TWellPoint>>()
            };
            return result;
        }

        private static IList<WellPointWithScore<TWellPoint>> GetUserTrajectoryWithScore(
            TUserModel user,
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth,
            TRealizationData trueRealization)
        {
            return user.GetEvaluationForTruth(
                evaluatorTruth,
                trueRealization);
        }


        #endregion


        #region privateMembers


        private TUserModel _user;
        private UserResultFinal<TWellPoint> _score;

        private bool _Stopped
        {
            get
            {
                return _score.Stopped;
            }
        }

        private string _UserIdPrivate
        {
            get
            {
                return _score.UserName;
            }
        } 

        private ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, UserResultFinal<TWellPoint>>.
            ObjectiveEvaluationFunction _EvaluatorUser => _user.Evaluator;

        /// <summary>
        /// Requires a user lock
        /// </summary>
        /// <param name="evaluatorTruth"></param>
        /// <param name="trueRealization"></param>
        /// <returns></returns>
        private UserResultFinal<TWellPoint> _GetUserResult(
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth,
            TRealizationData trueRealization)
        {
            var newUserResult = GetResultDefault(_UserIdPrivate);
            newUserResult.TrajectoryWithScore = GetUserTrajectoryWithScore(
                _user,
                evaluatorTruth,
                trueRealization);
            return newUserResult;
        }


        #endregion
    }
}