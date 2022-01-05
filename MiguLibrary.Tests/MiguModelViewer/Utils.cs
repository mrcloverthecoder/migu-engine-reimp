using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;

namespace MiguModelViewer
{
    public class Utils
    {
        public static string DecimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        public static float ParseFloatEx(string str)
        {
            return float.Parse(str.Replace(".", DecimalSeparator));
        }
    }
}
