using System;
using System.Diagnostics;
using System.IO;

namespace DataSorter
{
    public class Program
    {
        public static void Main(params string[] args)
        {
            var sorter = new DataSorter();
            var settings = new Settings();
            var stopWatch = Stopwatch.StartNew();

            if (!Directory.Exists(settings.TempLocation))
            { 
                Directory.CreateDirectory(settings.TempLocation);
            }

            using (var source = new FileStream(@"", FileMode.Open))
            using (var target = new FileStream(@"", FileMode.Create))
            using (var sourceReader = new StreamReader(source))
            {
                sorter.SortStream(sourceReader, target, settings);
            }

            stopWatch.Stop();
            Console.WriteLine(stopWatch.Elapsed.TotalSeconds);
        }
    }
}