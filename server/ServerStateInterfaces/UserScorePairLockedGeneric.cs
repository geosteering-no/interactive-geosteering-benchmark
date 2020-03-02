using Newtonsoft.Json;
using ServerDataStructures;
using System;
using System.Collections.Generic;
using System.IO;

namespace ServerStateInterfaces
{
    public class UserScorePairLockedGeneric<TUserModel, TUserDataModel,
            TSecretState, TWellPoint,
            TUserEvaluation, TRealizationData>
        where TUserModel : IUserImplementaion<
            TUserDataModel, TWellPoint, TSecretState,
            TUserEvaluation, TRealizationData>, new()
    {
        protected readonly int TotalLevelsOnServer;

        private object _thisUserLockObject = new Object();
        public UserScorePairLockedGeneric(string userName,
            ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, TUserEvaluation>.ObjectiveEvaluationFunction
                EvaluatorUser,
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth,
            IList<TRealizationData> trueRealizationsLevels)
        {
            _user = GetUserDefault(userName, EvaluatorUser);
            //TODO check if we need score for new game user...
            _score = GetResultEmpty(userName);
            TotalLevelsOnServer = trueRealizationsLevels.Count;
            _score.TrajectoryWithScore = GetUserTrajectoryWithScore(_user, evaluatorTruth, 
                _GetCurrentTrueRealization(trueRealizationsLevels));
        }

        /// <summary>
        /// locked
        /// </summary>
        public TUserDataModel UserDataLocked
        {
            get
            {
                TUserDataModel newUserData;
                lock (_thisUserLockObject)
                {
                    newUserData = _user.UserData;
                }
                //DumpUserStateToFile(_UserIdPrivate, newUserData);
                return newUserData;
            }
        }

        ///// <summary>
        ///// locked
        ///// </summary>
        //private int GameIndexLocked
        //{
        //    get
        //    {
        //        //we lock to prevent game advancement 
        //        // TODO consider removing
        //        lock (_thisUserLockObject)
        //        {
        //            return _gameNumber;
        //        }
        //    }
        //}

        private UserResultFinal<TWellPoint> ScoreUnlocked
        {
            get
            {
                var copyOfResult = new UserResultFinal<TWellPoint>();
                copyOfResult.Stopped = _score.Stopped;
                copyOfResult.UserName = _score.UserName;
                copyOfResult.TrajectoryWithScore = _score.TrajectoryWithScore;
                return copyOfResult;
            }
        }

        ///// <summary>
        ///// locked, marked for removal
        ///// </summary>
        //private UserResultFinal<TWellPoint> ScoreLocked
        //{
        //    get
        //    {
        //        var copyOfResult = new UserResultFinal<TWellPoint>();
        //        lock (_thisUserLockObject)
        //        {
        //            copyOfResult.TrajectoryWithScore = _score.TrajectoryWithScore;
        //            copyOfResult.Stopped = _score.Stopped;
        //            copyOfResult.UserName = _score.UserName;
        //            //copyOfResult.AccumulatedScoreFromPreviousGames = _score.AccumulatedScoreFromPreviousGames;
        //            //copyOfResult.AccumulatedScorePercentFromPreviousGames =
        //            //    _score.AccumulatedScorePercentFromPreviousGames;
        //        }
        //        return copyOfResult;
        //    }
        //}

        public static int CalculateHashInt(string read, int moduloInt = ModuloInt)
        {
            UInt64 hashedValue = 3074457345618258791ul;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457345618258799ul;
            }
            return (int) hashedValue % moduloInt;
        }

        private const int ModuloInt = 1000000009;

        protected virtual void GenerateLevelIdsForUser()
        {
            GameIds = new List<int>(TotalUniqueGameInds);
            var rnd = new Random(_MyHash);
            for (int i = 0; i < TotalUniqueGameInds; i++)
            {
                GameIds.Add(rnd.Next(ModuloInt));
            }

            GameIds[0] = 0;
            var repeatOption = rnd.Next(3);
            if (repeatOption == 0)
            {
                GameIds[2] = GameIds[1];
            }
            else if (repeatOption == 1)
            {
                GameIds[3] = GameIds[1];
            }
            else
            {
                GameIds[3] = GameIds[2];
            }
        }

