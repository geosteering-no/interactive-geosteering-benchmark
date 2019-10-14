using System.Collections.Generic;

namespace ServerDataStructures
{
    public class PopulationScoreData
    {
        public RealizationData SecretRealization { get; set; }
        public IList<UserResultFinal> UserResults { get; set; }
    }
}
