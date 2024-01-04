using System;
using System.Drawing;
using NUnit.Framework;

namespace TrajectoryOptimization
{
    [TestFixture]
    public class TrajectoryOptimizerDP_Test : TrajectoryOptimizerDP<SimpleLayeredObject>
    {
        //private TrajectoryOptimizerDP<SimpleLayeredObject> _optimizer;

        [Test]
        public void IntegrateAlongState_GivesTrajectoryLen()
        {
            var wellCurrentAngle = (80.0 - 90.0) / 180.0 * Math.PI;
            var decisionLenX = 29.0 * Math.Cos(wellCurrentAngle) * 1.0;
            var optimizer = new TrajectoryOptimizerDP<SimpleLayeredObject>()
            {
                DX = decisionLenX,
            };
            var model = new SimpleLayeredObject();
            var computationDPResult = new TrajectoryOptimizationDPResult<SimpleLayeredObject>(model);
            var incState = optimizer.GetDiscreteState(0, 0, 0, computationDPResult);
            var result = optimizer.ComputeAverageAlongState(model, SimpleLayeredObject.PointValueOne,
                incState, computationDPResult);
            
            Assert.Less(Math.Abs(result- 1.0), 1e-7);

        }

        //[SetUp]
        //public void SetUp()
        //{
        //    var numRealizations = 30;
        //    _eManip = new EarthModelManipulator(numRealizations);
        //    _eManip.GenerateRealizationsInclined(20, 0, 50, 0, 8, 0); // EDIT init here
        //    _earthModel = _eManip.Realizations[0];

        //    _optimizer = new TrajectoryOptimizerDP()
        //    {
        //        ValueModel = _earthModel.ValueModel
        //    };
        //    DrillingCost = 0.0;
        //    SteeringCost = 0.0;
        //}

        ////TODO finish this test for consistency
        ////        [Test]
        ////        public void GetPointIsConsistentWithGetDiscreteState()
        ////        {
        ////            GetPoint(GetDicreteState(startingStateContinous));
        ////        }


        //[Test]
        //public void ComputeBestValue_WithSteeringCostGivesAdequateResult()
        //{
        //    //DrillingCost = 0.04;
        //    SteeringCost = 0.04;
        //    var result = _optimizer.ComputeBestValue(0, -2, 0);
        //    System.Console.WriteLine("was " + result);
        //    System.Console.WriteLine("difference " + (4.67879630772237 - result));

        //    Assert.LessOrEqual(Math.Abs(4.67879630772237 - result), 1e-9);
        //    System.Console.WriteLine(_optimizer.CallCount);
        //}

        //[Test]
        //public void ComputeBestValue_GivesMaximumWhenStartingInTheMidel()
        //{
        //    //TODO update to non-static random in the model for this to work all the time
        //    var result = _optimizer.ComputeBestValue(0, 0, 0);
        //    var list = _optimizer.ComputeTrajectoryDefault();
        //    foreach (var pointF in list)
        //    {
        //        System.Console.Write("-->" + pointF);
        //    }
        //    Assert.AreEqual(TrajectoryOptimizerDP.MaxX, result);
        //    System.Console.WriteLine(result);
        //    System.Console.WriteLine(_optimizer.CallCount);
        //}

        //[Test]
        //public void ComputeBestValue_GivesPreSetWhenEnteringFromAbove()
        //{
        //    var result = _optimizer.ComputeBestValue(0, -2, 0);
        //    Assert.LessOrEqual(Math.Abs(4.7999999999999998 - result), 1e-9);
        //    System.Console.WriteLine(result);
        //    System.Console.WriteLine(_optimizer.CallCount);
        //}

        //[Test]
        //public void ComputeBestValue_GivesWithCostPreSetWhenEnteringFromAbove()
        //{
        //    DrillingCost = 0.04;
        //    var result = _optimizer.ComputeBestValue(0, -2, 0);
        //    Assert.LessOrEqual(Math.Abs(2.9907566617998231 - result), 1e-9);
        //    System.Console.WriteLine("was " + result);
        //    System.Console.WriteLine("difference " + (2.9907566617998231 - result));
        //    System.Console.WriteLine(_optimizer.CallCount);
        //}


        //[Test]
        //public void ComputeBestValue_ForMultiple()
        //{
        //    var bestValue = 0.0;
        //    foreach (var earthModelRelization in _eManip.Realizations)
        //    {
        //        _optimizer = new TrajectoryOptimizerDP()
        //        {
        //            ValueModel = earthModelRelization.ValueModel
        //        };
        //        var oldResult = bestValue;
        //        var result = _optimizer.ComputeBestValue(0, -2, 0);
        //        bestValue = Math.Max(result, oldResult);
        //        Assert.LessOrEqual(oldResult, bestValue);
        //        System.Console.WriteLine("Result: " + result + " -- call count " + _optimizer.CallCount);
        //    }
        //}

        //[Test]
        //public void ComputeAnglesDiscrete_ForLandingGivesReasonableList()
        //{
        //    var anglesDiscrete = _optimizer.ComputeAnglesDiscrete(0, -4, Math.PI / 10);
        //    var time = DateTime.Now;
        //    var result = _optimizer.ComputeBestValue(0, -4, Math.PI / 10);
        //    var newTime = DateTime.Now;
        //    Assert.Less((newTime - time).TotalMilliseconds, 10);

        //    Console.WriteLine();
        //    Console.WriteLine(result);

        //    foreach (var i in anglesDiscrete)
        //    {
        //        Console.Write(i + " -> ");
        //    }
        //}

        //[Test]
        //public void ComputeBestValue_ForMultipleLanding()
        //{
        //    var bestValue = 0.0;
        //    foreach (var earthModelRelization in _eManip.Realizations)
        //    {
        //        _optimizer = new TrajectoryOptimizerDP()
        //        {
        //            ValueModel = earthModelRelization.ValueModel
        //        };
        //        var oldResult = bestValue;
        //        var result = _optimizer.ComputeBestValue(0, -4, Math.PI / 8);
        //        bestValue = Math.Max(result, oldResult);
        //        Assert.LessOrEqual(oldResult, bestValue);
        //        System.Console.WriteLine("Result: " + result + " -- call count " + _optimizer.CallCount);
        //    }
        //}
    }

    public class SimpleLayeredObject
    {
        public static double PointValueOne(SimpleLayeredObject model, double x, double z)
        {
            return 1;
        }
    }
}