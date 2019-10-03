using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ResistivitySimulator
{
    /// <summary>
    /// 
    /// </summary>
    public class EarthModelRealizationMultiLayer : EarthModelRealizationBaseClass
    {
        bool _consistentNotUsed;
        private IList<double> _upperBoundary;
        private IList<double> _lowerBoundary;

        private IList<double> _upperBoundary2;
        private IList<double> _lowerBoundary2;


        //measured in depth!
        private IList<double> _resistivityData;
        internal double _resistivityLayerBottom;
        internal double _resistivityLayerTop;
        internal double _resistivityOutsideLayer;


        public EarthModelRealizationMultiLayer(EarthModelManipulator parent, IList<double> up, IList<double> low,
            IList<double>  up2, IList<double> low2,
            double resistivityLayerBottom, double resistivityLayerTop, double resistivityOutsideLayer)
        {
            _parent = parent;
            _upperBoundary = up;
            _lowerBoundary = low;
            _upperBoundary2 = up2;
            _lowerBoundary2 = low2;
            _resistivityLayerBottom = resistivityLayerBottom;
            _resistivityLayerTop = resistivityLayerTop;
            _resistivityOutsideLayer = resistivityOutsideLayer;
            //TODO change to a more flexible way of instanciating value models.
            //_valueModel = new MetersAboveLowerBoundaryValueModel(this, 1.5);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="original"></param>
        public EarthModelRealizationMultiLayer(EarthModelManipulator parent, EarthModelRealizationMultiLayer original)
        {
            _parent = parent;
            _upperBoundary = original.UpperBoundary.ToArray<double>();
            _lowerBoundary = original.LowerBoundary.ToArray<double>();
            _upperBoundary2 = original.UpperBoundary2.ToArray();
            _lowerBoundary2 = original.LowerBoundary2.ToArray();
            _resistivityLayerBottom = original._resistivityLayerBottom;
            _resistivityLayerTop = original._resistivityLayerTop;
            _resistivityOutsideLayer = original._resistivityOutsideLayer;
            this.Vector = (Vector) original.Vector.Clone();
            //throw new NotSupportedException();
            //TODO set if it is supported
            //As of 08.02.2018 cannot copy like that
            //_valueModel = original.ValueModel;
        }

        //TODO add uncertainty to the layer resitivity

        /// <summary>
        /// Vector representation of a model
        /// </summary>
        public override Vector Vector
        {
            get
            {
                List<double> output = new List<double>(VectorSize);
                output.AddRange(UpperBoundary);
                output.AddRange(LowerBoundary);
                output.AddRange(UpperBoundary2);
                output.AddRange(LowerBoundary2);
                return new DenseVector(output.ToArray());
            }
            set
            {
                int ind = 0;
                for (int i = 0; i < UpperBoundary.Count; ++i, ++ind)
                {
                    UpperBoundary[i] = value[ind];
                }
                for (int i = 0; i < LowerBoundary.Count; ++i, ++ind)
                {
                    LowerBoundary[i] = value[ind];
                }
                for (int i = 0; i < UpperBoundary2.Count; ++i, ++ind)
                {
                    UpperBoundary2[i] = value[ind];
                }
                for (int i = 0; i < LowerBoundary2.Count; ++i, ++ind)
                {
                    LowerBoundary2[i] = value[ind];
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override int VectorSize
        {
            get
            {
                return LowerBoundary.Count + UpperBoundary.Count + LowerBoundary2.Count + UpperBoundary2.Count;
                //TODO consider rewriting
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public override double GetResistivity(double x, double z)
        {
            var index = GetGeobodyIndex(x,z);
            if (index == 0)
            {
                return _resistivityLayerBottom;
            }

            if (index == 1)
            {
                return _resistivityLayerTop;
            }

            if (index == -1)
            {
                return _resistivityOutsideLayer;
            }

            throw new IndexOutOfRangeException();

        }

        private double ConvertToPositive(double value)
        {
            return -value;
        }

        public override IList<IList<double>> GetBoundaryLists()
        {
            var boundaries = new List<IList<double>>();
            //we need to convert the order and signs
            boundaries.Add(LowerBoundary2.Select(x => -x).ToList());
            boundaries.Add(UpperBoundary2.Select(x => -x).ToList());
            boundaries.Add(LowerBoundary.Select(x=>-x).ToList());
            boundaries.Add(UpperBoundary.Select(x => -x).ToList());
            //
            //boundaries.Add(UpperBoundary);
            //boundaries.Add(LowerBoundary);
            //boundaries.Add(UpperBoundary2);
            //boundaries.Add(LowerBoundary2);
            return boundaries;
        }

        /// <summary>
        /// For now, the geobody can only be a layer
        /// Returns -1 if not found (e.g. outside reservoir)
        /// Returns -2 if not properly implemented in some derived class (which is very lazy...)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        public override int GetGeobodyIndex(double x, double z)
        {
            int indexPrev = _parent.IndexOfX(x);
            double len = _parent.IntervalLength(indexPrev);
            x -= _parent.IntervalStart(indexPrev);
            double lambda = x / len;
            //get value for mid point for bottom layer
            double zTopOfBottomLayer = UpperBoundary[indexPrev] * (1 - lambda) + UpperBoundary[indexPrev + 1] * lambda;
            double zBottomOfBottomLayer = LowerBoundary[indexPrev] * (1 - lambda) + LowerBoundary[indexPrev + 1] * lambda;
            //get top layer
            double zTopOfTopLayer = UpperBoundary2[indexPrev] * (1 - lambda) + UpperBoundary2[indexPrev + 1] * lambda;
            double zBottomOfTopLayer = LowerBoundary2[indexPrev] * (1 - lambda) + LowerBoundary2[indexPrev + 1] * lambda;

            if (z >= zTopOfBottomLayer && z <= zBottomOfBottomLayer) 
            {
                return 0;
            }
            if (z >= zTopOfTopLayer && z <= zBottomOfTopLayer)
            {
                return 1;
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public override Tuple<double, double> ThicknessAndDistanceAbove(double x, double z)
        {
            int indexPrev = _parent.IndexOfX(x);
            double len = _parent.IntervalLength(indexPrev);
            x -= _parent.IntervalStart(indexPrev);
            double lambda = x / len;
            //get value for mid point
            double zTop = UpperBoundary[indexPrev] * (1 - lambda) + UpperBoundary[indexPrev + 1] * lambda;
            double zBot = LowerBoundary[indexPrev] * (1 - lambda) + LowerBoundary[indexPrev + 1] * lambda;
            //get second layer
            double zTop2 = UpperBoundary2[indexPrev] * (1 - lambda) + UpperBoundary2[indexPrev + 1] * lambda;
            double zBot2 = LowerBoundary2[indexPrev] * (1 - lambda) + LowerBoundary2[indexPrev + 1] * lambda;

            //check if one layer only (degenerate model)
            if (zBot >= zTop2)
            {
                if (zTop <= z && z <= zBot2)
                {
                    return new Tuple<double, double>(zBot2 - zTop, zBot2 - z);
                }
                else
                {
                    return new Tuple<double, double>(0.0, 0.0);
                }
            }
            if (zTop <= z && z <= zBot)
            {
                return new Tuple<double, double>(zBot - zTop, zBot - z);
            }
            if (zTop2 <= z && z <= zBot2)
            {
                return new Tuple<double, double>(zBot2 - zTop2, zBot2 - z);
            }
            return new Tuple<double, double>(0.0, 0.0);
        }

        /// <summary>
        /// In this class, all layers are targets.
        /// </summary>
        /// <param name="geobodyIndex"></param>
        /// <returns></returns>
        public override bool IsTargetGeobody(int geobodyIndex)
        {
            return true;
        }

        private PointF[] GeneratePoints(IList<double> boundary)
        {
            var points = new PointF[boundary.Count];
            for (var index = 0; index < boundary.Count; index++)
            {
                var d = boundary[index];
                var x = this._parent.XPositions[index];
                points[index] = new PointF((float) x, (float) d);
            }

            return points;
        }

        internal List<double> XPositions
        {
            get => _parent.XPositions;
        }

        internal IList<double> UpperBoundary
        {
            get { return _upperBoundary; }
        }

        internal IList<double> LowerBoundary
        {
            get { return _lowerBoundary; }
        }

        internal IList<double> UpperBoundary2
        {
            get { return _upperBoundary2; }
        }

        internal IList<double> LowerBoundary2
        {
            get { return _lowerBoundary2; }
        }

        public void DrawBoundaries(Pen pen, Graphics gr)
        {
            
            gr.DrawLines(pen, GeneratePoints(LowerBoundary));
            gr.DrawLines(pen, GeneratePoints(LowerBoundary2));
            gr.DrawLines(pen, GeneratePoints(UpperBoundary));
            gr.DrawLines(pen, GeneratePoints(UpperBoundary2));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geobodyIndices"></param>
        public override void SetTargetGeobody(IList<int> geobodyIndices)
        {
            // Nothing to do, this class does not consider specific targets
        }

        /// <summary>
        /// Does the class allow considering specific geobodies as targets?
        /// Allows more control than just aiming for something with high thickness, porosity, etc.
        /// </summary>
        /// <returns></returns>
        public override bool ConsiderSpecificTargetGeobodies()
        {
            return false;
        }
    }

    public class MultiLayerVisualizer : IModelDrawer<EarthModelRealizationMultiLayer>
    {

        private void DrawLayer(Graphics gr, int transperancy, double resistivity, IList<double> xpos, IList<double> topZ,
            IList<double> bottomZ)
        {
            //FIXME do intersecting non-ordered boundaries
            var br = new SolidBrush(Color.FromArgb(transperancy, (int)resistivity, (int)resistivity, (int)resistivity));
            //draw quads
            for (int j = 1; j < xpos.Count; j++)
            {
                PointF[] points = new PointF[4];
                points[0] = new PointF((float)xpos[j - 1], (float)topZ[j - 1]);
                points[1] = new PointF((float)xpos[j - 1], (float)bottomZ[j - 1]);
                points[2] = new PointF((float)xpos[j], (float)bottomZ[j]);
                points[3] = new PointF((float)xpos[j], (float)topZ[j]);


                gr.FillPolygon(br, points);
            }
        }

        public void DrawOnGraphics(Graphics gr, EarthModelRealizationMultiLayer model, int transperancy)
        {
                        if (gr == null || model == null)
            {
                throw new ArgumentNullException();
            }

            //TODO add OWC visualization
            var xpos = model.XPositions;
            var i = 0;
            Brush br = new SolidBrush(Color.Black);
            var resistivities = new double[]{ 
                model._resistivityOutsideLayer, 
                model._resistivityLayerTop, 
                model._resistivityOutsideLayer, 
                model._resistivityLayerBottom, 
                model._resistivityOutsideLayer};
            //draw black ?
            var absoluteNegative = new double[xpos.Count];
            var absolutePositive = new double[xpos.Count];
            for (int j = 0; j < absolutePositive.Length; j++)
            {
                absolutePositive[j] = 100;
                absoluteNegative[j] = -100;
            }

            DrawLayer(gr, transperancy, resistivities[0], xpos, absoluteNegative, model.UpperBoundary);
            //top layer
            DrawLayer(gr, transperancy, resistivities[1], xpos, model.UpperBoundary,
                model.LowerBoundary);
            //mid layer
            DrawLayer(gr, transperancy, resistivities[2], xpos, model.LowerBoundary,
                model.UpperBoundary2);
            //bottom layer
            DrawLayer(gr, transperancy, resistivities[3], xpos, model.UpperBoundary2,
                model.LowerBoundary2);

            //over? burdon
            DrawLayer(gr, transperancy, resistivities[4], xpos, model.LowerBoundary2, absolutePositive);
        }
    }
}
