using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Variogram
{
    public interface ISimpleModel
    {

    }

    public class MoreSpecificModel : ISimpleModel
    {
        public double TestField;
    }

    public interface IObjectiveFunction<T>
    {
        double get();
        double get(T spec);
    }

    public class SimpleObjective<T> : IObjectiveFunction<T> where T : ISimpleModel
    {
        public double get()
        {
            throw new NotImplementedException();
        }

        public double get(T spec)
        {
            throw new NotImplementedException();
        }
    }

    public class HitroviebaniObjective<T> : IObjectiveFunction<T> where T : MoreSpecificModel
    {
        public double get()
        {
            throw new NotImplementedException();
        }

        double IObjectiveFunction<T>.get(T spec)
        {
            return get(spec);
        }

        double get(T spec)
        {
            return spec.TestField;
        }
    }

    //public class Opt<T>

    class OutTest
    {

        public delegate double Objective<in T>(T model);

        public double SimpleObjectiveDeligate(ISimpleModel model)
        {
            return 0.0;
        }

        public double ComplexObjectiveDelegate(MoreSpecificModel model)
        {
            return 0.0;
        }


        public OutTest()
        {
            Objective<MoreSpecificModel> del1 = SimpleObjectiveDeligate;
            Objective<MoreSpecificModel> del2 = ComplexObjectiveDelegate;

//            Objective<ISimpleModel> del11 = SimpleObjectiveDeligate;
//            Objective<ISimpleModel> del22 = ComplexObjectiveDelegate;


            IObjectiveFunction<MoreSpecificModel> obj1 = new SimpleObjective<MoreSpecificModel>();
            IObjectiveFunction<MoreSpecificModel> obj2 = new HitroviebaniObjective<MoreSpecificModel>();
            //obj2 = obj1;
        }
    }
}