        /// <summary>
        /// This returns a number that can be user modular, i.e. it is a non-negative integer
        /// </summary>
        /// <returns></returns>
        private int _GetLevelIdForUser()
        {
            return _GetLevelIdForUserForGameIndexSafe(_gameNumber);
        }

        private int _GetLevelIdForUserForGameIndexSafe(int gameIndex)
        {
            if (GameIds == null)
            {
                GenerateLevelIdsForUser();
            }

            return GameIds[gameIndex % GameIds.Count];

        }

        private TSecretState _GetCurrentSecretState(IList<TSecretState> secrets)
        {
            var truthIndex = _GetLevelIdForUser() % secrets.Count;
            return secrets[truthIndex];
        }


        private TRealizationData _GetCurrentTrueRealization(IList<TRealizationData> realizations)
        {
            var truthIndex = _GetLevelIdForUser() % realizations.Count;
            return realizations[truthIndex];
        }

        /// <summary>
        /// locked
        /// updates and evaluates user against truth 
        /// </summary>
        /// <param name="load"></param>
        /// <param name="secrets"></param>
        /// <param name="evaluatorTruth"></param>
        /// <param name="trueRealization"></param>
        /// <returns></returns>
        public TUserDataModel UpdateUserLocked(TWellPoint load,
            IList<TSecretState> secrets,
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth,
            IList<TRealizationData> trueRealizations)
        {
            TUserDataModel newUserData = default;

            lock (_thisUserLockObject)
            {
                if (_Stopped)
                {
                    return _user.UserData;
                }

                var ok = _user.UpdateUser(load, _GetCurrentSecretState(secrets));

                if (ok)
                {
                    var newTrajWithScore = GetUserTrajectoryWithScore(_user, evaluatorTruth, _GetCurrentTrueRealization(trueRealizations));
                    _score.TrajectoryWithScore = newTrajWithScore;
                    newUserData = _user.UserData;
                    //var userResult = _userResults.
                }

            }
            //DumpUserStateToFile(_UserIdPrivate, newUserData, "Update");
            return newUserData;

        }

        /// <summary>
        /// locked
        /// stops and evaluates user against truth
        /// </summary>
        /// <param name="evaluatorTruth"></param>
        /// <param name="trueRealization"></param>
        /// <returns></returns>
        public TUserDataModel StopUserLocked(
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth,
            IList<TRealizationData> trueRealizations)
        {
            TUserDataModel newUserData;
            lock (_thisUserLockObject)
            {
                if (_Stopped)
                {
                    return _user.UserData;
                }

                _user.StopDrilling();
                _score.Stopped = true;

                var newTrajWithScore = GetUserTrajectoryWithScore(_user, evaluatorTruth, _GetCurrentTrueRealization(trueRealizations));
                _score.TrajectoryWithScore = newTrajWithScore;
                newUserData = _user.UserData;
            }
            DumpUserStateToFile(_UserIdPrivate, newUserData, "Stop");
            return newUserData;
        }

        /// <summary>
        /// This function just moves but does not do anything else
        /// </summary>
        /// <returns></returns>
        public int MoveUserToNewGameLocked(
            ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth,
            IList<TRealizationData> trueRealizationLevels)
        {
            lock (_thisUserLockObject)
            {
                if (!_Stopped)
                {
                    return _gameNumber;
                }
                _gameNumber++;
                _user = GetUserDefault(_UserIdPrivate, _EvaluatorUser);
                _score.Stopped = false;
                _score.TrajectoryWithScore = GetUserTrajectoryWithScore(_user, evaluatorTruth,
                    _GetCurrentTrueRealization(trueRealizationLevels));
                //TODO see which other things to reinit
                //_score = GetResultEmpty(_score.UserName);
                //_score.TrajectoryWithScore = GetUserTrajectoryWithScore(_user, evaluatorTruth,
                //    _GetCurrentTrueRealization(trueRealizationsLevels));
                //_Stopped = false;
                return _gameNumber;
            }
        }

