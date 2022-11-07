using System.IO;

namespace DataGenerator
{
    public partial class Program
    {
        public static void Main(params string[] args)
        {
            var outputFileSize = 10;

            var generator = new DataGenerator();

            using (var stream = new FileStream(@".\output10.txt", FileMode.Create))
            using (var streamWriter = new StreamWriter(stream))
            {
                generator.Generate(streamWriter, outputFileSize);
            }
        }
    }
}