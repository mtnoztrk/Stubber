using Newtonsoft.Json;

namespace StubberProject.Helpers
{
    internal static class NewtonsoftHelper
    {

        public static JsonSerializerSettings Settings => new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented

        };
    }
}
