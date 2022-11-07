using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataGenerator
{
    public class DataGenerator
    {
        private static Random random = new Random();
        private static Queue<string> toRepeat = new Queue<string>();

        public void Generate(StreamWriter streamWriter, double outputFileSize)
        {
            var dataGenerator = new RandomDataFactory<DataStructure>();
            ulong totalWritedBytes = 0L;

            var randomRepeatSave = GetRandomRepeat();
            var randomRepeatInsert = GetRandomRepeat();

            while (totalWritedBytes < (outputFileSize * 1e9))
            {
                var data = dataGenerator.Generate();

                if (randomRepeatSave == 0)
                {
                    toRepeat.Enqueue(data.Text);
                    randomRepeatSave = GetRandomRepeat();
                }

                if (randomRepeatInsert < 1 && toRepeat.Count > 0)
                {
                    var toInsert = toRepeat.Dequeue();
                    data.Text = toInsert;
                    randomRepeatInsert = GetRandomRepeat();
                }

                var stringToWrite = data.ToString();
                var byteCount = (uint)Encoding.UTF8.GetByteCount(stringToWrite);
                totalWritedBytes += byteCount;
                streamWriter.WriteLine(stringToWrite);

                randomRepeatSave--;
                randomRepeatInsert--;
            }
        }

        private static int GetRandomRepeat()
        {
            return random.Next(100, 150);
        }
    }
}
