using System;
using System.Collections.Generic;
using System.Text;

namespace ServerDataStructures
{
    public class UserEvaluation
    {
        /// <summary>
        /// Scores for original indexes
        /// </summary>
        public IList<double> RealizationScores { get; set;}
        public IList<int> SortedIndexes { get; set; }
    }
}
