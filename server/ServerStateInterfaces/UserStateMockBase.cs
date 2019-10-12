﻿namespace ServerStateInterfaces
{
    public class UserStateMockBase<T> : IUserImplementaion<UserData, WellPoint, T>
    {
        private readonly UserData _userData;

        public UserData UserData
        {
            get => _userData;
        } 

        public bool UpdateUser(WellPoint updatePoint, T secret)
        {
            return true;
        }

        public WellPoint GetNextStateDefault()
        {
            //TODO consider a better implementation, but this is not needed functionality once client is good
            var point = new WellPoint()
            {
                X = 50,
                Y = 70,
                Angle = 0,
            };
            return point;
        }
    }
}