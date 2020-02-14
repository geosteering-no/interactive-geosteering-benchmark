using System.Collections.Generic;

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
        TUserData StopUser(string userId);

        TUserData LossyCompress(TUserData data);
        TUserData GetUserData(string userId);
        TUserEvaluationData GetUserEvaluationData(string userId, IList<TWellPoint> trajectory);
        TScoreData GetScoreboard();

        void AddBotUserDefault();

        bool UserExists(string str);
        TScoreData GetScoreboardFromFile(string fileName);
    }
}