using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using ResistivitySimulator;
using TrajectoryInterfaces;

namespace UserState
{
    public class TrueModelState
    {
        public const double InstrumentSize = 3.0;
        private MultiDataGenerator _trueModel;
        private IResistivityModel _trueSubsurfaseModel1;


        public TrueModelState(int seed)
        {
            _trueSubsurfaseModel1 = _GenerateSyntheticTruthFromSeed(seed);
            _InitializeDataGenrator(_trueSubsurfaseModel1);
        }

        public TrueModelState(IResistivityModel syntheticTruth)
        {
            _trueSubsurfaseModel1 = syntheticTruth;
            _InitializeDataGenrator(_trueSubsurfaseModel1);
        }

        private IResistivityModel _GenerateSyntheticTruthFromSeed(int randomSeed)
        {
            var eManip = UserState.InitializeManipulator();
            var model = eManip.Realizations[0];
            return model;
        }

        private void _InitializeDataGenrator(IResistivityModel earthmodel)
        {
            _trueModel = new MultiDataGenerator(earthmodel, InstrumentSize);
        }

        public ResistivityMeasurement GetData(IContinousState pos)
        {
            var v = _trueModel.GetData(pos);
            var fullMeasurement = new ResistivityMeasurement(pos, new ResistivityData2DFull(v),
                new ResistivityData2DFull(_trueModel.GetDataVariance()));
            return fullMeasurement;
        }
    }
}
