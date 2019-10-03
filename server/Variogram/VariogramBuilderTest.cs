using System.Collections.Generic;
using NUnit.Framework;

namespace Variogram
{
    [TestFixture]
    public class VariogramBuilderTest
    {
        [SetUp]
        public void SetUp()
        {
            
        }

        [Test]
        public void Init_Works()
        {
            IList<double> points = new List<double>()
            {
                0.1,
                0.2,
                0.5
            };

            VariogramBuilder v = new VariogramBuilder(points, new ExpVariogram(0.1, 1.0, 2.0), 2.0);
            var vec = v.DrawVector();
            vec = v.DrawVector();
            vec = v.DrawVector();
            vec = v.DrawVector();
            //TODO test that the vectors are properly corelated
        }
    }
}