using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ResistivitySimulator
{
    //OLD!!!
    public class DataGenerator
    {
        private ResistivityModel model;
        private Vector trueRealization;

        public DataGenerator(Vector trueRealization, IResistivityModel eModel, double instrumentSize)
        {
            this.trueRealization = trueRealization;
            model = new ResistivityModel(eModel, instrumentSize);
        }

        public Point2DDouble Position
        {
            get 
            { 
                return model.Position; 
            }
            set 
            { 
                model.Position = value; 
            }
        }

        public Vector GetData(Point2DDouble position)
        {
            model.Position = position;
            return model.ModelToData(trueRealization);
        }


        /// <summary>
        /// used for visualization
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public double GetResistivity(double x, double y)
        {
            
            return model.GetModelResistivity(trueRealization, x, y);
        }

    }
}
