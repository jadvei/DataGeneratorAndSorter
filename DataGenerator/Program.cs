using System.IO;

namespace DataGenerator
{
    public partial class Program
    {
        public static void Main(params string[] args)
        {
            var outputFileSize = double.Parse(args[0]);

            var generator = new DataGenerator();

            using (var stream = new FileStream(@".\output.txt", FileMode.Create))
            using (var streamWriter = new StreamWriter(stream))
            {
                generator.Generate(streamWriter, outputFileSize);
            }
        }
    }
}