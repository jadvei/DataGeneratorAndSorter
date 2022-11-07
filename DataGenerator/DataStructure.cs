namespace DataGenerator
{
    public class DataStructure
    {
        public int Number { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return $"{Number}. {Text}";
        }
    }
}