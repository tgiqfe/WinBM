using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBM
{
    internal class CalculateData
    {
        public static int ComputeInt(string text)
        {
            if (int.TryParse(text, out int tempInt))
            {
                return tempInt;
            }

            var dataTable = new System.Data.DataTable();
            var result = dataTable.Compute(text, "");
            switch (result)
            {
                case int i:
                    return i;
                case long l:
                    return l > int.MaxValue ? int.MaxValue : (int)l;
                case float f:
                    return (int)Math.Round(f, MidpointRounding.AwayFromZero);
                case double d:
                    return (int)Math.Round(d, MidpointRounding.AwayFromZero);
                case decimal c:
                    return (int)Math.Round(decimal.ToDouble(c), MidpointRounding.AwayFromZero);
                default:
                    return 0;
            }
        }

        public static double ComputeDouble(string text)
        {
            if (double.TryParse(text, out double tempDouble))
            {
                return tempDouble;
            }

            var dataTable = new System.Data.DataTable();
            var result = dataTable.Compute(text, "");
            switch (result)
            {
                case int i:
                    return i;
                case long l:
                    return l;
                case float f:
                    return f;
                case double d:
                    return d;
                case decimal c:
                    return decimal.ToDouble(c);
                default:
                    return 0;
            }
        }
    }
}
