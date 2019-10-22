using System.Collections.Generic;

namespace ServerStateInterfaces
{
    public interface IFullServerStateGeocontroller<
        TWellPoint, 
        TUserData, 
        TUserEvaluationData, 
        TScoreData>
    {

        void RestartServer(int seed = -1);
        void StopAllUsers();
        TUserData UpdateUser(string userId, TWellPoint load = default);
        TUserData StopUser(string userId);
        TUserData GetOrAddUserState(string userId);
        TUserEvaluationData GetUserEvaluationData(string userId, IList<TWellPoint> trajectory);
        TScoreData GetScoreboard();

        bool UserExists(string str);

    }
}