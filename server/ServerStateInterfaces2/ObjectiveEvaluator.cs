using System;
using System.Collections.Generic;
using ServerDataStructures;
using ServerObjectives;

namespace ServerStateInterfaces
{
    public class ObjectiveEvaluator
    {
        private TheServerObjective localEvaluator = new TheServerObjective();
        

        public double EvaluateOneRealizationDefault(RealizationData realizationData, IList<WellPoint> trajectory)
        {
            var sum = 0.0;
            for (int i = 1; i < trajectory.Count; ++i)
            {
                var segmentResult = localEvaluator.TheObjective(realizationData,
                    trajectory[i - 1].X, trajectory[i - 1].Y,
                    trajectory[i].X, trajectory[i].Y);
                sum += segmentResult;
            }
            return sum;
        }



        public ObjectiveEvaluatorDelegateTruth<RealizationData, WellPoint>.ObjectiveEvaluationFunctionSimple EvaluatorDelegate
        {
            get
            {
                return localEvaluator.TheObjective;
            }
        }

        public UserEvaluation EvaluateDefault(UserData userData, IList<WellPoint> trajectory)
        {
            var values = new List<double>(userData.realizations.Count);
            foreach (var realizationData in userData.realizations)
            {
                var res = EvaluateOneRealizationDefault(realizationData, trajectory);
                values.Add(res);
            }
            var inds = new List<int>(userData.realizations.Count);
            var ind = 0;
            foreach (var realization in userData.realizations)
            {
                inds.Add(ind);
                ind++;
            }
            inds.Sort((a, b) => Math.Sign(values[a] - values[b]));

            var result = new UserEvaluation()
            {
                RealizationScores = values,
                SortedIndexes = inds
            };
            return result;
        }


    }
}