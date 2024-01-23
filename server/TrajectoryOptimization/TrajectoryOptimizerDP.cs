using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using TrajectoryInterfaces;

namespace TrajectoryOptimization
{
    //public delegate IValueModel

    public class TrajectoryOptimizerDP<T> 
    {
        //assume we go from left to right
        //assume equidistant discretization in X
        //assume absolute lower and upper bounds for values



        #region Optimazation function inputs




        //public static Dictionary<IValueModel, double> ModelsWeightMapping = new Dictionary<IValueModel, double>();

        #endregion

        //TODO add a deligate for optimization constraints
        #region Hard Optimization constraints

        public double MaxTurnAngle = 3 * Math.PI / 180.0;
        //inclanation that is allowed
        public double MaxInclinationAllowed = double.MaxValue;
        public double MinInclinationAllowed = double.MinValue;

        public double DX
        {
            get { return _dx; }
            set
            {
                _dx = value;
                Dy = 1.0 / 180.0 * Math.PI * _dx;
                dReservoirValue = 1.0 / _dx;
            }
        }



        #endregion Optimization constraints


        #region trajectory consts

        private double _dx = 4.5;
        internal double dReservoirValue = 1.0;
        public double Dy { get; private set;  } = 0.12;
        public int StateTranzitionSamplingResolution { get; set; } = 10;

        public double DiscountFactor { get; set; } = 1.0;

        #endregion

        #region Trajectory helper funcs
        private IncompleteTrajectoryState GetDiscreteState(ContinousState continousStateTuple, TrajectoryOptimizationDPResult<T> result)
        {
            return GetDiscreteState(continousStateTuple.X, continousStateTuple.Y, continousStateTuple.Alpha, result);
        }

        internal IncompleteTrajectoryState GetDiscreteState(double x, double y, double angle, TrajectoryOptimizationDPResult<T> result)
        {
            double ta = Math.Tan(angle) / Dy * DX;
            int angleJump = (int)(Math.Abs(ta) + 0.5) * Math.Sign(ta);
            if (result.OriginX == null || result.OriginY == null)
            {
                result.OriginX = x;
                result.OriginY = y;
                result.ZeroAngle = angle;
            }
            var cur = new IncompleteTrajectoryState()
            {
                X = (int)(Math.Abs((x - result.OriginX.Value) / DX) + 0.5) * Math.Sign(x - result.OriginX.Value),
                Y = (int)(Math.Abs((y - result.OriginY.Value) / Dy) + 0.5) * Math.Sign(y - result.OriginY.Value),
                AngleJumpInY = angleJump
            };
            return cur;
        }

        private Tuple<ContinousState, ContinousState> GetSegmentFromTo(IncompleteTrajectoryState discreteSate,
            TrajectoryOptimizationDPResult<T> result)
        {
            ContinousState from = GetContinousStateBefore(discreteSate, result);
            ContinousState to = GetContinousState(discreteSate, result);
            return new Tuple<ContinousState, ContinousState>(from, to);
        }

        private ContinousState GetContinousState(IncompleteTrajectoryState discreteSate,
            TrajectoryOptimizationDPResult<T> result)
        {
            var angle1 = Math.Atan2(Dy * discreteSate.AngleJumpInY, DX);
            var contState = new ContinousState()
            {
                X = DX * discreteSate.X + result.OriginX.Value,
                Y = Dy * discreteSate.Y + result.OriginY.Value,
                Alpha = angle1
            };
            return contState;
        }

        private ContinousState GetContinousStateBefore(IncompleteTrajectoryState discreteSate,
            TrajectoryOptimizationDPResult<T> result)
        {
            var x = DX * (discreteSate.X - 1) + result.OriginX.Value; 
            var y = Dy * (discreteSate.Y - discreteSate.AngleJumpInY) + result.OriginY.Value;
            var contState = new ContinousState()
            {
                X = x,
                Y = y,
                //Alpha = angle1
            };          
            return contState;
        }

        private double GetAngle(IncompleteTrajectoryState discreteSate)
        {
            var angle1 = Math.Atan2(Dy * discreteSate.AngleJumpInY, DX);
            return angle1;
        }

        private PointF GetPointBefore(IncompleteTrajectoryState discreteSate, TrajectoryOptimizationDPResult<T> result)
        {
            return new PointF((float)DX * (discreteSate.X - 1) + (float)result.OriginX.Value,
                (float)Dy * (discreteSate.Y - discreteSate.AngleJumpInY) + (float)result.OriginY.Value);
        }

