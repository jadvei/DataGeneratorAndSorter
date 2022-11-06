using DataGenerator;
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
        private const int fileSize = 600_000;
        private const string fileLocation = @"./Sorter";
        private int _maxUnsortedRows;
        private const string UnsortedFileExtension = ".unsorted";
        private const string SortedFileExtension = ".sorted";

        public void SortStream(StreamReader reader, Stream target)
        {
            var stopWatch = Stopwatch.StartNew();
            var unsortedfiles = SplitFile(reader);
            Console.WriteLine($"Files splited {stopWatch.Elapsed.TotalSeconds} sec");

            var sortedFiles = SortFiles(unsortedfiles);
            Console.WriteLine($"Files sorted {stopWatch.Elapsed.TotalSeconds} sec");

            var merger = new Merger();
            merger.MergeFiles(sortedFiles, target);
            Console.WriteLine($"Files merged {stopWatch.Elapsed.TotalSeconds} sec");
        }

        private IReadOnlyCollection<string> SplitFile(StreamReader sourceStream)
        {
            var filenames = new List<string>();
            var streams = InitStreams();
            string? line;

            while ((line = sourceStream.ReadLine()) != null)
            {
                var firstChar = char.ToUpper(line.First(x => char.IsLetter(x)));
                var writer = streams[firstChar];
                writer.GetStream().WriteLine(line);
                writer.CurrentStreamRows++;
                if (_maxUnsortedRows <= writer.CurrentStreamRows)
                { 
                    _maxUnsortedRows = writer.CurrentStreamRows;
                }
            }

            foreach (var stream in streams.Values)
            {
                filenames.AddRange(stream.FileNames);
                stream.Dispose();
            }

            return filenames;
        }

        private Dictionary<char, StreamWithCount> InitStreams()
        {
            var streams = new Dictionary<char, StreamWithCount>();
            for (char c = 'A'; c <= 'Z'; c++)
            {
                streams.Add(c, new StreamWithCount(c));
            }

            return streams;
        }

        private class StreamWithCount : IDisposable
        {
            private StreamWriter? streamWriter;

            public StreamWithCount(char streamChar)
            {
                StreamChar = streamChar;
            }

            public char StreamChar { get; init; }
            public int StreamNumber { get; private set; } = 0;
            public int CurrentStreamRows { get; set; } = 0;
            public List<string> FileNames { get; } = new List<string>();

            public StreamWriter GetStream()
            {
                if (streamWriter != null && CurrentStreamRows < fileSize)
                {
                    return streamWriter;
                }

                if (streamWriter != null)
                {
                    streamWriter.Dispose();
                }

                var fileName = GetFileName();
                var unsortedFile = File.Create(Path.Combine(fileLocation, fileName));
                FileNames.Add(fileName);
                streamWriter = new StreamWriter(unsortedFile);
                CurrentStreamRows = 0;
                return streamWriter;
            }

            public void Dispose()
            {
                streamWriter?.Dispose();
            }

            private string GetFileName()
            {
                return $"{StreamChar}.{StreamNumber++}{UnsortedFileExtension}";
            }
        }

        private IReadOnlyList<string> SortFiles(IReadOnlyCollection<string> unsortedFiles)
        {
            var sortedFiles = new ConcurrentBag<string>();
            double totalFiles = unsortedFiles.Count;

            Parallel.ForEach(unsortedFiles, new ParallelOptions() { MaxDegreeOfParallelism = 8 } , unsortedFile =>
            {
                var sortedFilename = unsortedFile.Replace(UnsortedFileExtension, SortedFileExtension);
                var unsortedFilePath = Path.Combine(fileLocation, unsortedFile);
                var sortedFilePath = Path.Combine(fileLocation, sortedFilename);
                SortFile(File.OpenRead(unsortedFilePath), File.OpenWrite(sortedFilePath));
                File.Delete(unsortedFilePath);
                sortedFiles.Add(sortedFilename);
            });

            return sortedFiles.ToList();
        }

        private void SortFile(Stream unsortedFile, Stream target)
        {
            using var streamReader = new StreamReader(unsortedFile);
            var unsortedRows = new DataStructure?[_maxUnsortedRows];
            var counter = 0;
            string? line;
            while((line = streamReader.ReadLine()) != null)
            {
                var splited = line.Split('.');
                unsortedRows[counter] = new DataStructure { Number = splited[0], Text = splited[1] };
                counter++;
            }

            Array.Sort(unsortedRows, RowComparer.CompareRows);

            using var streamWriter = new StreamWriter(target);
            foreach (var row in unsortedRows.Where(x => x != null))
            {
                streamWriter.WriteLine(row);
            }
        }
    }
}
