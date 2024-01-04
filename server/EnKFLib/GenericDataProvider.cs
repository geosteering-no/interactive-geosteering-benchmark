using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vectorizable;
using MathNet.Numerics.LinearAlgebra.Double;

namespace EnKFLib
{
    public class GenericDataProvider<ModelType, ParameterType, ValueType> : IDataProvider<ModelType>
        where ValueType: IVectorizable
        where ModelType: IVectorizable 
    {


        private IList<IData<ParameterType, ValueType>> dataList = new List<IData<ParameterType, ValueType>>();


        //TODO implement adding data to reduce the surtainty of older to be consistent

        public IList<IData<ParameterType, ValueType>> DataList
        {
            get { return dataList; }
            set { dataList = value; }
        }

        private IDataGenerator<ModelType, ParameterType, ValueType> syntheticDataGenerator;

        public GenericDataProvider(IDataGenerator<ModelType, ParameterType, ValueType> gen)
        {
            this.syntheticDataGenerator = gen;
        }

        public void AddData(IData<ParameterType, ValueType> dataPoint)
        {
            dataList.Add(dataPoint);
        }

        public Vector GenerateData(ModelType m)
        {
            IList<ValueType> values = syntheticDataGenerator.GenerateData(m, dataList);
            //Vector res = new DenseVector(dataList.Count * values[0].VectorSize);
            //int i = 0;
            //foreach (ValueType vectorizable in values)
            //{
                
            //    foreach (VectorEntry entry in vectorizable.Vector)
            //    {
            //        res[i] = entry.get();
            //        ++i;
            //    }
            //}
            var res = DataUtils.CollectIntoVector(values);
            return res;
        }
        

        public IList<Vector> GenerateDataList(ModelType m)
        {
            IList<ValueType> values = syntheticDataGenerator.GenerateData(m, dataList);
            IList<Vector> res = new List<Vector>(values.Count);
            foreach (ValueType val in values)
            {
                res.Add(val.Vector);
            }
            return res;
        }
        
        ///////

        //private 

        //TODO consider storing precomputed value
        public Vector ErrorVarience
        {
            get 
            {
                //TODO do the check zero index existance
                var variences = ErrorVarienceList;
                //Vector vec = new DenseVector(dataList.Count * dataList[0].ErrorVarience.VectorSize);
                //int i = 0;
                //foreach (Vector val in ErrorVarienceList)
                //{
                //    foreach (VectorEntry entry in val)
                //    {
                //        vec[i] = entry.get();
                //        i++;
                //    }
                //}

                var vec = DataUtils.CollectIntoVector(variences);
                return vec;
            }
        }

        public IList<Vector> ErrorVarienceList
        {
            get 
            {
                IList<Vector> res = new List<Vector>(dataList.Count);
                foreach (IData<ParameterType, ValueType> data in dataList)
                {
                    res.Add(data.ErrorVarience.Vector);
                }
                return res;
            }
        }

        public int DataPointsCount
        {
            get { return dataList.Count; }
        }

        public int SingleDataSize
        {
            get {
                if (dataList.Count == 0)
                {
                    return 0;
                }
                return dataList[0].Value.VectorSize ; 
            }
        }


        public Vector DataValue
        {
            get
            {
                var values = DataValueList;
                //Vector vec = new DenseVector(dataList.Count * dataList[0].Value.VectorSize);
                //int i = 0;
                //foreach (Vector val in DataValueList)
                //{
                //    foreach (VectorEntry entry in val)
                //    {
                //        vec[i] = entry.get();
                //        i++;
                //    }
                //}
                var vec = DataUtils.CollectIntoVector(values);
                return vec;
            }
        }

        public IList<Vector> DataValueList
        {
            get {
                IList<Vector> res = new List<Vector>(dataList.Count);
                foreach (IData<ParameterType, ValueType> data in dataList)
                {
                    res.Add(data.Value.Vector);
                }
                return res;
            }
        }
    }
}
