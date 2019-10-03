using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ResistivitySimulator
{
    public interface IValueModel<T> where T : IEarthModelRealization
    {
        
        double ComputeReservoirValue(T model, double x, double y);
    }
}
