using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;


namespace TrajectoryOptimization
{
    public class MultiRalizationOptimizer
    {

//        public void ClearStates()
//        {
//            _nextBestKeyValue = null;
//            foreach (var oneRealizationOptimizer in OneRealizationOptimizers)
//            {
//                oneRealizationOptimizer.ClearStates();
//            }
//        }

        //public int AlternativeIndex
        //{
        //    get
        //    {
        //        var proposedNext = optimizer.GetProposedNextPointsAndAnglesDefault(oneRealizationResults.First());
        //    }
        //}

        public KeyValuePair<ContinousState, double> NextBestKeyValue
        {
            get
            {
                if (_nextBestKeyValue == null)
                {
                    throw new NotSupportedException("This use should be discontinued");

                }
                Debug.Assert(_nextBestKeyValue != null, "_nextBestKeyValue != null");
                return (KeyValuePair<ContinousState, double>) _nextBestKeyValue;
            }
        }

        public List<ContinousState> ProposedNext
        {
            get { return _proposedNext; }
        }


        //public ContinousState ZeroStateContinous => OneRealizationOptimizers[0].ZeroStateContinous;

        private KeyValuePair<ContinousState, double>? _nextBestKeyValue = null;
        //private ContinousState _startingPoint = null;

        /// <summary>
        /// Computes the weighted value of outcomes starting from the position specified by tuple
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optimizer"></param>
        /// <param name="oneRealizationResults"></param>
        /// <param name="tuple"></param>
        /// <returns></returns>
        private double ComputeFullWeightedValue<T>(TrajectoryOptimizerDP<T> optimizer,
            IList<TrajectoryOptimizationDPResult<T>> oneRealizationResults, ContinousState tuple)
        {
            var meanValueForChoice = 0.0;
            foreach (var trajectoryOptimizationDpResult in oneRealizationResults)
            {
                var value = optimizer.ComputeBestValue(tuple.X, tuple.Y, tuple.Alpha, trajectoryOptimizationDpResult);
                meanValueForChoice += value / oneRealizationResults.Count;
            }
            return meanValueForChoice;
        }

        /// <summary>
        /// Computes the weighted value of outcomes starting from the position specified by tuple
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optimizer"></param>
        /// <param name="oneRealizationResults"></param>
        /// <param name="nextState"></param>
        /// <returns></returns>
        private double ComputeWeightedOutcomeForSegment<T>(TrajectoryOptimizerDP<T> optimizer,
            IList<TrajectoryOptimizationDPResult<T>> oneRealizationResults, ContinousState nextState)
        {
            Debug.Assert(oneRealizationResults != null, nameof(oneRealizationResults) + " != null");
            var firstState = optimizer.GetLastRequestedContinous(oneRealizationResults.First());
            
            var meanValueForChoice = 0.0;
            foreach (var trajectoryOptimizationDpResult in oneRealizationResults)
            {
                var value = optimizer.ComnputeOutcome(firstState, nextState, trajectoryOptimizationDpResult);
                meanValueForChoice += value / oneRealizationResults.Count;
            }
            return meanValueForChoice;
        }

        List<ContinousState> _proposedNext;

        //TODO make non-default extension
        public KeyValuePair<ContinousState, double> ComputeNextPointAndAngleAndValueDefault<T>
            (TrajectoryOptimizerDP<T> optimizer, IList<TrajectoryOptimizationDPResult<T>> oneRealizationResults)
        {
            //_startingPoint = OneRealizationOptimizers[0].ZeroStateContinous;

            _proposedNext = optimizer.GetProposedNextPointsAndAnglesDefault(oneRealizationResults.First());
            var bestValue = 0.0;
            ContinousState bestNext = null;
            foreach (var nextState in ProposedNext)
            {
                //nothing
                var meanValueForChoice = 0.0;
                //immidiate reward
                meanValueForChoice += ComputeWeightedOutcomeForSegment(optimizer, oneRealizationResults, nextState);
                //future reward
                meanValueForChoice += optimizer.DiscountFactor * ComputeFullWeightedValue(optimizer, oneRealizationResults, nextState);
                //applying same discount factor
                if (meanValueForChoice > bestValue)
                {
                    bestValue = meanValueForChoice;
                    bestNext = nextState;
                }
            }
            _nextBestKeyValue = new KeyValuePair<ContinousState, double>(bestNext, bestValue);
            //return NextBestKeyValue;
            return _nextBestKeyValue.Value;
        }
    }
}