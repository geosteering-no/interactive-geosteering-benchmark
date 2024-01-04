using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ResistivitySimulator
{
    public class ResestivityObjective
    {
        /// <summary>
        /// Everything below that is saturated with water
        /// </summary>
        public double WaterSaturatedShaleReference = 20;


        /// <summary>
        /// Everything above that is good reservoir sand
        /// </summary>
        public double GoodReservoirLimit = 210;

        public double ReservoirThicknessMultiplier = 7.0;

        private double _ReservoirQualityFromResestivity(double resestivity)
        {
            if (resestivity < WaterSaturatedShaleReference)
            {
                return 0;
            }
            if (resestivity < GoodReservoirLimit)
            {
                return 1;
            }
            return 2;
        }

        public double ReservoirQualityFunction<T>(
            T model, double x, double y) where T: IResistivityModel
        {
            return _ReservoirQualityFromResestivity(model.GetResistivity(x, y))
                   *ReservoirThicknessMultiplier;
        }

    }
}
