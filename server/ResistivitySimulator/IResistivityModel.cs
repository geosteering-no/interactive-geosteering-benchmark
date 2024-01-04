using System;
using System.Drawing;
using Vectorizable;

namespace ResistivitySimulator
{
    /// <summary>
    /// This one is the resestivty model
    /// </summary>
    public interface IResistivityModel : IVectorizable
    {
        double GetResistivity(Point2DDouble position);
        double GetResistivity(double x, double y);

    }
}
