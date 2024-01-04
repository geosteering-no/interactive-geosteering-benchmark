using System;
using System.Collections.Generic;

namespace ServerDataStructures
{
    public class LevelDescription<TWellPoint, TRealizationData, TSecretState>
    {
        public double Xtopleft { get; set; }
        public double Ytopleft { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public int TotalDecisionPoints { get; set; }

        public double Xdist { get; set; }

        public IList<double> xList { get; set; }

        public TRealizationData secretRealization { get; set; }

        //public TSecretState secretState { get; set; }
        //UserResults were here

        /// <summary>
        /// TODO check if safe
        /// </summary>
        public IList<UserResultFinal<TWellPoint>> UserResults { get; set; }


        public UserResultFinal<TWellPoint> BestPossible { get; set; }
        public UserResultFinal<TWellPoint> BotResult { get; set; }

    }
}
