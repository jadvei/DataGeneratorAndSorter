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
            var stopWatch = Stopwatch.StartNew();

            using (var source = new FileStream(@"C:\Users\Timer\source\repos\DataGenerator\DataGenerator\bin\Debug\net6.0\output.txt", FileMode.Open))
            using (var target = new FileStream(@".\sortedResult.txt", FileMode.Create))
            using (var sourceReader = new StreamReader(source))
            {
                sorter.SortStream(sourceReader, target);
            }

            stopWatch.Stop();
            Console.WriteLine(stopWatch.Elapsed.TotalSeconds);
        }
    }
}