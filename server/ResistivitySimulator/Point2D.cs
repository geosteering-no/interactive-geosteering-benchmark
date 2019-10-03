using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResistivitySimulator
{
    public class Point2DDouble
    {
        public double X;
        public double Y;

        public Point2DDouble()
        {

        }

        public Point2DDouble(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public static Point2DDouble operator + (Point2DDouble a, Point2DDouble b)
        {
            return new Point2DDouble()
            {
                X = a.X + b.X,
                Y = a.Y + b.Y
            };
        }
    }
}
