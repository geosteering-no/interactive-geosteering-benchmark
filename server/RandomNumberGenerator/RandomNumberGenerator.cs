using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyFunctions.Default.RandomNumberGenerator
{
    public class RandomNumberGenerator
    {
        //TODO consider changing static random
        Random rnd = new Random(0);
        

        public RandomNumberGenerator(int n)
        {
            rnd = new Random(n);
        }

        public RandomNumberGenerator()
        {
            rnd = new Random();
        }

        public double NextNormalRandomValue(double mean = 0, double stdDev = 1)
        {
            double u1 = rnd.NextDouble(); //these are uniform(0,1) random doubles
            double u2 = rnd.NextDouble();
            double randStdNormal = System.Math.Sqrt(-2.0 * System.Math.Log(u1)) *
                         System.Math.Sin(2.0 * System.Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }

        public double NextNormalRandomValueStatic(double mean = 0, double stdDev = 1)
        {
            double u1 = rnd.NextDouble(); //these are uniform(0,1) random doubles
            double u2 = rnd.NextDouble();
            double randStdNormal = System.Math.Sqrt(-2.0 * System.Math.Log(u1)) *
                         System.Math.Sin(2.0 * System.Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }

        public int RandomInt(int max)
        {
            return rnd.Next(max);
        }

        public double RandomUnifrormDouble()
        {
            return rnd.NextDouble();
        }

        public double RandomUnifrormDouble(double min, double max)
        {
            double diff = max - min;
            return min+rnd.NextDouble()/diff;
        }

    }
}
