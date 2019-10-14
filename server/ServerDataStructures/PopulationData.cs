using System.Collections.Generic;

namespace ServerDataStructures
{
    public class PopulationData
    {
        public RealizationData SecretRealization { get; set; }
        public IList<UserResult> UserResults { get; set; }
    }
}
