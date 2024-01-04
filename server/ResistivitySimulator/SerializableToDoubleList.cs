using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ResistivitySimulator
{
    interface SerializableToDoubleList
    {
        IList<double> DoubleList
        {
            get;
            set;
        }
    }
}
