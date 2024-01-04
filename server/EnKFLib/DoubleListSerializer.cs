using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EnKFLib
{
    public class DoubleListSerializer
    {
        public static IList<double> Serialize(Object o)
        {
            Type t = o.GetType();
            System.Reflection.FieldInfo[] fis = t.GetFields();
            foreach (System.Reflection.FieldInfo fi in fis)
            {
                if (fi.FieldType == typeof(double))
                {
                    double d = (double) fi.GetValue(o);
                }
                if (fi.FieldType == typeof(IList<double>))
                {
                    IList<double> list = (IList<double>)fi.GetValue(o);
                }
            }
            throw new NotImplementedException();
        }
    }
}
