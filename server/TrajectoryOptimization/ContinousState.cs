using System;
using System.Drawing;
using TrajectoryInterfaces;

namespace TrajectoryOptimization
{
    public class ContinousState : IContinousState
    {
        public ContinousState()
        {

        }

        public ContinousState(double x, double y, double alpha)
        {
            X = x;
            Y = y;
            Alpha = alpha;
        }

        public ContinousState(double x, double y)
        {
            X = x;
            Y = y;
            Alpha = double.NaN;
        }

        private double _Hypot(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        public double GetDistance(IContinousState position)
        {
            var dist = _Hypot(this.X - position.X, this.Y - position.Y);
            return dist;
        }

        public ContinousState(PointF p, double alpha)
        {
            X = p.X;
            Y = p.Y;
            Alpha = alpha;
        }

        public PointF Item1 => new PointF((float)X, (float)Y);

        public double X { get; set; }
        public double Y { get; set; }
        public double Alpha { get; set; } = double.NaN;
    }
}
