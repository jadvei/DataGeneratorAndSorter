using DataGenerator;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataSorter
{
    internal class Merger
    {
        private const string TempFileExtension = ".tmp";
        private const string UnsortedFileExtension = ".unsorted";
        private const string SortedFileExtension = ".sorted";
        private const string fileLocation = @"./Sorter";
        private object lockObj = new object();
        public void MergeFiles(IReadOnlyList<string> sortedFiles, Stream target)
        {
            var done = false;
            while (!done)
            {
                var runSize = 10;
                var finalRun = sortedFiles.Count <= runSize;

                if (finalRun)
                {
                    Merge(sortedFiles, target);
                    return;
                }

                var runs = sortedFiles.Chunk(runSize);
                var chunkCounter = sortedFiles.Count();
                Parallel.ForEach(runs, new ParallelOptions { MaxDegreeOfParallelism = 8 } , files =>
                {
                    string outputFilename;
                    lock (lockObj)
                    {
                        outputFilename = $"{++chunkCounter}{SortedFileExtension}{TempFileExtension}";
                    }

                    if (files.Length == 1)
                    {
                        File.Move(GetFullPath(files.First()), GetFullPath(outputFilename.Replace(TempFileExtension, string.Empty)));
                        return;
                    }

                    var outputStream = File.OpenWrite(GetFullPath(outputFilename));
                    Merge(files, outputStream);
                    File.Move(GetFullPath(outputFilename), GetFullPath(outputFilename.Replace(TempFileExtension, string.Empty)), true);
                });

                sortedFiles = Directory.GetFiles(fileLocation, $"*{SortedFileExtension}")
                    .OrderBy(x =>
                    {
                        var filename = Path.GetFileNameWithoutExtension(x);
                        return int.Parse(filename);
                    })
                    .ToArray();

                if (sortedFiles.Count > 1)
                {
                    continue;
                }

                done = true;
            }
        }

        private async void Merge(IReadOnlyList<string> filesToMerge, Stream outputStream)
        {
            var (streamReaders, rows) = InitializeStreamReaders(filesToMerge);
            var finishedStreamReaders = new List<int>(streamReaders.Length);
            var done = false;
            using var outputWriter = new StreamWriter(outputStream);

            while (!done)
            {
                rows.Sort((row1, row2) => RowComparer.CompareRows(row1.Value, row2.Value));
                var valueToWrite = rows[0].Value;
                var streamReaderIndex = rows[0].StreamReader;
                outputWriter.WriteLine(valueToWrite.ToString());

                if (streamReaders[streamReaderIndex].EndOfStream)
                {
                    var indexToRemove = rows.FindIndex(x => x.StreamReader == streamReaderIndex);
                    rows.RemoveAt(indexToRemove);
                    finishedStreamReaders.Add(streamReaderIndex);
                    done = finishedStreamReaders.Count == streamReaders.Length;
                    continue;
                }

                var value = await streamReaders[streamReaderIndex].ReadLineAsync();
                if (value == null)
                {
                    continue;
                }

                var splited = value.Split('.');
                rows[0] = new Row { Value = new DataStructure { Number = splited[0], Text = splited[1] }, StreamReader = streamReaderIndex };
            }

            CleanupRun(streamReaders, filesToMerge);
        }

        /// <summary>
        /// Creates a StreamReader for each sorted sourceStream.
        /// Reads one line per StreamReader to initialize the rows list.
        /// </summary>
        /// <param name="sortedFiles"></param>
        /// <returns></returns>
        private (StreamReader[] StreamReaders, List<Row> rows) InitializeStreamReaders(IReadOnlyList<string> sortedFiles)
        {
            var streamReaders = new StreamReader[sortedFiles.Count];
            var rows = new ConcurrentBag<Row>();
            Parallel.For(0, sortedFiles.Count, i =>
            {
                var sortedFilePath = GetFullPath(sortedFiles[i]);
                var sortedFileStream = File.OpenRead(sortedFilePath);
                streamReaders[i] = new StreamReader(sortedFileStream);
                var value = streamReaders[i].ReadLine();

                if (value == null)
                {
                    return;
                }

                var splited = value.Split('.');
                var row = new Row
                {
                    Value = new DataStructure { Number = splited[0], Text = splited[1] },
                    StreamReader = i
                };
                rows.Add(row);
            });

            return (streamReaders, rows.ToList());
        }

        /// <summary>
        /// Disposes all StreamReaders
        /// Renames old files to a temporary name and then deletes them.
        /// Reason for renaming first is that large files can take quite some time to remove
        /// and the .Delete call returns immediately.
        /// </summary>
        /// <param name="streamReaders"></param>
        /// <param name="filesToMerge"></param>
        private void CleanupRun(StreamReader[] streamReaders, IReadOnlyList<string> filesToMerge)
        {
            for (var i = 0; i < streamReaders.Length; i++)
            {
                streamReaders[i].Dispose();
                var temporaryFilename = $"{filesToMerge[i]}.removal";
                File.Move(GetFullPath(filesToMerge[i]), GetFullPath(temporaryFilename));
                File.Delete(GetFullPath(temporaryFilename));
            }
        }

        internal readonly struct Row
        {
            public DataStructure Value { get; init; }
            public int StreamReader { get; init; }
        }

        private string GetFullPath(string filename)
        {
            return Path.Combine(fileLocation, Path.GetFileName(filename));
        }
    }
}
