using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ResistivitySimulator
{
    /// <summary>
    /// Geometry definition: reservoir top, then thicknesses for each layer at each x-value.
    /// Similar functionalities as EarthModelRealizationMultiLayer.
    /// </summary>
    public class EarthModelRealizationMultipleLayers : EarthModelRealizationBaseClass
    {
        private IList<double> _reservoirTop;        // surface describing the top of the reservoir
        private IList<IList<double>> _thicknesses;  // thickness for each node in each layer
        private IList<double> _resistivities;       // single value for each layer
        private double _OWC;                        // Depth of oil water contact
        private double _resistivityBelowOWC;


        // Target layers are the same for all realizations, so should be in some other class
        // that not yet exists (per 2018-02-05): probably a 'GeologicalScenario' or 'GeologicalConfiguration'
        private IList<int> _targetLayers;           // no other layers will be considered as targets
        // Does the class allow considering specific geobodies as targets?
        // Allows more control than just aiming for something with high thickness, porosity, etc.
        private bool _considerSpecificTargetGeobodies = true;


        /// <summary>
        /// Tuplet[0]: index of layer. Returns -1 on point being outside the reservoir boundary.
        /// Tuplet[1]: interpolation at x of the interface that represents the top of this layer
        /// Tuplet[2]: interpolation at x of the interface that represents the bottom of this layer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private Tuple<int, double, double> GetLayerInfo(double x, double z)
        {
            // DistanceAbove: currently not documented, seems to be a cut-off value

            // TODO: skip test of overprinting, too complex.
            // Assume a numerically valid model.

            int indexPrev = _parent.IndexOfX(x);
            double len = _parent.IntervalLength(indexPrev);
            x -= _parent.IntervalStart(indexPrev);
            double lambda = x / len;        // lambda in [0,1], for linear interpolation between two vertices
            //get value for mid point

            double zTopIndexPrev = _reservoirTop[indexPrev];
            double zTopIndex = _reservoirTop[indexPrev + 1];

            // Linear interpolation between two vertices
            double zTop = zTopIndexPrev * (1 - lambda) + zTopIndex * lambda;
            if (z > zTop)
            {
                // above reservoir top
                return new Tuple<int, double, double>(-1, double.NaN, double.NaN);
            }

            // Prepare for looping
            double zBottomIndexPrev = zTopIndexPrev;
            double zBottomIndex = zTopIndex;


            // Searches from top to bottom for correct layer
            for (int layerNum = 0; layerNum < _thicknesses.Count; layerNum++)
            {
                // All layers above have already been checked
                IList<double> thickness = _thicknesses[layerNum];

                // The bottom of the current layer
                zBottomIndexPrev -= _thicknesses[layerNum][indexPrev];
                zBottomIndex -= _thicknesses[layerNum][indexPrev + 1];

                // linear interpolation between two vertices
                double zBottom = zBottomIndexPrev * (1 - lambda) + zBottomIndex * lambda;

                if (z > zBottom)
                {
                    // The layer has the same index as its top interface
                    return new Tuple<int, double, double>(layerNum, zTop, zBottom);
                }

                zTop = zBottom;
            }

            // Below the bottom boundary of the model
            return new Tuple<int, double, double>(-1, double.NaN, double.NaN);

        }



        public EarthModelRealizationMultipleLayers(EarthModelManipulator parent, IList<double> reservoirTop,
            IList<IList<double>> thicknesses, IList<double> resistivities,
            double OWC, double resistivityBelowOWC,
            IList<int> targetLayers, bool considerSpecificTargetGeobody)
        {
            if (parent == null || reservoirTop == null || thicknesses == null || resistivities == null)
            {
                throw new ArgumentNullException();
            }

            if (thicknesses.Count != resistivities.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            _parent = parent;
            _reservoirTop = reservoirTop;
            _thicknesses = thicknesses;
            _resistivities = resistivities;
            _OWC = OWC;
            _resistivityBelowOWC = resistivityBelowOWC;
            _targetLayers = targetLayers;
            //_valueModel = new MetersAboveLowerBoundaryValueModel(0.3);
            _considerSpecificTargetGeobodies = considerSpecificTargetGeobody;
            //_valueModel = new OWCPlusOtherValueModel(this, 0.3);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="original"></param>
        public EarthModelRealizationMultipleLayers(EarthModelManipulator parent, EarthModelRealizationMultipleLayers original)
        {
            if (parent == null || original == null)
            {
                throw new ArgumentNullException();
            }

            _parent = parent;
            _reservoirTop = original._reservoirTop.ToArray<double>();

            for (int i =0; i<original._thicknesses.Count; i++)
            {
                _thicknesses.Add(original._thicknesses[i].ToArray<double>());
            }

            _resistivities = original._resistivities.ToArray<double>();
            _OWC = original._OWC;
            _resistivityBelowOWC = original._resistivityBelowOWC;
            _targetLayers = original._targetLayers.ToArray<int>();
            throw new NotSupportedException();
            //As of 08.02.2018 cannot copy like that
            //_valueModel = original.ValueModel;
        }

        /// <summary>
        /// Vector representation of a model
        /// </summary>
        public override Vector Vector
        {
            // TODO Insert top + thicknesses + resistivities, in the same order
            // return _reservoirTop.Count + _thicknesses.Count * _thicknesses [0].Count + _resistivities.Count;
            get
            {
                List<double> output = new List<double>(VectorSize);
                output.AddRange(_reservoirTop);
                foreach (IList<double> thickness in _thicknesses)
                {
                    output.AddRange(thickness);
                }
                output.AddRange(_resistivities);
                output.Add(_OWC);
                return new DenseVector(output.ToArray());
            }
            set
            {
                int ind = 0;
                for (int i = 0; i < _reservoirTop.Count; ++i, ++ind)
                {
                    _reservoirTop[i] = value[ind];
                }
                foreach (IList<double> thickness in _thicknesses)
                {
                    for (int i = 0; i < thickness.Count; ++i, ++ind)
                    {
                        if (value[ind] < 0.0)
                        {
                            thickness[i] = 0.0;         // 710 trick: set negative thickness to zero
                            // throw new Exception("Only shows that negative thickness can happen ...");
                        }
                        else
                        {
                            thickness[i] = value[ind];
                        }
                    }
                }
                for (int i = 0; i < _resistivities.Count; ++i, ++ind)
                {
                    _resistivities[i] = value[ind];
                }

                _OWC = value[ind]; ind++;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override int VectorSize
        {
            get
            {
                return _reservoirTop.Count + _thicknesses.Count * _thicknesses[0].Count + _resistivities.Count + 1;
                // The last "1" is for OWC
                //TODO consider rewriting
            }
        }

        public IList<double> ReservoirTop
        {
            get { return _reservoirTop; }
        }

        public IList<IList<double>> Thicknesses
        {
            get { return _thicknesses; }
        }

        public double OWC
        {
            get { return _OWC; }
            set { _OWC = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public override Tuple<double, double> ThicknessAndDistanceAbove(double x, double z)
        {
            // Version with taking OWC into account when calculating thicknesses.
            // A basic implementation of handling OWC: just assume that everything below OWC is not reservoir
            // Assumes that OWC is a horizontal line

            // DistanceAbove: currently not documented, seems to be a cut-off value

            Tuple<int, double, double> layerInfo = GetLayerInfo(x, z);
            int layerIndex = layerInfo.Item1;       // The index of the layer containing z
            double topOfLayer = layerInfo.Item2;
            double bottomOfLayer = layerInfo.Item3;

            if (layerIndex < 0 || layerIndex >= _thicknesses.Count)
            {
                // Above the top or below the bottom of the modelled reservoir
                return new Tuple<double, double>(0.0, 0.0);
                // If a zero tuple is returned, it means that the optimization does not favour this layer
            }


            double layerThickness = topOfLayer - bottomOfLayer;
            double heightAboveBottomOfLayer = z - bottomOfLayer;
            double heightAboveOWC = z - OWC;

            if (heightAboveOWC <= 0.0)  // already know that z is above the bottom of the layer, so no need to check
            {
                // Below OWC, not to be favoured.
                // But note that a dip into the water may still be preferrable to drilling in shale (or vice versa)
                // (should be part of the future more complex objectives)
                return new Tuple<double, double>(0.0, 0.0);
            }

            // The bottom boundary of the non-waterfilled part of the layer
            double shallowest_of_BoundaryLayer_or_OWC = Math.Max(OWC, bottomOfLayer);

            // The thickness above the OWC or the bottom of the layer, namely the non-waterfilled part of the layer
            // Both values are known to be positive
            double thicknessAboveShallowest_BoundaryLayer_OR_OWC = topOfLayer - shallowest_of_BoundaryLayer_or_OWC;
            double heightAboveShallowest_of_BoundaryLayer_or_OWC = z - shallowest_of_BoundaryLayer_or_OWC;

            // [Thickness, height of z above the bottom of the layer]
            // Could ne NTG, resistivity, etc (to be implemented in MetersAboveLowerBoundaryValueModel.ComputeReservoirValue())
            // OLD: return new Tuple<double, double>(layerThickness, heightAboveBottomOfLayer);

            // OBS: a bug or wanted behaviour?
            // Must return negative values to place the well close to the top of the layer
            // If positive values are returned, the well is placed close to the bottom of the layer instead.
            // But is is inconsistent: thicknesses should always be positive values, negative values are counterintuitive
            // and may lead to problems later on.
            return new Tuple<double, double>(-thicknessAboveShallowest_BoundaryLayer_OR_OWC, -heightAboveShallowest_of_BoundaryLayer_or_OWC);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public override double GetResistivity(double x, double z)
        {
            Tuple<int, double, double> layerInfo = GetLayerInfo(x, z);
            int layerIndex = layerInfo.Item1;
            double topOfLayer = layerInfo.Item2;
            double bottomOfLayer = layerInfo.Item3;

            if (z < _OWC)
            {
                // Below OWC
                return _resistivityBelowOWC;
            }

            if (layerIndex < 0)
            {
                // Outside the model
                // For now, return some low resistivity (background shale)
                return 15.0;
            }
            else
            {
                return _resistivities[layerIndex];
            }
        }

        public override IList<IList<double>> GetBoundaryLists()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// For now, a geobody is a layer
        /// Returns -1 if not found (e.g. outside reservoir)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        public override int GetGeobodyIndex(double x, double z)
        {
            Tuple<int, double, double> layerInfo = GetLayerInfo(x, z);
            int layerIndex = layerInfo.Item1;
            return layerIndex;
        }

        /// <summary>
        /// In this class, all layers are potential targets.
        /// </summary>
        /// <param name="geobodyIndex">Not considered in this class</param>
        /// <returns></returns>
        public override bool IsTargetGeobody(int geobodyIndex)
        {
            return _targetLayers.Contains(geobodyIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geobodyIndices"></param>
        public override void SetTargetGeobody(IList<int> geobodyIndices)
        {
            if (geobodyIndices != null)
            {
                _targetLayers = geobodyIndices.ToArray<int>();
            }
        }

        public override bool ConsiderSpecificTargetGeobodies()
        {
            // SetTargetGeobody etc should not be part of every realization, but probably the EarthModelManipulator
            return _considerSpecificTargetGeobodies;
        }

    }
}
