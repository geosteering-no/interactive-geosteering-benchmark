﻿using System.Collections.Generic;

namespace ServerDataStructures
{
    public class PopulationScoreData<TWellPoint, TRealizationData>
    {
        public double Xtopleft { get; set; }
        public double Ytopleft { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public int TotalDecisionPoints { get; set; }

        public double Xdist { get; set; }

        public IList<double> xList { get; set; }

        public IList<TRealizationData> secretRealizations { get; set; }
        public IList<UserResultFinal<TWellPoint>> UserResults { get; set; }

        public UserResultFinal<TWellPoint> BestPossible { get; set; }
        public UserResultFinal<TWellPoint> BotResult { get; set; }

    }
}
