using System.Collections.Generic;

namespace ResistivitySimulator
{
    public interface IEarthModelRealization : IResistivityModel
    {
        /// <summary>
        /// For optimization
        /// </summary>
        //IValueModel ValueModel { get; set; }
        IList<double> GetXs();
        IList<IList<double>> GetBoundaryLists();
    }
}