using System;
using System.Collections.Generic;
using System.Linq;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public class UserStateMockBase<TSecret> : IUserImplementaion<UserData, WellPoint, TSecret>
    {
        private readonly UserData _userData;

        //TODO implemnt adding points

        private UserData CreateUserData()
        {
            Random r = new Random();
            var userData = new UserData()
            {
                Width = 100.0,
                Height = 10.0,
                wellPoints = new List<WellPoint>() { GetNextStateDefault()},
                Xtopleft = 10.0,
                Ytopleft = 10.0,

            };
            var xs = new List<double>();

            for (double x = userData.Xtopleft; x <= userData.Xtopleft + userData.Width; x += userData.Width/10.0)
            {
                xs.Add(x);
            }

            userData.xList = xs;

            List<RealizationData> rs = Enumerable.Range(0, 100).Select(i =>
            {
                var realization = new RealizationData();

                var y1 = new List<double>();
                var y2 = new List<double>();

                for (double x = userData.Xtopleft; x <= userData.Xtopleft + userData.Width; x += userData.Width/10.0)
                {
                    y1.Add(userData.Ytopleft + userData.Height * 0.3 - (r.NextDouble()*userData.Height/2.0) / 10);
                    y2.Add(userData.Ytopleft + userData.Height * 0.3 + (r.NextDouble()*userData.Height/2.0) / 10);
                }

                realization.YLists.Add(y1);
                realization.YLists.Add(y2);

                //realization.polygons.Add(points);
                return realization;
            }).ToList();
            userData.realizations = rs;
            return userData;
        }

        public UserStateMockBase()
        {
            _userData = CreateUserData();
        }

        public UserData UserData
        {
            get => _userData;
        } 

        public bool UpdateUser(WellPoint updatePoint, TSecret secret)
        {
            return true;
        }

        public WellPoint GetNextStateDefault()
        {
            //TODO consider a better implementation, but this is not needed functionality once client is good
            var point = new WellPoint()
            {
                X = 0.0,
                Y = 0.0,
                Angle = 1.0,
            };
            return point;
        }
    }
}