        private PointF GetPoint(IncompleteTrajectoryState discreteSate, TrajectoryOptimizationDPResult<T> result)
        {
            return new PointF((float)DX * discreteSate.X + (float)result.OriginX.Value,
                (float)Dy * discreteSate.Y + (float)result.OriginY.Value);
        }
        #endregion

        private List<PointF> ComputeTrajectory(IncompleteTrajectoryState startingStateDiscrete, TrajectoryOptimizationDPResult<T> result)
        {
            var pointsList = new List<PointF>();
            var curr = startingStateDiscrete;
            ComputeStatesDiscrete(curr, result);
            while (true)
            {
                pointsList.Add(GetPoint(curr, result));
                //Console.Write(statesForRealiztion[cur].Value + " -> ");
                var incompleteTrajectoryState = result._statesForRealization[curr].BestNextState;
                if (incompleteTrajectoryState != null)
                {
                    curr = (IncompleteTrajectoryState)incompleteTrajectoryState;
                }
                else
                {
                    break;
                }
            }
            //Console.WriteLine();
            return pointsList;
        }

        private IList<ContinousState> ComputeTrajectoryConState(IncompleteTrajectoryState startingStateDiscrete, TrajectoryOptimizationDPResult<T> result)
        {
            var pointsList = new List<ContinousState>();
            var curr = startingStateDiscrete;
            ComputeStatesDiscrete(curr, result);
            while (true)
            {
                pointsList.Add(GetContinousState(curr, result));
                //Console.Write(statesForRealiztion[cur].Value + " -> ");
                var incompleteTrajectoryState = result._statesForRealization[curr].BestNextState;
                if (incompleteTrajectoryState != null)
                {
                    curr = (IncompleteTrajectoryState)incompleteTrajectoryState;
                }
                else
                {
                    break;
                }
            }
            //Console.WriteLine();
            return pointsList;
        }

        public IList<ContinousState> ComputeBestTrajectoryContState(ContinousState startingStateContinous, TrajectoryOptimizationDPResult<T> result)
        {
            var curr = GetDiscreteState(startingStateContinous, result);
            return ComputeTrajectoryConState(curr, result);
        }

        public List<PointF> ComputeTrajectory(ContinousState startingStateContinous, TrajectoryOptimizationDPResult<T> result)
        {
            var curr = GetDiscreteState(startingStateContinous, result);
            return ComputeTrajectory(curr, result);
        }

        public List<PointF> ComputeTrajectoryDefault(TrajectoryOptimizationDPResult<T> result)
        {
            var pointsList = new List<PointF>();
            if (result.LastRequested != null)
            {
                var cur = (IncompleteTrajectoryState)result.LastRequested;
                pointsList = ComputeTrajectory(cur, result);
            }
            return pointsList;
        }

        public List<int> ComputeAnglesDiscrete(double x, double y, double angle, TrajectoryOptimizationDPResult<T> result)
        {
            IncompleteTrajectoryState cur = GetDiscreteState(x, y, angle, result);
            ComputeStatesDiscrete(cur, result);
            var intAngles = new List<int>();
            while (true)
            {
                intAngles.Add(cur.AngleJumpInY);
                //Console.Write(result._statesForRealization[cur].Value + " -> ");
                var incompleteTrajectoryState = result._statesForRealization[cur].BestNextState;
                if (incompleteTrajectoryState != null)
                {
                    cur = (IncompleteTrajectoryState)incompleteTrajectoryState;
                }
                else
                {
                    break;
                }
            }
            return intAngles;
        }


        /// <summary>
        /// Computes the value of switching between steering of segments.
        /// This includes for example cost of steering
        /// Does not do caching
        /// </summary>
        /// <param name="state"></param>
        /// <param name="nextState"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private double ComputeSegmentTransitionValue(IncompleteTrajectoryState state, IncompleteTrajectoryState nextState, TrajectoryOptimizationDPResult<T> result)
        {
            //            if (SteeringCost < 1e-7)
            //            {
            //                return 0;
            //            }
            //            return -ComputeTurnPenalty(state, nextState);
            var value = 0.0;
            foreach (var bendingObjectivePair in _bendingObjectives)
            {
                if (Math.Abs(bendingObjectivePair.Value) < 1e-16)
                {
                    continue;
                }
                value += bendingObjectivePair.Key(result.Model, state, nextState)*bendingObjectivePair.Value;
            }
            return value;
        }

