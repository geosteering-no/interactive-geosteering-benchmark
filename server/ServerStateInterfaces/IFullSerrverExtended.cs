using System;
using System.Collections.Generic;
using System.Text;

namespace ServerStateInterfaces
{
    public interface IFullSerrverExtended<
        TWellPoint,
        TUserData,
        TUserEvaluationData,
        TScoreData> : IFullServerStateGeocontroller<
        TWellPoint,
        TUserData,
        TUserEvaluationData,
        TScoreData>
    {
        bool UserExists(string str);
        bool AddUser(string str);
    }
}
