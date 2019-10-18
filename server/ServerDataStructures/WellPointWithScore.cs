using System;
using System.Collections.Generic;
using System.Text;

namespace ServerDataStructures
{
    public class WellPointWithScore<TWellPoint>
    {
        public TWellPoint wellPoint { get; set; }
        public double Score { get; set; }
    }
}
