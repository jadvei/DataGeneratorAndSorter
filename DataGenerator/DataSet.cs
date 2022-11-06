using System;
using System.IO;

namespace DataGenerator
{
    public static class DataSet
    {
        private const string englishWordsPath = @".\Content\EnglishWords.txt";
        private static string[] englishWordsCached = Array.Empty<string>();

        internal static string[] GetStringData()
        {
            if (englishWordsCached.Length == 0)
            {
                englishWordsCached = File.ReadAllLines(englishWordsPath);
            }

            return englishWordsCached;
        }
    }
}
