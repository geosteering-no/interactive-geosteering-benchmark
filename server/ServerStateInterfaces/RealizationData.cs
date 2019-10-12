using System.Collections.Generic;

namespace ServerStateInterfaces
{
    public class RealizationData {
        private IList<IList<double>> _yLists = new List<IList<double>>();

        public IList<IList<double>> YLists
        {
            get => _yLists;
            set => _yLists = value;
        }
    }
}