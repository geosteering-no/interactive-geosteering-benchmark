﻿namespace ServerStateInterfaces
{
    public interface IFullServerState<in TWellPoint>
    {
        bool AddUser(string userId);
        void RestartServer(int seed = 0);
        bool UpdateUser(string userId, TWellPoint load = default);
        bool UserExists(string userId);
    }
}