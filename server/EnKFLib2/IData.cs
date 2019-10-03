using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vectorizable;

namespace EnKFLib
{
    public interface IData<ParameterType, ValueType>
        where ValueType: IVectorizable
    {
        //TODO consider adding time
        ParameterType Parameter { get;}
        ValueType Value { get;}
        //TODO consider having option of different type e.g. ValueType
        ValueType ErrorVarience { get;}
    }
}
