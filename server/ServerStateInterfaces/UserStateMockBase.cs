using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerStateInterfaces
{
    public class UserStateMockBase<TSecret> : IUserImplementaion<UserData, WellPoint, TSecret>
    {
        private readonly UserData _userData;

        private UserData CreateUserData()
        {
            Random r = new Random();
            var userData = new UserData()
            {
                Width = 1.0,
                Height = 1.0,
                wellPoints = new List<WellPoint>() { GetNextStateDefault()},
                Xtopleft = 0.0,
                Ytopleft = 0.0,

            };
            var xs = new List<double>();
            for (double x = 0.0; x <= 1.0; x += 0.1)
            {
                xs.Add(x);
            }

            userData.xList = xs;

            List<RealizationData> rs = Enumerable.Range(0, 100).Select(i =>
            {
                var realization = new RealizationData();
                var startY = 0.2 + r.NextDouble();
                var startX = 0.0;
                var points = new List<double>();

                realization.YLists.Add(points);
                points = new List<double>();
                for (double x = 1.0; x >= 0.0; x -= 0.1)
                {
                    points.Add(0.6 + (r.NextDouble() - 0.5) / 10);
                }
                realization.YLists.Add(points);


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