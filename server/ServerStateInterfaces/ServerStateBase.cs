using System;
using System.Collections.Generic;
using System.Text;

namespace ServerStateInterfaces
{
    class ServerStateBase
    {
        public class FullServerStateBase<TWellPoint, TUserDataModel>
        {
            bool AddUser(string userId)
            {
                return true;
            }

            void RestartServer(int seed = 0)
            {

            }

            bool UpdateUser(string userId, TWellPoint load = default)
            {
                return true;
            }

            bool UserExists(string userId)
            {
                return true;
            }


        }

    }
}
