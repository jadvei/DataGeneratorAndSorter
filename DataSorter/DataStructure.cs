using static DataSorter.Merger;

namespace DataSorter
{
    public struct DataStructure
    {
        public int DotIndex { get; set; }
        public string Text { get; set; }

        public string Value { get; set; }

        public int GetNumber()
        {
            var parseResult = int.TryParse(Value.Substring(0, DotIndex), out var number);

            if (parseResult == false)
            {
                throw new NotSupportedException($"Failed to get number from {Value}");
            }

            return number;
        }

        public static DataStructure FromString(string row)
        {
            var dotIndex = row.IndexOf('.');

            if (dotIndex == -1)
            {
                throw new NotSupportedException($"Row without dot not supported {row}");
            }

            var text = row.Substring(dotIndex + 2);

            return new DataStructure { DotIndex = dotIndex, Text = text, Value = row };
        }

        public override string ToString()
        {
            return Value;
        }
    }
}