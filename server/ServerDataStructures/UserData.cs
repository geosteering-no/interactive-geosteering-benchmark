using System.Collections.Generic;

namespace ServerDataStructures {
    public class UserData  {

        public double Xtopleft { get; set; }
        public double Ytopleft { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public double Xdist {get; set;}

        public IList<double> xList { get; set; }

        public IList<RealizationData> realizations  { get; set; }

        public IList<WellPoint> wellPoints  { get; set; }

        public bool stopped { get; set; }
        public double DoiX { get; set; } = 2.25; //TODO 5.25 is the correct size
        public double DoiY { get; set; } = 2.6; //TODO 9.6 is a correct size
    }
}
