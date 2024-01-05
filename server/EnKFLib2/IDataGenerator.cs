using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vectorizable;

namespace EnKFLib
{
    public interface IDataGenerator<ModelType, ParameterType, ValueType>
        where ModelType: IVectorizable 
        where ValueType: IVectorizable
    {
        /// <summary>
        /// Should genearte data vector of size data_size*points.Count based on the model
        /// </summary>
        /// <param name="m"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        IList<ValueType> GenerateData(ModelType m, IList<IData<ParameterType,ValueType>> points);
    }
}
