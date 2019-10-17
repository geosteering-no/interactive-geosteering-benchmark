using System.Collections.Generic;

namespace ServerStateInterfaces
{
    public interface IFullServerState<
        TWellPoint, 
        TUserData, 
        TUserEvaluationData, 
        TScoreData>
    {
        bool AddUser(string userId);
        void RestartServer(int seed = -1);
        TUserData UpdateUser(string userId, TWellPoint load = default);
        TUserData StopUser(string userId);
        bool UserExists(string userId);
        TUserData GetOrAddUserState(string userId);
        TUserEvaluationData GetUserEvaluationData(string userId, IList<TWellPoint> trajectory);
        TScoreData GetScoreboard();

    }
}