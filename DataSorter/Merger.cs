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
        private Settings _settings;
        private const string TempFileExtension = ".tmp";

        public Merger(Settings settings)
        {
            this._settings = settings;
        }

        public void MergeFiles(IReadOnlyList<string> sortedFiles, Stream target)
        {
            var chunkCounter = sortedFiles.Count();
            var done = false;
            while (!done)
            {
                var runSize = _settings.MergeChunkSize;
                var finalRun = sortedFiles.Count <= runSize;

                if (finalRun)
                {
                    Merge(sortedFiles, target);
                    return;
                }

                var runs = sortedFiles.Chunk(runSize).Select(x => new { Files = x, OutputFileName = $"{++chunkCounter}{_settings.SortedFileExtension}{TempFileExtension}" });
                
                Parallel.ForEach(runs, new ParallelOptions { MaxDegreeOfParallelism = 8 }, chunk =>
                {
                    var outputFilename = chunk.OutputFileName;
                    if (chunk.Files.Length == 1)
                    {
                        File.Move(GetFullPath(chunk.Files.First()), GetFullPath(outputFilename.Replace(TempFileExtension, string.Empty)));
                        return;
                    }

                    var outputStream = File.OpenWrite(GetFullPath(outputFilename));
                    Merge(chunk.Files, outputStream);
                    File.Move(GetFullPath(outputFilename), GetFullPath(outputFilename.Replace(TempFileExtension, string.Empty)), true);
                });

                sortedFiles = Directory.GetFiles(_settings.TempLocation, $"*{_settings.SortedFileExtension}")
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

        private void Merge(IReadOnlyList<string> filesToMerge, Stream outputStream)
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

                var value = streamReaders[streamReaderIndex].ReadLine();
                if (value == null)
                {
                    break;
                }

                rows[0] = new Row { Value = DataStructure.FromString(value), StreamReader = streamReaderIndex };
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

            for (var i = 0; i < sortedFiles.Count; i++)
            {
                var sortedFilePath = GetFullPath(sortedFiles[i]);
                var sortedFileStream = File.OpenRead(sortedFilePath);
                streamReaders[i] = new StreamReader(sortedFileStream);
                var value = streamReaders[i].ReadLine();

                if (value == null)
                {
                    continue;
                }

                var row = new Row
                {
                    Value = DataStructure.FromString(value),
                    StreamReader = i
                };
                rows.Add(row);
            }

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
            return Path.Combine(_settings.TempLocation, Path.GetFileName(filename));
        }
    }
}
