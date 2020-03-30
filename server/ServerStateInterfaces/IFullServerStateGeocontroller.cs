using System.Collections.Generic;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public interface IFullServerStateGeocontroller<
        TWellPoint, 
        TUserData, 
        TUserEvaluationData, 
        TScoreData>
    {

        //void RestartServer(int seed = -1);
        void ResetServer(int seed = -1);
        //TODO implement the seed
        TUserData UpdateUser(string userId, TWellPoint load = default);
        MyScore StopUser(string userId, string friendId = null);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>sequential game index</returns>
        int MoveUserToNewGame(string userId);

        TUserData LossyCompress(TUserData data);
        TUserData GetUserData(string userId);
        TUserEvaluationData GetUserEvaluationData(string userId, IList<TWellPoint> trajectory);
        TScoreData GetScoreboard(int serverGameIndex);

        void AddBotUserDefault();

        string LoadFriendUserNameFromFile(string folderId);

        Dictionary<string, UserRating> GetAllRatings();

        bool UserExists(string str);
        TScoreData LoadScoreboardFromFile(string fileName);
        TUserData LoadNextUserStateFromFile(bool nextUser=false, string userToLoad="");
        ManyWells<TWellPoint> GetScreenFull();
        TUserData GetUserDataDefault();

    }
}