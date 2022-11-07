using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataSorter
{
    public class DataSorter
    {
        public void SortStream(StreamReader reader, Stream target, Settings settings)
        {
            var stopWatch = Stopwatch.StartNew();
            var splitter = new Splitter(settings);
            var (unsortedfiles, maxRowsInFile) = splitter.SplitFile(reader);
            Console.WriteLine($"Files splited {stopWatch.Elapsed.TotalSeconds} sec");

            var sorter = new Sorter(settings, maxRowsInFile);
            var sortedFiles = sorter.SortFiles(unsortedfiles);
            Console.WriteLine($"Files sorted {stopWatch.Elapsed.TotalSeconds} sec");

            var merger = new Merger(settings);
            merger.MergeFiles(sortedFiles, target);
            Console.WriteLine($"Files merged {stopWatch.Elapsed.TotalSeconds} sec");
        }
    }
}
