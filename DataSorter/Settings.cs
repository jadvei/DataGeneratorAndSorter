using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSorter
{
    public class Settings
    {
        /// <summary>
        /// Location for temp files
        /// </summary>
        public string TempLocation { get; set; } = @"./Temp";

        /// <summary>
        /// Extension of unsorted files
        /// </summary>
        public string UnsortedFileExtension { get; set; } = ".unsorted";

        /// <summary>
        /// Extension of sorted files
        /// </summary>
        public string SortedFileExtension { get; set; } = ".sorted";

        /// <summary>
        /// Max size of temp files in rows
        /// </summary>
        public int FileSize { get; set; } = 500_000;
        
        /// <summary>
        /// How many files will merge at once
        /// </summary>
        public int MergeChunkSize { get; set; } = 4;
    }
}