        ///// <summary>
        ///// locked
        ///// does not update user scores for current game
        ///// </summary>
        ///// <param name="evaluatorTruth"></param>
        ///// <param name="newTrueRealization"></param>
        //public void MoveToNewGame(
        //    ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluatorTruth,
        //    TRealizationData newTrueRealization, double oldBest)
        //{
        //    // TODO fix: look for deadlocks here
        //    var newUser = GetUserDefault(_UserIdPrivate, _EvaluatorUser);
        //    var newTrajectory = GetUserTrajectoryWithScore(
        //        newUser,
        //        evaluatorTruth,
        //        newTrueRealization);

        //    lock (_thisUserLockObject)
        //    {
        //        var trajLen = _score.TrajectoryWithScore.Count;
        //        _score.AccumulatedScoreFromPreviousGames += _score.TrajectoryWithScore[trajLen - 1].Score;
        //        _score.AccumulatedScorePercentFromPreviousGames += _score.TrajectoryWithScore[trajLen - 1].Score / oldBest;
        //        _user = newUser;
        //        _score.TrajectoryWithScore = newTrajectory;
        //        _score.Stopped = false;
        //    }
        //}

        /// <summary>
        /// locked
        /// gets evaluation for the user given user trajectory
        /// </summary>
        public TUserEvaluation GetEvalaution(IList<TWellPoint> trajectory)
        {
            //TODO add a proper callback to register trajectory
            lock (_thisUserLockObject)
            {
                var evaluation = _user.GetEvaluation(trajectory);
                _score.PlannedTrajectory = trajectory;
                return evaluation;
            }
        }



        #region staticMembers

        public delegate void RegisterScorePairCallback(KeyValuePair<UserResultId, UserResultFinal<TWellPoint>> pair);

        public KeyValuePair<UserResultId, UserResultFinal<TWellPoint>> GetUserResultScorePairLocked(int totalServerGames)
        {
            //locked here
            lock (_thisUserLockObject)
            {
                var gameInd = _gameNumber;
                var resultId = new UserResultId(_UserIdPrivate, gameInd,
                    _GetLevelIdForUserForGameIndexSafe(gameInd) % totalServerGames);
                var scoreCopy = ScoreUnlocked;
                return new KeyValuePair<UserResultId, UserResultFinal<TWellPoint>>(resultId, scoreCopy);
            }
        }

        private static void DumpUserStateToFile(string userId, TUserDataModel data, string suffix = "")
        {
            var hashString = string.Format("{0:X}", CalculateHashInt(userId));
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
                Evaluator = evaluatorUser,
            };
            return newUser;
        }

        private static UserResultFinal<TWellPoint> GetResultEmpty(string userName)
        {
            var result = new UserResultFinal<TWellPoint>()
            {
                UserName = userName,
                //AccumulatedScoreFromPreviousGames = 0,
                //AccumulatedScorePercentFromPreviousGames = 0,
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
        private int _gameNumber = 0;
        private int _myHash = -1;
        //private Random _myRnd;
        protected IList<int> GameIds;
        private const int TotalUniqueGameInds = 101;

        //private Random _MyRnd
        //{
        //    get
        //    {
        //        if (_MyRnd == null)
        //        {
        //            _myRnd = new Random(CalculateHashInt(_UserIdPrivate));
        //        }

        //        return _myRnd;
        //    }
        //}

        private int _MyHash
        {
            get
            {
                if (_myHash < 0)
                {
                    _myHash = CalculateHashInt(_UserIdPrivate);
                }

                return _myHash;
            }
        }

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