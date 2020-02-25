using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using ServerDataStructures;
using ServerStateInterfaces;
using TrajectoryInterfaces;
using UserState;
using UserData = ServerDataStructures.UserData;

namespace serverConsoleApp
{
    class ConsoleServer
    {
        private const string Value = "Command not recognised";
        static IFullServerStateGeocontroller<WellPoint, UserData, UserEvaluation, LevelDescription<WellPoint, RealizationData, TrueModelState>> _serverStateGeocontroller = new FullServerState();

        static void Main(string[] args)
        {
            Console.WriteLine("exit to exit");
            while (true)
            {
                var str = Console.ReadLine();
                var watch = System.Diagnostics.Stopwatch.StartNew();
                ProcessCommand(str);
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("Request finished in " + elapsedMs + " ms");
            }
        }



        static void ProcessCommand(string str)
        {
            if (str.Trim() == "")
            {
                return;
            }

            string[] ss = str.Split(" ");
            str = ss[0];
            var command = "";
            IContinousState data = default;
            if (ss.Length > 1)
            {
                command = ss[1];
            }

            if (ss.Length > 2)
            {
                //TODO parse JSON
                //data = ss[2];
            }

            if (str == "exit")
            {
                throw new Exception("User interrupt");
                return;
            }

            if (str == "restart")
            {
                Random rnd = new Random();
                int seed = rnd.Next();
                int.TryParse(command, out seed);
                //_serverStateGeocontroller.RestartServer();
                throw new NotImplementedException();
                return;
            }



            if (_serverStateGeocontroller.UserExists(str))
            {
                Console.WriteLine("We have a user: " + str);
                //var curUser = _users[str];
                if (command == "update")
                {
                    var result = _serverStateGeocontroller.UpdateUser(str);
                    //var result = curUser.OfferUpdatePoint(updatePoint, _syntheticTruth.GetData);
                    Console.WriteLine("Update successful: " + result);
                }
                else
                {
                    Console.WriteLine(Value);
                }

            }
            else
            {
                //TODO change add user
                var res = _serverStateGeocontroller.GetUserData(str);
                if (res != null)
                {
                    Console.WriteLine("Added new user: " + str);
                }
                else
                {
                    Console.WriteLine("Could not add user");
                }
            }
        }
    }
}
