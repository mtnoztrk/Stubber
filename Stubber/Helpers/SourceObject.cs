using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace StubberProject.Helpers
{
    public class SourceObject
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
    }
}