        /// <summary>
        /// Computes transition value for the state
        /// State contains angle, and discretization is pre-set
        /// Therefore the state is actually an edge
        /// Does caching
        /// </summary>
        /// <param name="endOfSegmentState"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private double ComputeDrillingValue(IncompleteTrajectoryState endOfSegmentState, TrajectoryOptimizationDPResult<T> result)
        {
            if (result._transitionValue.ContainsKey(endOfSegmentState))
            {
                return result._transitionValue[endOfSegmentState];
                //TODO add a ceparate cache for each function
            }
            var value = 0.0;
            //value += ComputeReservoirValueTransition(endOfSegmentState);
            //value -= ComputeDrillingCost(endOfSegmentState);
            //add to a table
            foreach (var segmentObjectivePair in _segmentObjectives)
            {
                if (Math.Abs(segmentObjectivePair.Value) < 1e-16)
                {
                    continue;
                }
                value += segmentObjectivePair.Key(result.Model, GetSegmentFromTo(endOfSegmentState, result))
                         *segmentObjectivePair.Value;
            }
            foreach (var segmentObjectivePair in _segmentObjectivesSimple)
            {
                if (Math.Abs(segmentObjectivePair.Value) < 1e-16)
                {
                    continue;
                }

                //var segment = GetSegmentFromTo(endOfSegmentState, result);
                var p1 = GetPointBefore(endOfSegmentState, result);
                var p2 = GetPoint(endOfSegmentState, result);
                value += segmentObjectivePair.Key(result.Model, p1.X, p1.Y, p2.X, p2.Y)
                         * segmentObjectivePair.Value;
            }

            foreach (var pointObjective in _pointObjectives)
            {
                if (Math.Abs(pointObjective.Value) < 1e-16)
                {
                    continue;
                }
                value += ComputeAverageAlongState(result.Model, pointObjective.Key, endOfSegmentState, result) * pointObjective.Value;
            }
            result._transitionValue.Add(endOfSegmentState, value);
            return value;
        }


        //TODO fix this
        private double ComputePointValue(IncompleteTrajectoryState state)
        {
            return 0.0;
        }

        //TODO fixme: this does not integrate but computes an average along segment
        internal double ComputeAverageAlongState(T model, PointValueFunction<T> func, IncompleteTrajectoryState state, TrajectoryOptimizationDPResult<T> result)
        {
            var point1 = GetContinousStateBefore(state, result);
            var point2 = GetContinousState(state, result);
            var totP = StateTranzitionSamplingResolution;
            var sum = 0.0;
            for (int i = 0; i < totP; ++i)
            {
                var x = (point1.X * i + point2.X * (totP - i)) / totP;
                var y = (point1.Y * i + point2.Y * (totP - i)) / totP;
                //var samplingPoint = new PointF(x, y);
                sum += func(model, x,y ) / totP;
            }
            return sum;
        }



        #region computation of cost elements

        #region

        private Dictionary<BendingValueFunction<T>, double> _bendingObjectives =  new Dictionary<BendingValueFunction<T>, double>();
        private Dictionary<SegmentValueFunctionSimple<T>, double> _segmentObjectivesSimple = new Dictionary<SegmentValueFunctionSimple<T>, double>();
        private Dictionary<SegmentValueFunction<T>, double> _segmentObjectives = new Dictionary<SegmentValueFunction<T>, double>();
        private Dictionary<PointValueFunction<T>, double> _pointObjectives = new Dictionary<PointValueFunction<T>, double>();

        /// <summary>
        /// Add a an objective function with weight
        /// An average value for a trajectory segment is used 
        /// //TODO consider changing behaviour
        /// </summary>
        /// <param name="func"></param>
        /// <param name="weight"></param>
        /// <returns>true if this function already existed</returns>
        public bool AddObjectiveFucntion(PointValueFunction<T> func, double weight)
        {
            if (_pointObjectives.ContainsKey(func))
            {
                _pointObjectives[func] = weight;
                return true;
            }
            _pointObjectives.Add(func, weight);
            return false;
        }


        /// <summary>
        /// Add a an objective function with weight
        /// </summary>
        /// <param name="func"></param>
        /// <param name="weight"></param>
        /// <returns>true if this function already existed</returns>
        public bool AddObjectiveFucntion(BendingValueFunction<T> func, double weight)
        {
            if (_bendingObjectives.ContainsKey(func))
            {
                _bendingObjectives[func] = weight;
                return true;
            }
            _bendingObjectives.Add(func, weight);
            return false;
        }

