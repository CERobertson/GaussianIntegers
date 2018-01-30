using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace In_Extremis.Editor.Euler
{
    public class Constants
    {
        private static double e = 0.0;
        public static double E {
            get {
                var a = 0.0;
                foreach (var i in new Generator().Factorials())
                {
                    if (i > 0)
                    {
                        a += 1.0 / i;
                    }
                    else
                    {
                        return a;
                    }
                }
                return a;
            }
        }
    }
}
