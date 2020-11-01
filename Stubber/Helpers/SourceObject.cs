using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StubberProject.Extensions;
using System.IO;

namespace StubberProject.Helpers
{
    public class SourceObject //TODO: factory
    {
        private JObject stubs;
        public SourceObject(string filePath)
        {
            var jsonString = File.ReadAllText(filePath);
            stubs = JObject.Parse(jsonString);
        }

        public T Get<T>(string dictKey, string propertyKey)
        {
            var property = stubs.SelectToken(dictKey).SelectToken(propertyKey);
            if (typeof(T).IsList())
                return JsonConvert.DeserializeObject<T>(property.ToString());

            return property.ToObject<T>();
        }

        public T Matcher<T>(string dictKey, string propertyKey)
        {
            return Match.Create<T>((o) =>
            {
                var property = stubs.SelectToken(dictKey).SelectToken(propertyKey);
                var result = property.ToString() == JsonConvert.SerializeObject(o, NewtonsoftHelper.Settings);
                return result;
            });
        }
    }
}
