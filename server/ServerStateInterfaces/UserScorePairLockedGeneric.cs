using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public class UserScorePairLockedGeneric<TUserModel, TUserDataModel,
            TSecretState, TWellPoint,
            TUserEvaluation, TRealizationData>
        where TUserModel : IUserImplementaion<
            TUserDataModel, TWellPoint, TSecretState,
            TUserEvaluation, TRealizationData>, new()
    {

        public UserScorePairLockedGeneric(string userName,
            ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, TUserEvaluation>.ObjectiveEvaluationFunction
                EvaluatorUser,
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth,
            TRealizationData trueRealization)
        {
            _user = GetUserDefault(userName, EvaluatorUser);
            _score = GetResultEmpty(userName);
            _score.TrajectoryWithScore = GetUserTrajectoryWithScore(_user, evaluatorTruth, trueRealization);
        }

        /// <summary>
        /// locked
        /// </summary>
        public TUserDataModel UserData
        {
            get
            {
                TUserDataModel newUserData;
                lock (this)
                {
                    newUserData = _user.UserData;
                }
                DumpUserStateToFile(_UserIdPrivate, newUserData);
                return newUserData;
            }
        }



        /// <summary>
        /// locked
        /// </summary>
        public UserResultFinal<TWellPoint> Score
        {
            get
            {
                var copyOfResult = new UserResultFinal<TWellPoint>();
                lock (this)
                {
                    copyOfResult.TrajectoryWithScore = _score.TrajectoryWithScore;
                    copyOfResult.Stopped = _score.Stopped;
                    copyOfResult.UserName = _score.UserName;
                    copyOfResult.AccumulatedScoreFromPreviousGames = _score.AccumulatedScoreFromPreviousGames;
                    copyOfResult.AccumulatedScorePercentFromPreviousGames =
                        _score.AccumulatedScorePercentFromPreviousGames;
                }
                return copyOfResult;
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
            TUserDataModel newUserData = default;

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
                    newUserData = _user.UserData;
                    //var userResult = _userResults.

                }

            }
            DumpUserStateToFile(_UserIdPrivate, newUserData, "Update");
            return newUserData;

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
            TUserDataModel newUserData;
            lock (this)
            {
                if (_Stopped)
                {
                    return _user.UserData;
                }

                _user.StopDrilling();
                _score.Stopped = true;

                var newTrajWithScore = GetUserTrajectoryWithScore(_user, evaluatorTruth, trueRealization);
                _score.TrajectoryWithScore = newTrajWithScore;
                newUserData = _user.UserData;
            }
            DumpUserStateToFile(_UserIdPrivate, newUserData, "Stop");
            return newUserData;
        }

        /// <summary>
        /// locked
        /// does not update user scores for current game
        /// </summary>
        /// <param name="evaluatorTruth"></param>
        /// <param name="newTrueRealization"></param>
        public void MoveToNewGame(
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth,
            TRealizationData newTrueRealization, double oldBest)
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
                _score.AccumulatedScorePercentFromPreviousGames += _score.TrajectoryWithScore[trajLen - 1].Score / oldBest;
                _user = newUser;
                _score.TrajectoryWithScore = newTrajectory;
                _score.Stopped = false;
            }
        }

        /// <summary>
        /// locked
        /// gets evaluation for the user given user trajectory
        /// </summary>
        public TUserEvaluation GetEvalaution(IList<TWellPoint> trajectory)
        {
            lock (this)
            {
                var evaluation = _user.GetEvaluation(trajectory);
                return evaluation;
            }
        }



        #region staticMembers

        private static void DumpUserStateToFile(string userId, TUserDataModel data, string suffix = "")
        {
            var hashString = string.Format("{0:X}", userId.GetHashCode());
            var strMaxLen = 15;
            var userDirName = userId.Trim();
            if (userDirName.Length > strMaxLen)
            {
                userDirName = userDirName.Remove(strMaxLen);
            }
            foreach (var ch in Path.GetInvalidFileNameChars())
            {
                userDirName = userDirName.Replace(ch, '-');
            }

            userDirName = userDirName + "_" + hashString;

            var dirId = "userLog/" + userDirName;
            if (!Directory.Exists(dirId))
            {
                Directory.CreateDirectory(dirId);
            }
            var jsonStr = JsonConvert.SerializeObject(data);
            System.IO.File.WriteAllText(dirId + "/" + DateTime.Now.Ticks + "_" + suffix, jsonStr);
        }

        private static TUserModel GetUserDefault(string userName,
            ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, TUserEvaluation>.ObjectiveEvaluationFunction evaluatorUser)
        {
            var newUser = new TUserModel()
            {
                //TODO here is a bunch of hard-coded things
                Evaluator = evaluatorUser
            };
            return newUser;
        }

        private static UserResultFinal<TWellPoint> GetResultEmpty(string userName)
        {
            var result = new UserResultFinal<TWellPoint>()
            {
                UserName = userName,
                AccumulatedScoreFromPreviousGames = 0,
                AccumulatedScorePercentFromPreviousGames = 0,
                Stopped = false,
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

        private ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, TUserEvaluation>.
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
            var newUserResult = GetResultEmpty(_UserIdPrivate);
            newUserResult.TrajectoryWithScore = GetUserTrajectoryWithScore(
                _user,
                evaluatorTruth,
                trueRealization);
            return newUserResult;
        }


        #endregion
    }
}