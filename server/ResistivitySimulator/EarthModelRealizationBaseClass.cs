using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace ResistivitySimulator
{
    /// <summary>
    /// Abstract base class for any earth model realization
    /// </summary>
    public abstract class EarthModelRealizationBaseClass : IEarthModelRealization
    {

        protected EarthModelManipulator _parent;
        //protected IValueModel _valueModel;

        /// <summary>
        /// Vector representation of a model
        /// </summary>
        public abstract Vector Vector { get; set; }

        /// <summary>
        /// Vector representation of a model
        /// </summary>
        public abstract int VectorSize { get; }

//        /// <summary>
//        /// 
//        /// </summary>
//        public IValueModel ValueModel
//        {
//            get
//            {
//                return _valueModel;
//            }
//            set
//            {
//                if (value != null)
//                {
//                    _valueModel = value;
//                }
//            }
//        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public virtual double GetResistivity(Point2DDouble position)
        {
            return GetResistivity(position.X, position.Y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public abstract double GetResistivity(double x, double z);

        public IList<double> GetXs()
        {
            return _parent.XPositions;
        }

        public abstract IList<IList<double>> GetBoundaryLists();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public abstract Tuple<double, double> ThicknessAndDistanceAbove(double x, double z);

        /// <summary>
        /// For now, the geobody can only be a layer
        /// Returns -1 if not found (e.g. outside reservoir)
        /// Returns -2 if not properly implemented in some derived class (which is very lazy...)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        public abstract int GetGeobodyIndex(double x, double z);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geobodyIndices"></param>
        public abstract void SetTargetGeobody(IList<int> geobodyIndices);

        /// <summary>
        /// Is this a target geobody in the reservoir?
        /// Allows more control than just aiming for something with high thickness, porosity, etc.
        /// </summary>
        /// <param name="geobodyIndex"></param>
        /// <returns></returns>
        public abstract bool IsTargetGeobody(int geobodyIndex);

        /// <summary>
        /// Does the class allow considering specific geobodies as targets?
        /// Allows more control than just aiming for something with high thickness, porosity, etc.
        /// </summary>
        /// <returns></returns>
        public abstract bool ConsiderSpecificTargetGeobodies();
    }
}
