﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResistivitySimulator;
using ServerDataStructures;
using ServerObjectives;
using TrajectoryOptimization;

namespace UserState
{
    public class DssBot
    {
        private TrajectoryOptimizerDP<RealizationData> _oneRealizationOptimizer;
        private IList<TrajectoryOptimizationDPResult<RealizationData>> _oneRealizationResults;
        private MultiRalizationOptimizer _multiOptimizer = new MultiRalizationOptimizer();
        private TheServerObjective _theObjective = new TheServerObjective();
        public void Init(double _discountFactorOptimization, double decisionLenX, int TotalOptSteps,
            double maxTurnAngle, double minInclination)
        {
            var oneRelaizationOptimizerSpecific = new TrajectoryOptimizerDP<RealizationData>()
            {
                DiscountFactor = _discountFactorOptimization
            };
            _oneRealizationOptimizer = oneRelaizationOptimizerSpecific;
            _oneRealizationOptimizer.DX = decisionLenX;
            //TotalOptSteps = (int)(deltaX / decisionLenX + 0.5) + 1;

            _oneRealizationOptimizer.MaxX = TotalOptSteps - 1;
            _oneRealizationOptimizer.MaxTurnAngle = maxTurnAngle;
            _oneRealizationOptimizer.MinInclinationAllowed = minInclination;

            //TODO add the correct objective
            _oneRealizationOptimizer.AddObjectiveFucntion(_theObjective.TheObjective, 1.0);
        }

        private TrajectoryOptimizationDPResult<T> UpdateSingleOptimizer<T>(TrajectoryOptimizerDP<T> optimizer, T model, WellPoint wp)
        {
            var resultCache = new TrajectoryOptimizationDPResult<T>(model);
            var result =
                optimizer.ComputeBestValue(wp.X, wp.Y, wp.Angle, resultCache);
            Console.WriteLine("Result: " + result + " -- call count " + resultCache.CallCount);
            return resultCache;
        }

        private void UpdateForEach(IList<RealizationData> realizations, WellPoint wp)
        {
            _oneRealizationResults = new List<TrajectoryOptimizationDPResult<RealizationData>>();
            foreach (var realization in realizations)
                _oneRealizationResults.Add(
                    UpdateSingleOptimizer(_oneRealizationOptimizer, realization, wp));
        }

        public IList<WellPoint> ComputeBestDeterministicTrajectory(RealizationData realization, WellPoint start)
        {
            var oneRealizationResult = new TrajectoryOptimizationDPResult<RealizationData>(realization);
            var startState = new ContinousState()
            {
                X = start.X,
                Y = start.Y,
                Alpha = start.Angle
            };
            var contStateResult =
                _oneRealizationOptimizer.ComputeBestTrajectoryContState(startState, oneRealizationResult);
            var result = contStateResult.Select(x => new WellPoint()
            {
                X = x.X,
                Y = x.Y,
                Angle = x.Alpha
            });

            return result.ToList();
        }

        public WellPoint ComputeBestChoice(IList<RealizationData> realizations, WellPoint wp)
        {
            UpdateForEach(realizations, wp);
            var bestNextKeyValue = _multiOptimizer.ComputeNextPointAndAngleAndValueDefault(_oneRealizationOptimizer,
                _oneRealizationResults);
            throw new NotImplementedException();
        }
    }
}
