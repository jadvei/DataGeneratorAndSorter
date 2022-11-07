using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSorter
{
    public class Splitter
    {
        private int _maxRowsInFile;
        private Settings _settings;

        public Splitter(Settings settings)
        {
            this._settings = settings;
        }

        public (IReadOnlyCollection<string> FileNames, int MaxRowsInFile) SplitFile(StreamReader sourceStream)
        {
            var filenames = new List<string>();
            var writer = new StreamWithCount(_settings);
            string? line;

            while ((line = sourceStream.ReadLine()) != null)
            {
                writer.GetStream().WriteLine(line);
                writer.CurrentStreamRows++;

                if (_maxRowsInFile <= writer.CurrentStreamRows)
                {
                    _maxRowsInFile = writer.CurrentStreamRows;
                }
            }

            filenames.AddRange(writer.FileNames);
            writer.Dispose();

            return (filenames, _maxRowsInFile);
        }

        private class StreamWithCount : IDisposable
        {
            private StreamWriter streamWriter;
            private Settings _settings;

            public int StreamNumber { get; private set; } = 0;
            public int CurrentStreamRows { get; set; } = 0;
            public List<string> FileNames { get; } = new List<string>();

            public StreamWithCount(Settings settings)
            {
                this._settings = settings;
                streamWriter = CreateNewStream();
            }

            public StreamWriter GetStream()
            {
                if (CurrentStreamRows < _settings.FileSize)
                {
                    return streamWriter;
                }

                streamWriter.Dispose();
                streamWriter = CreateNewStream();
                CurrentStreamRows = 0;
                return streamWriter;
            }

            private StreamWriter CreateNewStream()
            {
                var fileName = GetFileName();
                var unsortedFile = File.Create(Path.Combine(_settings.TempLocation, fileName));
                FileNames.Add(fileName);
                return new StreamWriter(unsortedFile);
            }

            public void Dispose()
            {
                streamWriter?.Dispose();
            }

            private string GetFileName()
            {
                return $"{StreamNumber++}{_settings.UnsortedFileExtension}";
            }
        }
    }
}
