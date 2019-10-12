using System;
using System.Collections.Generic;
using System.Text;

namespace ServerStateInterfaces
{
    public interface IUserImplementaion<TUserData, TWellPoint, TSecretState>
    {
        TUserData UserData { get; }
        bool UpdateUser(TWellPoint updatePoint, TSecretState secret);
        TWellPoint GetNextStateDefault();
    }
}
