using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace EnKFLib
{
    public interface IDataProvider<Model>
    {
        int SingleDataSize
        {
            get;
        }

        int DataPointsCount
        {
            get;
        }


        //TODO add option to process only desired indeces!!!!
        Vector GenerateData(Model m);
        IList<Vector> GenerateDataList(Model m);
        Vector ErrorVarience { get; }
        IList<Vector> ErrorVarienceList { get; }
        Vector DataValue { get; }
        IList<Vector> DataValueList { get; }
        
    }
}
