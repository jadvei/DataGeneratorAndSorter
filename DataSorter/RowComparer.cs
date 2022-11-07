using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSorter
{
    public static class RowComparer
    {
        public static int CompareRows(DataStructure? row1, DataStructure? row2)
        {
            if (row1 == null && row2 == null)
            {
                return 0;
            }
            if (row1 == null)
            {
                return 1;
            }
            if (row2 == null)
            {
                return -1;
            }

            var compareResult = string.CompareOrdinal(row1.Value.Text, row2.Value.Text);

            if (compareResult != 0)
            {
                return compareResult;
            }
            else
            {
                return row1.Value.GetNumber().CompareTo(row2.Value.GetNumber());
            }
        }
    }
}
