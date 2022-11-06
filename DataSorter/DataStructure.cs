namespace DataGenerator
{
    public struct DataStructure
    {
        public string Number { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return $"{Number}.{Text}";
        }
    }
}