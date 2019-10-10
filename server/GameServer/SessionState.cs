using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;


namespace GameServer
{
    public class SessionState
    {
        public SessionState()
        {
            realizations = new List<List<Realization>>();
            angles = new List<Double>();
        }
        public List<List<Realization>> realizations { get; }
        public List<Double> angles { get; }

        public string toJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static SessionState fromJson(string json)
        {
            return JsonConvert.DeserializeObject<SessionState>(json);
        }

    }
}
