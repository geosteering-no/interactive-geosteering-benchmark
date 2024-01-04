using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrajectoryInterfaces;
using TrajectoryOptimization;


namespace ResistivitySimulator
{
    public class ResistivityMeasurement : EnKFLib.IData<IContinousState, ResistivityData2DFull>
    {
        private IContinousState point = new ContinousState();
        private ResistivityData2DFull data = new ResistivityData2DFull();
        private ResistivityData2DFull error = new ResistivityData2DFull();

        public ResistivityMeasurement(IContinousState p, ResistivityData2DFull measurement, ResistivityData2DFull error)
        {
            this.point = p;
            this.data = measurement;
            this.error = error;
        }

        public IContinousState Parameter
        {
            get {
                return point;
            }
        }



        public ResistivityData2DFull Value
        {
            get {
                return data;
            }
        }

        public ResistivityData2DFull ErrorVarience
        {
            get {
                return error;
            }
        }
    }
}