        /// <summary>
        /// Add a an objective function with weight
        /// </summary>
        /// <param name="func"></param>
        /// <param name="weight"></param>
        /// <returns>true if this function already existed</returns>
        public bool AddObjectiveFucntion(SegmentValueFunction<T> func, double weight)
        {
            if (_segmentObjectives.ContainsKey(func))
            {
                _segmentObjectives[func] = weight;
                return true;
            }
            _segmentObjectives.Add(func, weight);
            return false;
        }

        /// <summary>
        /// Add a an objective function with weight
        /// </summary>
        /// <param name="func"></param>
        /// <param name="weight"></param>
        /// <returns>true if this function already existed</returns>
        public bool AddObjectiveFucntion(SegmentValueFunctionSimple<T> func, double weight)
        {
            if (_segmentObjectivesSimple.ContainsKey(func))
            {
                _segmentObjectivesSimple[func] = weight;
                return true;
            }
            _segmentObjectivesSimple.Add(func, weight);
            return false;
        }


        #endregion

        public delegate double BendingValueFunction<T>(
            T model, IncompleteTrajectoryState state, IncompleteTrajectoryState nextState);

        public delegate double SegmentValueFunction<T>(
            T model, Tuple<ContinousState, ContinousState> tuple);

        public delegate double SegmentValueFunctionSimple<T>(
            T model, double x0, double y0, double x1, double y1);

        public delegate double PointValueFunction<T>(
            T model, double x, double z);

        #region defaultdeligates

        /// <summary>
        /// Drilling cost used in compute drilling cost deligate
        /// </summary>
        public double DrillingCost = 1.0;

        public double ComputeDrillingCost<T>(T model, Tuple<ContinousState, ContinousState> pointFromTo)
        {
            //double length = Hypot(DX, DY * nextState.AngleJumpInY);
            double length = Hypot(pointFromTo.Item2.X - pointFromTo.Item1.X, pointFromTo.Item2.Y - pointFromTo.Item1.Y);
            return -length * DrillingCost;
        }

        /// <summary>
        /// Cost for every degree
        /// </summary>
        public double GoingUpCostMult = 0.2;

        public double ComputeGoingUpCost<T>(T model, Tuple<ContinousState, ContinousState> pointFromTo)
        {
            //double length = Hypot(DX, DY * nextState.AngleJumpInY);
            var dx = pointFromTo.Item2.X - pointFromTo.Item1.X;
            var dy = pointFromTo.Item2.Y - pointFromTo.Item1.Y;
            double length = Hypot(dx, dy);
            double angleApprox = dy/dx;
            if (angleApprox <= 0)
            {
                return 0;
            }
            double andgleDeg = angleApprox/Math.PI*180.0;
            return -length * GoingUpCostMult * andgleDeg;
        }




        /// <summary>
        /// This steering cost is to set Compute turn penalty to common scale
        /// </summary>
        public double SteeringCost = 0.1;

        /// <summary>
        /// Computes penalty for turning
        /// </summary>
        /// <param name="model">model is not used but needed for interface</param>
        /// <param name="state"></param>
        /// <param name="nextState"></param>
        /// <returns></returns>
        public double ComputeTurnPenalty<T>(T model, IncompleteTrajectoryState state, IncompleteTrajectoryState nextState)
        {
            var curAngleJumpInY = state.AngleJumpInY;
            var newAngleJumpInY = nextState.AngleJumpInY;
            var angle1 = Math.Atan2(Dy * curAngleJumpInY, DX);
            var angle2 = Math.Atan2(Dy * newAngleJumpInY, DX);
            //normalize to 0..1
            var bend = Math.Abs(angle2 - angle1) / ABSOLUTE_MAX_TURN_ANGLE;
            return -SteeringCost * bend;
        }
        #endregion

        #endregion

        public static double Hypot(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }



        public double ComputeBestValue(ContinousState startingStateContinous, TrajectoryOptimizationDPResult<T> result)
        {
            var value = ComputeBestValue(startingStateContinous.X, startingStateContinous.Y,
                startingStateContinous.Alpha, result);
            return value;
        }

        public double ComputeBestValue(double x, double y, double angle, TrajectoryOptimizationDPResult<T> result)
        {
            var curr = GetDiscreteState(x, y, angle, result);
            result.LastRequested = curr;
            var stateDiscr = ComputeStatesDiscrete(curr, result);
            if (stateDiscr == null)
            {
                throw new Exception("Null exception");
                //TODO fix
            }
            return stateDiscr.Value;
        }


