using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StubberProject.Helpers;
using StubberProject.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StubberProject
{
    public class DefaultOutputter : IOutputter
    {
        private readonly StubberOption _config;
        public DefaultOutputter(IOptions<StubberOption> options)
        {
            _config = options.Value;
        }

        public static string GetStubFullPath(StubberOption config, string outputName)
        {
            var path = $"{outputName}.json";
            if(!string.IsNullOrEmpty(config.StubFilePathPrefix))
                path = Path.Combine(config.StubFilePathPrefix, path);
            return path;
        }
        public static string GetCodeFullPath(StubberOption config, string outputName)
        {
            var path = $"{outputName}.txt";
            if (!string.IsNullOrEmpty(config.CodeFilePathPrefix))
                path = Path.Combine(config.CodeFilePathPrefix, path);
            return path;
        }

        public async Task OutputStubs(string outputName, Dictionary<string, Dictionary<string, object>> stubValues)
        {
            try
            {
                var stringified = JsonConvert.SerializeObject(stubValues,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            Formatting = Formatting.Indented
                        });
                using (var stream = new FileStream(GetStubFullPath(_config, outputName), FileMode.Create, FileAccess.Write, FileShare.Write, 4096))
                {
                    var bytes = Encoding.UTF8.GetBytes(stringified);
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (System.Exception ex)
            {

                throw;
            }
        }

        public async Task OutputSnippets(string outputName, List<string> snippetValues)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in snippetValues)
            {
                sb.AppendLine(item);
            }
            using (var stream = new FileStream(GetCodeFullPath(_config, outputName), FileMode.Create, FileAccess.Write, FileShare.Write, 4096))
            {
                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
