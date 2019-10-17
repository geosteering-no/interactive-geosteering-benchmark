using System.Collections.Generic;

namespace ServerDataStructures
{
    public class PopulationScoreData
    {
        public double Xtopleft { get; set; }
        public double Ytopleft { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public double Xdist { get; set; }

        public IList<double> xList { get; set; }

        public RealizationData secretRealization { get; set; }
        public IList<UserResultFinal> UserResults { get; set; }
    }
}
