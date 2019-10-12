using System;
using System.Collections.Generic;

namespace ServerStateInterfaces {
    public class UserData  {

        public double Xtopleft { get; set; }
        public double Ytopleft { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public double Xdist {get; set;}

        public IList<double> xList { get; set; }

        public IList<RealizationData> realizations  { get; set; }

        public IList<WellPoint> wellPoints  { get; set; }
    }
}
