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

        public T TryMe<T>(string dictKey, string propertyKey)
        {

            return Match.Create<T>((o) =>
            {
                var property = stubs.SelectToken(dictKey).SelectToken(propertyKey);
                if (typeof(T).IsList())
                    return property.ToString() == JsonConvert.SerializeObject(o, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        Formatting = Formatting.Indented

                    });
                else
                    return JsonConvert.SerializeObject(property.ToObject<T>()) == JsonConvert.SerializeObject(o);
            });
        }
    }
}
