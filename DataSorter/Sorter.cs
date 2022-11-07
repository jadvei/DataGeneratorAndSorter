using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSorter
{
    public class Sorter
    {
        private Settings _settings;
        private readonly int maxRowsInFile;
        private ArrayPool<DataStructure?> arrayPool;

        public Sorter(Settings settings, int maxRowsInFile)
        {
            this._settings = settings;
            this.maxRowsInFile = maxRowsInFile;
            arrayPool = ArrayPool<DataStructure?>.Create(maxRowsInFile, 100);
        }

        public IReadOnlyList<string> SortFiles(IReadOnlyCollection<string> unsortedFiles)
        {
            var sortedFiles = new ConcurrentBag<string>();
            double totalFiles = unsortedFiles.Count;

            Parallel.ForEach(unsortedFiles, new ParallelOptions() { MaxDegreeOfParallelism = 8 }, unsortedFile =>
            {
                var sortedFilename = unsortedFile.Replace(_settings.UnsortedFileExtension, _settings.SortedFileExtension);
                var unsortedFilePath = Path.Combine(_settings.TempLocation, unsortedFile);
                var sortedFilePath = Path.Combine(_settings.TempLocation, sortedFilename);
                SortFile(File.OpenRead(unsortedFilePath), File.OpenWrite(sortedFilePath));
                File.Delete(unsortedFilePath);
                sortedFiles.Add(sortedFilename);
            });

            return sortedFiles.ToList();
        }

        private void SortFile(Stream unsortedFile, Stream target)
        {
            using var streamReader = new StreamReader(unsortedFile);
            var unsortedRows = arrayPool.Rent(maxRowsInFile);
            var counter = 0;
            string? line;
            while ((line = streamReader.ReadLine()) != null)
            {
                unsortedRows[counter] = DataStructure.FromString(line);
                counter++;
            }

            Array.Sort(unsortedRows, RowComparer.CompareRows);

            using var streamWriter = new StreamWriter(target);
            foreach (var row in unsortedRows.Where(x => x != null))
            {
                streamWriter.WriteLine(row);
            }

            arrayPool.Return(unsortedRows, true);
        }
    }
}
