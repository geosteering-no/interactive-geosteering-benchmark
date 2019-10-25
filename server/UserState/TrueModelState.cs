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
        public IEarthModelRealization TrueSubsurfaseModel1 { get; }


        public TrueModelState(int seed)
        {
            TrueSubsurfaseModel1 = _GenerateSyntheticTruthFromSeed(seed);
            _InitializeDataGenrator(TrueSubsurfaseModel1);
        }

        public TrueModelState(IEarthModelRealization syntheticTruth)
        {
            TrueSubsurfaseModel1 = syntheticTruth;
            _InitializeDataGenrator(TrueSubsurfaseModel1);
        }

        private IEarthModelRealization _GenerateSyntheticTruthFromSeed(int randomSeed)
        {
            var eManip = UserState.InitializeManipulator(randomSeed);
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
