using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace WinBM
{
    internal class CalculateData
    {
        public static int ComputeInt(string value)
        {
            if (int.TryParse(value, out int tempInt))
            {
                return tempInt;
            }

            var dataTable = new DataTable();
            var result = decimal.ToDouble((decimal)dataTable.Compute(value, ""));

            return (int)Math.Round(result, MidpointRounding.AwayFromZero);
        }

        public static double ComputeDouble(string value)
        {
            if(double.TryParse(value, out double tempDouble))
            {
                return tempDouble;
            }

            var dataTable = new DataTable();
            return decimal.ToDouble((decimal)dataTable.Compute(value, ""));
        }
    }
}
