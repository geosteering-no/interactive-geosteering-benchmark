using System.Collections.Generic;

namespace ServerDataStructures
{
    public class RealizationData {
        private IList<IList<double>> _yLists = new List<IList<double>>();

        public IList<double> XList { get; set; }

        public IList<IList<double>> YLists
        {
            get => _yLists;
            set => _yLists = value;
        }
    }
}