        //TODO this needs to move out of here
        public int? MaxX = null;

        //private delegate double GetLocalValue(IResistivityModel model, IncompleteTrajectoryState state);



        public TrajectoryOptimizerDP()
        {
            dReservoirValue = 1.0 / _dx;
        }


        private bool TurnIsAllowed(int curAngleJumpInY, int newAngleJumpInY, double maxTurnAngleRad)
        {
            var angle1 = Math.Atan2(Dy * curAngleJumpInY, DX);
            var angle2 = Math.Atan2(Dy * newAngleJumpInY, DX);
            if (angle2 > MaxInclinationAllowed)
            {
                return false;
            }

            if (angle2 < MinInclinationAllowed)
            {
                return false;
            }
            if (Math.Abs(angle1 - angle2) >= maxTurnAngleRad)
            {
                return false;
            }

            return true;
        }

        public const double ABSOLUTE_MAX_TURN_ANGLE = 3 * Math.PI / 180.0;

        public TrajectoryState ComputeStatesDiscrete(TrajectoryOptimizationDPResult<T> result)
        {
            if (result.LastRequested != null)
            {
                return ComputeStatesDiscrete((IncompleteTrajectoryState)result.LastRequested, result);
            }
            throw new Exception("No computed states");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<ContinousState> GetProposedNextPointsAndAnglesDefault(TrajectoryOptimizationDPResult<T> result)
        {
            Debug.Assert(result.LastRequested != null, "result.LastRequested != null");
            var curstate = (IncompleteTrajectoryState)result.LastRequested;
            var pointList = new List<ContinousState>();
            for (int newY = curstate.Y + curstate.AngleJumpInY;
                TurnIsAllowed(curstate.AngleJumpInY, newY - curstate.Y, MaxTurnAngle);
                ++newY)
            {
                var nextState = new IncompleteTrajectoryState()
                {
                    X = curstate.X + 1,
                    Y = newY,
                    AngleJumpInY = newY - curstate.Y
                };
                pointList.Add(GetContinousState(nextState, result));
            }
            for (int newY = curstate.Y + curstate.AngleJumpInY - 1;
                TurnIsAllowed(curstate.AngleJumpInY, newY - curstate.Y, MaxTurnAngle);
                --newY)
            {
                var nextState = new IncompleteTrajectoryState()
                {
                    X = curstate.X + 1,
                    Y = newY,
                    AngleJumpInY = newY - curstate.Y
                };
                pointList.Add(GetContinousState(nextState, result));
            }
            return pointList;
        }

        public double ComnputeOutcome(ContinousState currState, ContinousState nextState,
            TrajectoryOptimizationDPResult<T> result)
        {
            var curStateDiscr = GetDiscreteState(currState, result);
            var nextStateDiscr = GetDiscreteState(nextState, result);
            return ComnputeOutcome(curStateDiscr, nextStateDiscr, result);
        }
        
        private double ComnputeOutcome(IncompleteTrajectoryState currState, IncompleteTrajectoryState nextState, TrajectoryOptimizationDPResult<T> result)
        {
            double value = ComputeDrillingValue(nextState, result) + ComputeSegmentTransitionValue(currState, nextState, result);
            return value;
        }

        //private double ComputeDiscountMultiplyer(IncompleteTrajectoryState currState)
        //{
        //    return Math.Pow(DiscountFactor, currState.X);
        //}

        public double ComputeValueGivenNextState(ContinousState continousState0, 
            ContinousState continousStateNext, TrajectoryOptimizationDPResult<T> result)
        {
            var discrState0 = GetDiscreteState(continousState0, result);
            var discrState1 = GetDiscreteState(continousStateNext, result);
            var res = ComputeValueToNextStep(discrState0, discrState1, result);
            return res.Item1;
        }

        private Tuple<double, IncompleteTrajectoryState> ComputeValueToNextStep(IncompleteTrajectoryState currState, IncompleteTrajectoryState nextState, 
            TrajectoryOptimizationDPResult<T> result)
        {
            var nextFullState = ComputeStatesDiscrete(nextState, result);
            //we multiply next state by discount due to uncertainty
            var drillingValue = ComputeDrillingValue(nextState, result);
            var segmentTranzitionValue = ComputeSegmentTransitionValue(currState, nextState, result);
            var value = nextFullState.Value * DiscountFactor
                        + drillingValue
                        + segmentTranzitionValue;
            return new Tuple<double, IncompleteTrajectoryState>(value, nextFullState.IncompleteState);
        }

        /// <summary>
        /// Computes states values of the states along trajectroy 
        /// </summary>
        /// <param name="currState">The starting state of the trajectory</param>
        /// <param name="result"></param>
        /// <returns></returns>
        private TrajectoryState ComputeStatesDiscrete(IncompleteTrajectoryState currState, TrajectoryOptimizationDPResult<T> result)
        {
            result.CallCount++;
            //already computed
            if (result._statesForRealization.ContainsKey(currState))
            {
                return result._statesForRealization[currState];
            }
            //TODO add angle information and integrate
            //compute current value 
            var localValue = ComputePointValue(currState);
            //TODO the value associated with state is lees correct than an integral over path
            //Fix might be required 
            var trState = new TrajectoryState()
            {
                IncompleteState = currState,
                Value = localValue,
            };
            //finished the job
            if (currState.X >= MaxX)
            {
                result._statesForRealization.Add(currState, trState);
                return result._statesForRealization[currState];
            }

            //var discountMult = ComputeDiscountMultiplyer(currState);

            //recursion
            var maxValue = 0.0;
            IncompleteTrajectoryState? nextBest = null;
            //go up and straight
            for (int newY = currState.Y + currState.AngleJumpInY; TurnIsAllowed(currState.AngleJumpInY, newY - currState.Y, MaxTurnAngle); ++newY)
            {
                var nextState = new IncompleteTrajectoryState()
                {
                    X = currState.X + 1,
                    Y = newY,
                    AngleJumpInY = newY - currState.Y
                };

                var res = ComputeValueToNextStep(currState, nextState, result);
                if (maxValue < res.Item1)
                {
                    maxValue = res.Item1;
                    nextBest = res.Item2;
                }
            }
            //go down
            for (int newY = currState.Y + currState.AngleJumpInY - 1; TurnIsAllowed(currState.AngleJumpInY, newY - currState.Y, MaxTurnAngle); --newY)
            {
                var nextState = new IncompleteTrajectoryState()
                {
                    X = currState.X + 1,
                    Y = newY,
                    AngleJumpInY = newY - currState.Y
                };
                var res = ComputeValueToNextStep(currState, nextState, result);
                if (maxValue < res.Item1)
                {
                    maxValue = res.Item1;
                    nextBest = res.Item2;
                }
            }
            //multiplying by discout factor
            
            //maxValue*=
            trState.Value += maxValue;
            trState.BestNextState = nextBest;

            result._statesForRealization.Add(currState, trState);
            return result._statesForRealization[currState];
        }

        /// <summary>
        /// returns continous version of the state
        /// </summary>
        /// <param name="result">the result</param>
        /// <returns></returns>
        public ContinousState GetLastRequestedContinous(TrajectoryOptimizationDPResult<T> result)
        {
            return GetContinousState((IncompleteTrajectoryState)result.LastRequested, result);
        }

    }

