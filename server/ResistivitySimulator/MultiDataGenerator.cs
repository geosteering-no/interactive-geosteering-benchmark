using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using TrajectoryInterfaces;
using Vectorizable;

namespace ResistivitySimulator
{
    public class MultiDataGenerator
    {
        //private MultiResistivityModel _model;
        private IResistivityModel eModel;

        private double instrumentSize;
        //private Vector trueRealization;
        public double dataError = 0.5;

        public Vector GetDataVariance(int posCount=1)
        {
            return (Vector)VectorGenerator.One(GetDataSize(posCount)).Multiply(dataError);
        }

        //public Vector GetDataVariance(double dataVarienceValue)
        //{
        //    return (Vector)VectorGenerator.One(GetDataSize).Multiply(dataVarienceValue);
        //}


        //public Vector GetDataVarianceRelative(double factor)
        //{
        //    var error = dataError * factor;
        //    return (Vector)VectorGenerator.One(GetDataSize).Multiply(error);
        //}

        //public Matrix GetDataCovariance()
        //{
        //    return _model.GetDataCovarience(dataError);
        //}

        public MultiDataGenerator(IResistivityModel eModel, double instrumentSize)
        {
            //this.trueRealization = trueRealization;
            //_model = new MultiResistivityModel(eModel, instrumentSize);
            this.eModel = eModel;
            this.instrumentSize = instrumentSize;
        }

        //public IList<IContinousState> Position
        //{
        //    get 
        //    { 
        //        return _model.Position; 
        //    }
        //    set 
        //    { 
        //        _model.Position = value; 
        //    }
        //}

        public int GetDataSize(int posCount)
        {
            return MultiResistivityModel.GetDataSize(posCount);
        }

        /// <summary>
        /// Gets the list of points used in the last update to show depth of invetigation
        /// </summary>
        //public HashSet<PointF> LastEvaluatedPoints => _model.LastEvaluatedPoints;

        public Vector GetData(IContinousState position)
        {
            var model = new MultiResistivityModel(eModel, instrumentSize);
            var positions = new List<IContinousState>(1);
            positions.Add(position);
            return model.ModelToData(positions);
        }


        //public Vector GetData(IList<IContinousState> position)
        //{
        //    model.Position = position;
        //    return _model.ModelToData();
        //}


        ///// <summary>
        ///// used for visualization
        ///// </summary>
        ///// <param name="x"></param>
        ///// <param name="y"></param>
        ///// <returns></returns>
        //public double GetResistivity(double x, double y)
        //{
        //    return _model.GetModelResistivity(x, y);
        //}

    }
}
