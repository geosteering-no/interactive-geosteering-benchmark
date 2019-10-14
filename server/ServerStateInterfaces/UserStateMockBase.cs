using System;
using System.Collections.Generic;
using System.Linq;
using ServerDataStructures;

namespace ServerStateInterfaces
{
    public class UserStateMockBase<TSecret> : IUserImplementaion<UserData, WellPoint, TSecret>
    {
        private readonly UserData _userData;
        const int DISCRETIZATION_POINTS = 10;
        const double X_TOP_LEFT = 10.0;
        const double Y_TOP_LEFT = 10.0;
        private const double X_WIDTH = 100;


        //TODO implemnt adding points

        private UserData CreateUserData()
        {
            Random r = new Random();
            var userData = new UserData()
            {
                Width = X_WIDTH,
                Xdist = X_WIDTH / 10.0,
                Height = 10.0,
                wellPoints = new List<WellPoint>() { GetNextStateDefault()},
                Xtopleft = X_TOP_LEFT,
                Ytopleft = Y_TOP_LEFT,
            };
            var xs = new List<double>();

            for (double x = userData.Xtopleft; x <= userData.Xtopleft + userData.Width; x += userData.Width/DISCRETIZATION_POINTS)
            {
                xs.Add(x);
            }

            userData.xList = xs;

            List<RealizationData> rs = Enumerable.Range(0, 100).Select(i =>
            {
                var realization = new RealizationData();

                var y1 = new List<double>();
                var y2 = new List<double>();

                for (double x = userData.Xtopleft; x <= userData.Xtopleft + userData.Width; x += userData.Width/DISCRETIZATION_POINTS)
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
                X = X_TOP_LEFT,
                Y = Y_TOP_LEFT,
                Angle = 1.0,
            };
            return point;
        }
    }
}