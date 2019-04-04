using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team6.Engine.Misc
{
    public static class EnumExt
    {

        public static IEnumerable<T> GetValues<T>() 
        {
            return Enum.GetValues(typeof(T)).OfType<T>();
        }
    }
}
