using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using ServerStateInterfaces;
using TrajectoryInterfaces;
using UserState;
using UserData = ServerStateInterfaces.UserData;

namespace serverConsoleApp
{
    class ConsoleServer
    {
        private const string Value = "Command not recognised";
        static IFullServerState<IContinousState, UserData> serverState = new FullServerState();

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
                serverState.RestartServer();
                return;
            }



            if (serverState.UserExists(str))
            {
                Console.WriteLine("We have a user: " + str);
                //var curUser = _users[str];
                if (command == "update")
                {
                    var result = serverState.UpdateUser(str);
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
                var res = serverState.AddUser(str);
                if (res)
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
