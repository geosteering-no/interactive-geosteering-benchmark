using System;
using System.Collections.Generic;
using System.Text;

namespace ServerDataStructures
{
    public class UserRating
    {
        public long TimeTicks { get; set; }
        public string UserName { get; set; }
        public IList<double> Rating { get; set; }
    }
}
