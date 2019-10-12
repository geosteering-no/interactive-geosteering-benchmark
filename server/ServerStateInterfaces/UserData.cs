using System;
using System.Collections.Generic;

namespace ServerStateInterfaces {
    public class UserData {

        double Xtopleft { get; set; }
        double Ytopleft { get; set; }
        double Width { get; set; }
        double Height { get; set; }


        IList<double> xList { get; set; }

        IList<RealizationData> realizations  { get; set; }
    }

    public class RealizationData {
        IList<IList<double>> YLists { get; set; }
    }
}
