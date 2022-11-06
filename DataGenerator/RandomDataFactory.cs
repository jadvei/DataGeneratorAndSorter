using System;
using System.Text;

namespace DataGenerator
{
    public sealed class RandomDataFactory<T>
    {
        private readonly Random random = new Random();
        private readonly StringBuilder stringBuilder = new StringBuilder();
        public T Generate()
        {
            T item = Activator.CreateInstance<T>();

            FillProperties(item);

            return item;
        }

        private void FillProperties(T item)
        {
            var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var prop in props)
            {
                var value = GenerateValue(prop.PropertyType);
                prop.SetValue(item, value);
            }
        }

        private object GenerateValue(Type propertyType)
        {
            if(propertyType == typeof(string))
                return GetRandomString();
            if(propertyType == typeof(int))
                return GetRandomInt();

            throw new NotSupportedException("Supports only string and int");
        }

        private int GetRandomInt()
        {
            return random.Next(0, (int)1e3);
        }

        private string GetRandomString()
        {
            var values = DataSet.GetStringData();
            var randomWordsCount = random.Next(1, 5);
            for (int i = 0; i < randomWordsCount; i++)
            {
                var randomEntry = random.Next(0, values.Length);
                stringBuilder.Append(values[randomEntry]);
                stringBuilder.Append(" ");
            }

            var result = stringBuilder.ToString();
            stringBuilder.Clear();
            return result;
        }
    }
}
