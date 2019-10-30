using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResistivitySimulator;
using ServerDataStructures;
using ServerObjectives;
using ServerStateInterfaces;
using TrajectoryOptimization;

namespace UserState
{
    public class DssBotGeneric<TRealizationData> : IDssBotGeneric<WellPoint, TRealizationData>
    {
        private TrajectoryOptimizerDP<TRealizationData> _oneRealizationOptimizer;
        private IList<TrajectoryOptimizationDPResult<TRealizationData>> _oneRealizationResults;
        private MultiRalizationOptimizer _multiOptimizer = new MultiRalizationOptimizer();
        //private TheServerObjective _theObjective = new TheServerObjective();
        protected ObjectiveEvaluatorDelegateTruth<TRealizationData, WellPoint>.ObjectiveEvaluationFunctionSimple
            Evaluator
        { get;
            private set;
        }

        public void Init(double discountFactorOptimization, double decisionLenX, int totalOptSteps, double maxTurnAngle,
            double minInclination, ObjectiveEvaluatorDelegateTruth<TRealizationData, WellPoint>.ObjectiveEvaluationFunctionSimple 
                simpleEvaluator)
        {
            var oneRelaizationOptimizerSpecific = new TrajectoryOptimizerDP<TRealizationData>()
            {
                DiscountFactor = discountFactorOptimization
            };
            _oneRealizationOptimizer = oneRelaizationOptimizerSpecific;
            _oneRealizationOptimizer.DX = decisionLenX;
            //TotalOptSteps = (int)(deltaX / decisionLenX + 0.5) + 1;

            _oneRealizationOptimizer.MaxX = totalOptSteps;
            _oneRealizationOptimizer.MaxTurnAngle = maxTurnAngle;
            _oneRealizationOptimizer.MinInclinationAllowed = minInclination;
            Evaluator = simpleEvaluator;
            _oneRealizationOptimizer.AddObjectiveFucntion(Evaluate, 1.0);
        }

        private double Evaluate(TRealizationData realization, double x1, double y1, double x2, double y2)
        {
            return Evaluator(realization, x1, y1, x2, y2);
        }

        private void Init(double discountFactorOptimization, double decisionLenX, int totalOptSteps,
            double maxTurnAngle, double minInclination)
        {
            var oneRelaizationOptimizerSpecific = new TrajectoryOptimizerDP<TRealizationData>()
            {
                DiscountFactor = discountFactorOptimization
            };
            _oneRealizationOptimizer = oneRelaizationOptimizerSpecific;
            _oneRealizationOptimizer.DX = decisionLenX;
            //TotalOptSteps = (int)(deltaX / decisionLenX + 0.5) + 1;

            _oneRealizationOptimizer.MaxX = totalOptSteps;
            _oneRealizationOptimizer.MaxTurnAngle = maxTurnAngle;
            _oneRealizationOptimizer.MinInclinationAllowed = minInclination;

            //TODO add the correct objective
            _oneRealizationOptimizer.AddObjectiveFucntion(Evaluate, 1.0);
        }

        private TrajectoryOptimizationDPResult<TRealizationData> UpdateSingleOptimizer(TrajectoryOptimizerDP<TRealizationData> optimizer, 
            TRealizationData model, WellPoint wp)
        {
            var resultCache = new TrajectoryOptimizationDPResult<TRealizationData>(model);
            var result =
                optimizer.ComputeBestValue(wp.X, wp.Y, wp.Angle, resultCache);
            Console.WriteLine("Result: " + result + " -- call count " + resultCache.CallCount);
            return resultCache;
        }

        private void UpdateForEach(IList<TRealizationData> realizations, WellPoint wp)
        {
            _oneRealizationResults = new List<TrajectoryOptimizationDPResult<TRealizationData>>();
            foreach (var realization in realizations)
                _oneRealizationResults.Add(
                    UpdateSingleOptimizer(_oneRealizationOptimizer, realization, wp));
        }

        public IList<WellPoint> ComputeBestDeterministicTrajectory(TRealizationData realization, WellPoint start)
        {
            var oneRealizationResult = new TrajectoryOptimizationDPResult<TRealizationData>(realization);
            var startState = ToContinousState(start);
            var contStateResult =
                _oneRealizationOptimizer.ComputeBestTrajectoryContState(startState, oneRealizationResult);
            var result = contStateResult.Select(x => ToWellPoint(x));

            return result.ToList();
        }

        public static ContinousState ToContinousState(WellPoint pt)
        {
            return new ContinousState()
            {
                X = pt.X,
                Y = pt.Y,
                Alpha = pt.Angle
            };
        }

        public static WellPoint ToWellPoint(ContinousState x)
        {
            return new WellPoint()
            {
                X = x.X,
                Y = x.Y,
                Angle = x.Alpha
            };
        }



        public WellPoint ComputeBestChoice(IList<TRealizationData> realizations, WellPoint wp, int totalRemaining)
        {
            _oneRealizationOptimizer.MaxX = totalRemaining;
            UpdateForEach(realizations, wp);
            var bestNextKeyValue = _multiOptimizer.ComputeNextPointAndAngleAndValueDefault(_oneRealizationOptimizer,
                _oneRealizationResults);
            return ToWellPoint(bestNextKeyValue.Key);
        }
    }
}
