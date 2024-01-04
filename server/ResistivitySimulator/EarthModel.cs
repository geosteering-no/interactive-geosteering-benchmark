using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ResistivitySimulator
{
    public class EarthModel : IResistivityModel
    {
        //faultY0 is always set to 0
        private static MathNet.Numerics.LinearAlgebra.VectorBuilder<double> _vBuild =
            MathNet.Numerics.LinearAlgebra.Vector<double>.Build;
        double faultX0 = 100;
        double faultHeave, faultThrow;
        double layerThickness;
        double resistivityInLayer;
        double resistivityOutsideLayer;
        double MAX_X = 300;


        public EarthModel(double layerH, double resistIn, double resistOut, double faultPos, double faultHeave, double faultThrow)
        {
            this.layerThickness = layerH;
            this.resistivityInLayer = resistIn;
            this.resistivityOutsideLayer = resistOut;
            this.faultX0 = faultPos;
            this.faultThrow = faultThrow;
            this.faultHeave = faultHeave;
        }


        public EarthModel(double layerH, double resistIn, double resistOut)
        {
            this.layerThickness = layerH;
            this.resistivityInLayer = resistIn;
            this.resistivityOutsideLayer = resistOut;
        }
        
        /// <summary>
        /// Model vector has length 3 and consists of:
        /// x0 - position of the fault
        /// heave - positive number for = x1-x0
        /// throw - positive when directed upwards = y1-yo
        /// </summary>
        public Vector Vector
        {
            get {
                double[] x = new double[2];
                x[0] = faultX0;
                x[1] = faultHeave;
                x[2] = faultThrow;
                return (Vector)_vBuild.DenseOfArray(x);
            }
            set { 
                faultX0 = value[0];
                faultHeave = value[1];
                faultThrow = value[2];
            }
        }


        /// <summary>
        /// This returns resistivity in the current point
        /// </summary>
        /// <param name="x">directed right</param>
        /// <param name="y">directed up</param>
        /// <returns></returns>
        public double GetResistivity(double x, double y)
        {
            //determine if we are on the left of the line
            //find direwction to the point
            double dx = x - faultX0;
            double dy = y;
        
            double crossDFaultZ = dx * faultThrow - dy * faultHeave;
            
            bool toTheRight = crossDFaultZ > 0;
            //TODO check the sign

            if (toTheRight)
            {
                //height edjustment
                y -= faultThrow;
                
                //x adjustment
                x -= faultHeave;

                //TODO check this
            }
            //else
            //{
            //    bool notTrue = false;
            //}
            if (y >= 0 && y <= layerThickness)
            {
                return resistivityInLayer;
            }
            else
            {
                return resistivityOutsideLayer;
            }
        }

        public void DrawOnGraphics(Graphics gr, int transperancy)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public double GetResistivity(Point2DDouble position)
        {
            //Vector = modelVector;
            return GetResistivity(position.X, position.Y);
        }

        public IList<Tuple<double,double>> GetXYValuesAsPairs()
        {
            IList<Tuple<double,double>> list = new List<Tuple<double,double>>();
            list.Add(new Tuple<double,double>(0,0));
            list.Add(new Tuple<double,double>(faultX0,0));
            list.Add(new Tuple<double,double>(faultX0 + faultHeave,faultThrow));
            list.Add(new Tuple<double,double>(MAX_X,faultThrow));

            return list;
        }




        public int VectorSize
        {
            get {
                return 3;
            }
        }
    }
}
