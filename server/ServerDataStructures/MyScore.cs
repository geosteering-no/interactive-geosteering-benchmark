using System;
using System.Collections.Generic;
using System.Text;

namespace ServerDataStructures
{
    public class MyScore
    {
        public string UserName { get; set; }
        public double ScoreValue { get; set; }
        public double ScorePercent { get; set; }
        public double YouDidBetterThan { get; set; }
        public IList<double> Rating { get; set; }
        public MyScore FriendsScore { get; set; }
    }
}
