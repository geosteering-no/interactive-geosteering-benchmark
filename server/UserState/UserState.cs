﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using EnKFLib;
using ResistivitySimulator;
using TrajectoryInterfaces;
using TrajectoryOptimization;


namespace UserState
{
    [DataContract]
    public class UserState
    {
        [DataMember]
        private EarthModelManipulator _earthManipulator;
        private EnKF2<IResistivityModel> _enkf;

        private NewEMSimulator _emSim;
        private GenericDataProvider<IResistivityModel, IContinousState, ResistivityData2DFull>
            _dataProvider;

        private IList<IContinousState> _trajectory;

        private const int NumRealiztions = 100;
        private const int NumPointsInLayer = 40;
        private const double Offset = -15.0;
        private const double MinX = 0;
        private const double MaxX = 350;
        private const double EPS = 1e-7;

        private double DefaultDecisionStep { get; }

        private IContinousState GetDefaultFirstState()
        {
            var state = new ContinousState()
            {
                X = MinX,
                Y = 0,
                Alpha = (80.0 - 90.0) / 180.0 * Math.PI
            };
            return state;
        }

        private double GetDefaultDecisionStep()
        {
            var decisionLenX =
                29.0 * Math.Cos(GetDefaultFirstState().Alpha) * 1.0;
            return decisionLenX;
        }

        /// <summary>
        /// Creates a default user state
        /// </summary>
        public UserState()
        {
            _earthManipulator = InitializeManipulator();
            _trajectory = new List<IContinousState> {GetDefaultFirstState()};
            DefaultDecisionStep = GetDefaultDecisionStep();
            _InitializeEnkf(_earthManipulator);
        }


        private bool PointOnNextStep(IContinousState newState)
        {
            if (Math.Abs(newState.X - GetDefaultDecisionStep()) < EPS)
            {
                return true;
            }

            return false;
        }

        public IContinousState GetNextStateDefault()
        {
            var dX = DefaultDecisionStep;
            var angle = _trajectory[_trajectory.Count - 1].Alpha;
            var dY = dX * Math.Tan(angle);
            var curY = _trajectory[_trajectory.Count - 1].Y;
            var state = new ContinousState()
            {
                X = GetNextDecisionX(),
                Y = curY + dY,
                Alpha = angle,
            };
            return state;
        }

        public double GetNextDecisionX()
        {
            return _trajectory[_trajectory.Count - 1].X + DefaultDecisionStep;
        }

        public delegate ResistivityMeasurement GetMeasurementForPoint(IContinousState state);

        public bool OfferUpdatePoint(IContinousState newState, GetMeasurementForPoint measuringFunction)
        {
            //TODO add the synthetic truth to the input
            if (!PointOnNextStep(newState))
            {
                return false;
            }
            //correcting for EPS error
            newState.X = GetNextDecisionX();
            _Update(newState, measuringFunction);

            return true;
        }

        private void _Update(IContinousState newState, GetMeasurementForPoint measuringFunction)
        {
            var measurement = measuringFunction(newState);
            //_dataProvider is already refered in the _enkf
            _dataProvider.DataList = new List<IData<IContinousState, ResistivityData2DFull>>{measurement};
            //load up data
            //update enkf
            _enkf.Update();
            _enkf.AcceptUpdate();
        }

        static internal EarthModelManipulator InitializeManipulator(int seed = 0)
        {
            var earthManipulator = new EarthModelManipulator(NumRealiztions, seed);
            //TODO check the layer positions with negative sign

            var topOfLayer2 = 0.0 + Offset;
            var bottomOfLayer2 = - 5.3 + Offset;
            var topOfLayer1 = - 13.3 + Offset;
            var bottomOfLayer1 = - 20.1 + Offset;
            var deviation = 2.5;
            var depthOfReservoirDeviation = 0.0;
            double resistivityLayerBottom = 250;
            double resistivityLayerTop = 150;
            double resistivityOutsideLayer = 10;
            //TODO make the correct layer sorting
            earthManipulator.GenerateRealizationsTwoLayerFromVariogramWithKrigging(NumPointsInLayer, 
                MinX, MaxX,
                bottomOfLayer1, topOfLayer1, bottomOfLayer2, topOfLayer2, deviation,
                resistivityLayerBottom, resistivityLayerTop, resistivityOutsideLayer, false, depthOfReservoirDeviation,
                0.7);
            return earthManipulator;
        }

        private void _InitializeEnkf(EarthModelManipulator eManip)
        {
            _enkf = new EnKF2<IResistivityModel>();
            var instrumentSize = TrueModelState.InstrumentSize;
            _emSim = new NewEMSimulator(instrumentSize);
            //this data provider would be able to 
            //compute resestivity
            //for a pointF
            //and outputs ResistivityData2DFull
            _dataProvider =
                new GenericDataProvider<IResistivityModel, IContinousState, ResistivityData2DFull>(
                    _emSim);
            _enkf.DataProviders.Add(_dataProvider);
            _enkf.SetRealizations(eManip.Realizations);
        }
    }
}