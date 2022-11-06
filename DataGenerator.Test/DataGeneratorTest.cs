using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace DataGenerator.Test
{
    public class DataGeneratorTest
    {
        public class DataStructure
        { 
            public int Number { get; set; }
            public string Text { get; set; }
        }

        [Fact]
        public void GeneratorTestObjectCreated()
        {
            var generator = new RandomDataFactory<DataStructure>();
            var item = generator.Generate();
            Assert.NotNull(item);
        }

        [Fact]
        public void GeneratorTestPropertiesPopulated()
        {
            var generator = new RandomDataFactory<DataStructure>();
            var item = generator.Generate();
            Assert.NotEqual(0, item.Number);
            Assert.NotEqual(string.Empty, item.Text);
        }

        [Fact]
        public void GeneratorTestDuplicate()
        {
            var generator = new DataGenerator();
            var allData = new List<DataStructure>();

            using (var stream = new MemoryStream())
            {
                var streamWriter = new StreamWriter(stream);
                generator.Generate(streamWriter, 1e-5);
                
                var reader = new StreamReader(stream);
                stream.Position = 0;
                var allLines = ReadAllLines(reader);

                foreach (var res in allLines)
                {
                    var splitResult = res.Split(".");
                    allData.Add(new DataStructure { Number = int.Parse(splitResult[0]), Text = splitResult[1] });
                }
            }

            var duplicates = allData.GroupBy(x => x.Text).Where(x => x.Count() > 1).ToList();

            Assert.True(allData.Any());
            Assert.True(duplicates.Any());
        }

        private IEnumerable<string> ReadAllLines(StreamReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}
