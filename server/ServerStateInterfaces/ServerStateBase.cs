using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using ServerDataStructures;
using static System.IO.Path;


namespace ServerStateInterfaces
{
    public abstract class ServerStateBase<
        TWellPoint, TUserDataModel, TUserModel,
        TSecretState, TUserResult, TRealizationData> :
        IFullServerStateGeocontroller<
            TWellPoint, TUserDataModel, TUserResult, LevelDescription<TWellPoint, TRealizationData, TSecretState>>
        where TUserModel : IUserImplementaion<
            TUserDataModel, TWellPoint, TSecretState, TUserResult, TRealizationData>, new()

    {
        protected ConcurrentDictionary<string, UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint, TUserResult, TRealizationData>> 
            _users =
                new ConcurrentDictionary<string, UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint, TUserResult, TRealizationData>>();
        //protected IList<LevelDescription<TWellPoint, TRealizationData>> _scoreDataAll;
        protected ConcurrentDictionary<UserResultId, UserResultFinal<TWellPoint>> 
            _resultingTrajectories = 
                new ConcurrentDictionary<UserResultId, UserResultFinal<TWellPoint>>();
        protected ConcurrentDictionary<UserResultId, UserResultFinal<TWellPoint>> 
            _newResultingTrajectories = 
                new ConcurrentDictionary<UserResultId, UserResultFinal<TWellPoint>>();
        /// <summary>
        /// We never write to this one
        /// </summary>
        protected readonly LevelDescription<TWellPoint, TRealizationData, TSecretState>[] _levelDescriptions =
            new LevelDescription<TWellPoint, TRealizationData, TSecretState>[TOTAL_LEVELS];

        /// <summary>
        /// 
        /// </summary>
        protected readonly TSecretState[] _secrets = new TSecretState[TOTAL_LEVELS];

        protected TUserDataModel _dummyUserData;


        protected const string BotUserName = "Alyaev et al.[2019]";

        protected const int TOTAL_LEVELS = 1;
        //protected TSecretState _secret = default;

        //TODO Generate secret states
        //TODO update code for many secrets
        //TODO make a funciton that fatches secret for a user given their game number
        private int _seedInd = 0;


        protected abstract ObjectiveEvaluationDelegateUser<TUserDataModel, TWellPoint, TUserResult>.ObjectiveEvaluationFunction
            EvaluatorUser
        { get; }

        protected abstract ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction
            EvaluatorTruth
        { get; }



        private Random rnd = new Random();
        //4 is really bad 91 is bad
        //good seeds in 100: 101, 102?, 103!, 105
        //good seeds in 200: 201, 202, 203, 204, 205, 206, 207, 208, 209
        //202 bottom good bot 1948/3000
        //105 top bot gets 1739 2200
        //101 bottom bot top 963 / 1727
        //!!!!
        //103 bottom bit top 3100 / 4400
        //!!!!
        //201 bottom only 725 / 1500
        //205 bottom bot 1168 / 2568
        //206 nope
        //207 top bot 3327 / 4318


        protected int[] seeds = {
            //original NFES
            //0,
            202,
            105, //105,
            //end of original NFES
            //103, //103,
            214, //214,
            209,
            //214,
            213,
            215,
            227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 240, 241 };
        //private int[] seeds = {0, 91, 91, 10, 100};
        //private int[] seeds = { 0, 1, 91, 91, 10, 100, 3, 1, 4, 4, 5, 6, 7, 7, 8, 8, 8 };
        


        private int NextSeed()
        {
            var res = rnd.Next();
            _seedInd++;
            if (_seedInd < seeds.Length)
            {
                res = seeds[_seedInd];
            }


            return res;
        }

        public ServerStateBase()
        {
            //TODO check if works without
            InitializeNewSyntheticTruths();
            AddBotUserDefault();
        }

        public void DumpScoreBoardToFile(LevelDescription<TWellPoint, TRealizationData, TSecretState> scoreBoard, 
            string dirIdHighScore="scoreLog/",
            string fileName="")
        {
            if (!Directory.Exists(dirIdHighScore))
            {
                Directory.CreateDirectory(dirIdHighScore);
            }

            if (fileName == "")
            {
                fileName += DateTime.Now.Ticks;
            }
            var jsonStr = JsonConvert.SerializeObject(scoreBoard);
            System.IO.File.WriteAllText(dirIdHighScore + "/" + fileName, jsonStr);
        }

        public void DumpAllScoreBoardToFile(
            string dirIdHighScore = "scoreLog/")
        {
            ;
            if (!Directory.Exists(dirIdHighScore))
            {
                Directory.CreateDirectory(dirIdHighScore);
            }

            DumpAllScoresToFile(GetUserResultsForAllGames(), dirIdHighScore);

            for (int i=0; i<TOTAL_LEVELS; ++i)
            {
                DumpScoreBoardToFile(GetScoreboard(i), 
                    dirIdHighScore, 
                    "board_"+seeds[i]+".json");
            }

        }

        public string DumpUserResultToFileOnStop(KeyValuePair<UserResultId, UserResultFinal<TWellPoint>> resultPair)
        {
            var userId = resultPair.Key.UserName;
            var serverGameSeed = seeds[resultPair.Key.GameId];
            var hashString = string.Format("{0:X}", UserScorePairLockedGeneric< TUserModel, TUserDataModel, TSecretState, TWellPoint, TUserResult,TRealizationData>
                .CalculateHashInt(userId));
            var strMaxLen = 15;
            var userDirName = userId.Trim();
            if (userDirName.Length > strMaxLen)
            {
                userDirName = userDirName.Remove(strMaxLen);
            }
            foreach (var ch in GetInvalidFileNameChars())
            {
                userDirName = userDirName.Replace(ch, '-');
            }

            userDirName = userDirName + "_" + hashString;

            var dirPrefix = "resultLog/";
            var userDirId = DateTime.Now.Ticks + "_" + userDirName;
            var dirId = dirPrefix + userDirId;
            if (!Directory.Exists(dirId))
            {
                Directory.CreateDirectory(dirId);
            }

            var newPair = new KeyValuePair<UserResultId, UserResultFinal<TWellPoint>>(
                new UserResultId(userId, resultPair.Key.GameNumberForUser, serverGameSeed),
                resultPair.Value);

            var jsonStr = JsonConvert.SerializeObject(newPair);
            File.WriteAllText(dirId + "/" + "userResultPair.json", jsonStr);

            DumpAllScoreBoardToFile(dirId);
            return userDirId;
        }

        private static KeyValuePair<UserResultId, UserResultFinal<TWellPoint>> GetUserResultPairFromFile(string folderId)
        {
            var dirId = "resultLog/" ;
            var fileSuffix = "/userResultPair.json";

            var userResultPair = ReadFromFileSafe<KeyValuePair<UserResultId, UserResultFinal<TWellPoint>>>(dirId, folderId, fileSuffix);
            return userResultPair;
        }

        private static T ReadFromFileSafe<T>(string prefix, string unsafeName, string suffix="") {
            foreach (var ch in GetInvalidFileNameChars())
            {
                if (unsafeName.Contains(ch))
                {
                    return default;
                }
            }

            try
            {
                var fileString = File.ReadAllText(prefix+unsafeName+suffix);
                var data = JsonConvert.DeserializeObject<T>(fileString);
                return data;
            }
            catch (Exception e)
            {
                return default;
            }
        }

        public LevelDescription<TWellPoint, TRealizationData, TSecretState> GetScoreboardFromFile(string fileName)
        {
            var dirId = "scoreLog";
            

            var scoreBoard = ReadFromFileSafe<LevelDescription<TWellPoint, TRealizationData, TSecretState>>(dirId, fileName);
            return scoreBoard;
        }

        public void DumpAllScoresToFile(IList<KeyValuePair<UserResultId, UserResultFinal<TWellPoint>>> scoreBoard,
            string folderName)
        {
            var dirId = "";
            var dirIdHighScore = dirId + folderName;
            if (!Directory.Exists(dirIdHighScore))
            {
                Directory.CreateDirectory(dirIdHighScore);
            }

            var fileName = "allScores.json";

            var jsonStr = JsonConvert.SerializeObject(scoreBoard);
            System.IO.File.WriteAllText(dirIdHighScore + "/" + fileName, jsonStr);
        }

        public IList<KeyValuePair<UserResultId, UserResultFinal<TWellPoint>>> GetAllScoresFromFile(string folderName)
        {
            var dirId = "resultLog/";
            var fileId = "/allScores.json";

            var allScores = ReadFromFileSafe<List<KeyValuePair<UserResultId, UserResultFinal<TWellPoint>>>>(dirId, folderName, fileId);
            return allScores;
        }

        public IList<UserResultFinal<TWellPoint>> GetScoresForGameFromFile(string folderName, int fileInd)
        {
            var dirId = "resultLog/";
            var fileId = "/board_"+fileInd+".json";

            var allScores = ReadFromFileSafe<ScoreBoard<TWellPoint, TRealizationData, TSecretState>>(dirId, folderName, fileId);
            return allScores.UserResults;
        }

        private string lastLoadedUser = "";
        private string lastLoadedUserFile = "";

        private string _GetNextDir()
        {
            var dirId = "userLog/";
            var allDirs = Directory.GetDirectories(dirId);
            Array.Sort(allDirs);
            if (allDirs.Length == 0)
            {
                throw new InvalidOperationException("No folders to choose from");
            }
            try
            {
                var nextDir = allDirs.First(str => str.CompareTo(lastLoadedUser) > 0);
                lastLoadedUser = nextDir;
                lastLoadedUserFile = "";
                return nextDir;
            }
            catch (InvalidOperationException e)
            {
                lastLoadedUser = "";
                return _GetNextDir();
            }
        }

        private string _GetNextFile(string dirId)
        {
            var files = Directory.GetFiles(dirId);
            Array.Sort(files);
            try
            {
                var getNextFile = files.First(str => str.CompareTo(lastLoadedUserFile) > 0);
                lastLoadedUserFile = getNextFile;
                return getNextFile;
            }
            catch (InvalidOperationException e)
            {
                dirId = _GetNextDir();
                return _GetNextFile(dirId);
            }
        }


        public TUserDataModel GetNextUserStateFromFile(bool nextUser = false, string userToLoad = "")
        {
            if (userToLoad != "")
            {
                //TODO make this into a const
                lastLoadedUser = "userLog/" + userToLoad;
                lastLoadedUserFile = "";
                lastLoadedUser = _GetNextDir();
            }
            if (nextUser || lastLoadedUser == "")
            {
                lastLoadedUser = _GetNextDir();
            }

            lastLoadedUserFile = _GetNextFile(lastLoadedUser);

            var fileName = lastLoadedUserFile;

            var userState = ReadFromFileSafe<TUserDataModel>(fileName, "");
            return userState;
        }

        public void DumpSectetStateToFile(int data)
        {
            var dirId = "serverstatelog/secret";
            if (!Directory.Exists(dirId))
            {
                Directory.CreateDirectory(dirId);
            }
            var jsonStr = JsonConvert.SerializeObject(data);
            System.IO.File.WriteAllText(dirId + "/" + DateTime.Now.Ticks, jsonStr);
        }

        /// <summary>
        /// this should call dump secret stateGeocontroller to file
        /// </summary>
        /// <param name="seed"></param>
        protected abstract TSecretState[] InitializeNewSyntheticTruths();
        //{
        //    DumpSectetStateToFile(seed);
        //    //Console.WriteLine("Initialized synthetic truth with seed: " + seed);
        //    //_syntheticTruth = new TrueModelState(seed);
        //}

        private WellPointWithScore<TWellPoint> EvalueateSegmentAgainstTruth(TWellPoint p1, TWellPoint p2)
        {
            throw new NotImplementedException();
            //var pointWithScore = new WellPointWithScore<TWellPoint>()
            //{
            //    wellPoint = p2,
            //};

            //var twoPoints = new List<TWellPoint>() { p1, p2 };
            //var localScore = EvaluatorTruth(GetTruthForEvaluation(), twoPoints);
            //pointWithScore.Score = localScore;
            //return pointWithScore;
        }

        protected abstract IList<TRealizationData> GetTruthsForEvaluation();

        protected UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint,
            TUserResult, TRealizationData> GetNewDefaultUserPair(string userKey)
        {
            //TODO here we need to create synthetic truth?
            return new UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, TWellPoint,
                TUserResult, TRealizationData>(
                userKey,
                EvaluatorUser, EvaluatorTruth, GetTruthsForEvaluation());
        }

        protected abstract TWellPoint GetInitialPoint();

        //protected virtual UserResultFinal<TWellPoint> GetBestTrajectoryWithScore(TRealizationData secret,
        //    TWellPoint start,
        //    ObjectiveEvaluatorDelegateTruth<TRealizationData, TWellPoint>.ObjectiveEvaluationFunction evaluator)
        //{
        //    return null;
        //}

        public void ResetServer(int seed = -1)
        {
            //TODO implement the seed
            _users = new ConcurrentDictionary<string, 
                UserScorePairLockedGeneric<TUserModel, TUserDataModel, TSecretState, 
                    TWellPoint, TUserResult, TRealizationData>>();
        }


        private static double GetFinalScore(UserResultFinal<TWellPoint> userResult)
        {
            return userResult.TrajectoryWithScore[userResult.TrajectoryWithScore.Count - 1].Score;
        }

        public ManyWells<TWellPoint> GetScreenFull()
        {
            var wells = new ManyWells<TWellPoint>();
            var allTrajectoryKeys = _resultingTrajectories.Keys;
            foreach (var userResultId in allTrajectoryKeys)
            {
                UserResultFinal<TWellPoint> curTrajectory;
                var result = _resultingTrajectories.TryGetValue(userResultId, out curTrajectory);
                if (result)
                {
                    wells.UserResults.Add(TrajectoryOutputSingle<TWellPoint>.FromUserResult(curTrajectory));
                }
            }

            return wells;
        }

        public IList<KeyValuePair<UserResultId, UserResultFinal<TWellPoint>>> GetUserResultsForAllGames()
        {
            var allTrajectoryKeys = _resultingTrajectories.Keys;
            var currentScores = new List<KeyValuePair<UserResultId, UserResultFinal<TWellPoint>>>();
            foreach (var userResultId in allTrajectoryKeys)
            {
                UserResultFinal<TWellPoint> curTrajectory;
                var result = _resultingTrajectories.TryGetValue(userResultId, out curTrajectory);
                if (result)
                {
                    var serverGameSeed = seeds[userResultId.GameId];
                    var newPair = new KeyValuePair<UserResultId, UserResultFinal<TWellPoint>>(
                        new UserResultId(userResultId.UserName, userResultId.GameNumberForUser, serverGameSeed),
                        curTrajectory);
                    currentScores.Add(newPair);
                }
            }

            return currentScores;
        }

        private static List<KeyValuePair<UserResultId, UserResultFinal<TWellPoint>>> GetResultsForUser(IList<KeyValuePair<UserResultId, UserResultFinal<TWellPoint>>> current, string userId)
        {
            //var allTrajectoryKeys = current.Select(x => x.Key);
            var selectedPairs = current.Where(x => x.Key.UserName == userId).ToList();

            selectedPairs = selectedPairs.OrderBy(x => x.Key.GameNumberForUser).ToList();
            return selectedPairs;
        }

        public IList<UserResultFinal<TWellPoint>> GetUserResultsForGame(int serverGameIndex)
        {
            serverGameIndex %= _levelDescriptions.Length;
            var allTrajectoryKeys = _resultingTrajectories.Keys;
            var selectedKeys = allTrajectoryKeys.Where(key => key.GameId == serverGameIndex);
            var currentScores = new List<UserResultFinal<TWellPoint>>();
            foreach (var userResultId in selectedKeys)
            {
                UserResultFinal<TWellPoint> curTrajectory;
                var result = _resultingTrajectories.TryGetValue(userResultId, out curTrajectory);
                if (result)
                {
                    currentScores.Add(curTrajectory);
                }
            }

            return currentScores;
        }

        public UserResultFinal<TWellPoint> GetBotResultForGame(int serverGameIndex)
        {
            UserResultFinal<TWellPoint> botResult = null;
            var key = new UserResultId(BotUserName, serverGameIndex, serverGameIndex);
            var getResult = _resultingTrajectories.TryGetValue(key, out botResult);
            return botResult;
        }

        private IList<double> GetRating(string userName, 
            IList<KeyValuePair<UserResultId, UserResultFinal<TWellPoint>>> current, 
            IList<KeyValuePair<UserResultId, UserResultFinal<TWellPoint>>> original = null,
            string folderName = "")
        {
            List<KeyValuePair<UserResultId, UserResultFinal<TWellPoint>>> resultsForUser = null;
            var exclude = 1;
            if (original == null)
            {
                resultsForUser = GetResultsForUser(current, userName);
                original = current;
            }
            else
            {
                resultsForUser = GetResultsForUser(original, userName);
                exclude = 0;
            }
            
            

            var resCount = resultsForUser.Count;

            var oneGameRatings = new double[resCount];
            var curTotal = 0.0;
            var totalRatingUpTo = new double[resCount];
            var maxRating = new double[resCount];

            for (var index = 0; index < resCount; index++)
            {
                var keyValuePair = resultsForUser[index];
                //if (keyValuePair.Key.GameNumberForUser != index)
                //{
                //    throw new InvalidOperationException("User has not done all games");
                //}

                var serverIndex = seeds.ToList().FindIndex(x => x == keyValuePair.Key.GameId);
                

                double curRating = 0;
                //the game is active on the server
                if (serverIndex != -1 && serverIndex<TOTAL_LEVELS)
                {
                    curRating = GetPercentile100ForGame(serverIndex, GetFinalScore(keyValuePair.Value), exclude);
                }
                else
                {
                    //this requests from a board
                    curRating = LoadPercentile100ForGame(GetFinalScore(keyValuePair.Value), folderName, keyValuePair.Key.GameId);
                }

                oneGameRatings[index] = curRating;

                //update averages
                for (var j = 0; j < index; ++j)
                {
                    totalRatingUpTo[j] = totalRatingUpTo[j] - oneGameRatings[index-j-1] + curRating;
                    if (totalRatingUpTo[j] / (j+1) > maxRating[j])
                    {
                        maxRating[j] = totalRatingUpTo[j] / (j+1);
                    }
                }

                totalRatingUpTo[index] = curTotal + curRating;
                maxRating[index] = totalRatingUpTo[index] / (index + 1);
                curTotal = totalRatingUpTo[index];

            }


            return maxRating;
        }

        public double GetPercentile100ForGame(int serverGameIndex, double myScore, int exclude = 1)
        {
            serverGameIndex %= _levelDescriptions.Length;
            var results = GetUserResultsForGame(serverGameIndex);
            double lower = results.Select(GetFinalScore).Count(value => value < myScore);
            var total = results.Count - exclude;
            if (total == 0)
            {
                return 100;
            }
            return lower / total * 100.0;
        }

        public double LoadPercentile100ForGame(double myScore, string folderName, int gameId)
        {
            var results = GetScoresForGameFromFile(folderName, gameId);
            double lower = results.Select(GetFinalScore).Count(value => value < myScore);
            var total = results.Count - 1;
            if (total == 0)
            {
                return 100;
            }
            return lower / total * 100.0;
        }



        public LevelDescription<TWellPoint, TRealizationData, TSecretState> GetScoreboard(int serverGameIndex)
        {
            serverGameIndex %= _levelDescriptions.Length;
            var results = GetUserResultsForGame(serverGameIndex);
            var level = _levelDescriptions[serverGameIndex];
            level.UserResults = results;
            level.BotResult = GetBotResultForGame(serverGameIndex);
            return level;
        }

        public double GetScorePercentForGame(int serverGameIndex, double myScore)
        {
            serverGameIndex %= _levelDescriptions.Length;
            var best = GetFinalScore(_levelDescriptions[serverGameIndex].BestPossible);
            return myScore / best * 100.0;
        }

        public MyScore GetMyFullScore(int serverGameIndex, double valueScore, string userName, bool getRating = false)
        {
            var score = new MyScore()
            {
                ScorePercent = GetScorePercentForGame(serverGameIndex, valueScore),
                ScoreValue = valueScore,
                YouDidBetterThan = GetPercentile100ForGame(serverGameIndex, valueScore),
                UserName = userName,
            };
            if (getRating)
            {
                score.Rating = GetRating(userName, GetResultsForUser(GetUserResultsForAllGames(), userName));
            }
            return score;
        }

        public string GetFriendUserNameFromFile(string folderId)
        {
            var pair = GetUserResultPairFromFile(folderId);
            if (pair.Value == null)
            {
                return null;
            }

            return pair.Key.UserName;
        }

        private MyScore GetUserResultFromFile(string folderId)
        {
            var pair = GetUserResultPairFromFile(folderId);
            if (pair.Value == null)
            {
                return null;
            }
            var serverGameIndex = seeds.ToList().FindIndex(x => x==pair.Key.GameId);
            if (serverGameIndex != -1 && serverGameIndex < TOTAL_LEVELS)
            {
                var valueScore = GetFinalScore(pair.Value);
                var score = new MyScore()
                {
                    ScorePercent = GetScorePercentForGame(serverGameIndex, valueScore),
                    ScoreValue = valueScore,
                    YouDidBetterThan = GetPercentile100ForGame(serverGameIndex, valueScore),
                    UserName = pair.Key.UserName,
                };
                return score;
            }
            else
            {
                var score = new MyScore()
                {
                    UserName = pair.Key.UserName,
                };
                return score;
            }
        }

        private void CleanNewResultingTrajectories(int n = 60)
        {
            if (_newResultingTrajectories.Count > n)
            {
                var pairs = _newResultingTrajectories.ToList();
                pairs.Sort((pair1,pair2) => (int) (pair1.Value.TimeTicks - pair2.Value.TimeTicks));
                for (var i = 0; i < pairs.Count - n; ++i)
                {
                    _newResultingTrajectories.TryRemove(pairs[i].Key, out _);
                }
            }
        }

        protected void PushToResultingTrajectories(KeyValuePair<UserResultId, UserResultFinal<TWellPoint>> pair)
        {
            _resultingTrajectories.AddOrUpdate(pair.Key, pair.Value,
                (key, value) => pair.Value);
            _newResultingTrajectories.AddOrUpdate(pair.Key, pair.Value,
                (key, value) => pair.Value);

        }





        public virtual TUserDataModel LossyCompress(TUserDataModel data)
        {
            return data;
        }

        public abstract void AddBotUserDefault();


        public bool UserExists(string userId)
        {
            return _users.ContainsKey(userId);
        }

        public TUserDataModel GetUserDataDefault()
        {
            return _dummyUserData;
        }

        public TUserDataModel GetUserData(string userId)
        {
            //GetNewDefaultUserPair does not lock
            //seems to be locked only if update is locked
            var userData = _users.GetOrAdd(userId, GetNewDefaultUserPair)
                .UserDataLocked;
            return userData;
        }

        public TUserResult GetUserEvaluationData(string userId, IList<TWellPoint> trajectory)
        { 
            //this method does NOT lock
            var userPair = _users.GetOrAdd(userId, GetNewDefaultUserPair);
            var resultDistribution = userPair.GetEvalautionLocked(trajectory);
            var scorePair = userPair.GetUserResultScorePairLocked(_levelDescriptions.Length);
            PushToResultingTrajectories(scorePair);
            return resultDistribution;
        }

        public MyScore StopUser(string userId, string friendSaveId = null)
        {
            //this method does NOT lock
            var userPair = _users.GetOrAdd(userId, GetNewDefaultUserPair);
            var updatedUser = userPair.StopUserLocked(EvaluatorTruth, GetTruthsForEvaluation());
            KeyValuePair<UserResultId, UserResultFinal<TWellPoint>> pair;
            pair = userPair.GetUserResultScorePairLocked(_levelDescriptions.Length);
            PushToResultingTrajectories(pair);
            var shareId = DumpUserResultToFileOnStop(pair);
            MyScore friendsScore = null;
            if (friendSaveId != null)
            {
                friendsScore = GetUserResultFromFile(friendSaveId);
                if (friendsScore != null)
                {
                    friendsScore.Rating = GetRating(friendsScore.UserName, GetUserResultsForAllGames(),
                        GetAllScoresFromFile(friendSaveId), friendSaveId);
                }
            }
        
            if (friendsScore == null)
            {
                friendsScore = GetMyFullScore(pair.Key.GameId, GetFinalScore(pair.Value), BotUserName, true);
            }

            MyScore myScore;
            myScore = GetMyFullScore(pair.Key.GameId, GetFinalScore(pair.Value), userId, true);
            myScore.SharingId = shareId;
            myScore.FriendsScore = friendsScore;
            return myScore;
        }

        public int MoveUserToNewGame(string userId)
        {
            //this method does not lock
            var gameInd = _users.GetOrAdd(userId, GetNewDefaultUserPair)
                .MoveUserToNewGameLocked(EvaluatorTruth, GetTruthsForEvaluation());
            return gameInd;
        }

        public TUserDataModel UpdateUser(string userId, TWellPoint load = default)
        {
            //this method locks
            var userPair = _users.GetOrAdd(userId, GetNewDefaultUserPair);
            var updatedUser = userPair.UpdateUserLocked(load, _secrets, EvaluatorTruth, GetTruthsForEvaluation());
            KeyValuePair<UserResultId, UserResultFinal<TWellPoint>> pair;
            pair = userPair.GetUserResultScorePairLocked(_levelDescriptions.Length);
            PushToResultingTrajectories(pair);
            return updatedUser;
        }

    }
}