    public class TrajectoryOptimizationDPResult<T>
    {
        //internal TrajectoryOptimizerDP<T> Optimizer { get; set; }
        internal readonly T Model;

        public TrajectoryOptimizationDPResult(T model)
        {
            this.Model = model;
        }

        public ContinousState ZeroStateContinous
        {
            get
            {
                if (OriginX == null || OriginY == null || ZeroAngle == null)
                {
                    return null;
                }

                return new ContinousState()
                {
                    X = OriginX.Value,
                    Y = OriginY.Value, 
                    Alpha = ZeroAngle.Value
                };
            }
        }



        public int CallCount = 0;
        internal Dictionary<IncompleteTrajectoryState, TrajectoryState> _statesForRealization =
            new Dictionary<IncompleteTrajectoryState, TrajectoryState>(50000);
        internal Dictionary<IncompleteTrajectoryState, double> _transitionValue =
            new Dictionary<IncompleteTrajectoryState, double>(50000);


        public IncompleteTrajectoryState? LastRequested = null;
        public double? OriginX = null;
        public double? OriginY = null;
        public double? ZeroAngle = null;

    }

    public struct IncompleteTrajectoryState
    {
        public int X;
        public int Y;
        public int AngleJumpInY;
    }

    public class TrajectoryState
    {
        public IncompleteTrajectoryState IncompleteState;
        public double Value;
        public IncompleteTrajectoryState? BestNextState;
    }
}
