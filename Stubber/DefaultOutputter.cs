using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StubberProject.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StubberProject
{
    public class DefaultOutputter : IOutputter
    {
        private StubberOption _config { get; set; }
        public DefaultOutputter(IOptions<StubberOption> options)
        {
            _config = options.Value;
        }

        private string GetStubFullPath(string outputName)
        {
            return $"{outputName}.json";
            return $"{_config.StubFilePathPrefix}/{outputName}.json";
        }
        private string GetCodeFullPath(string outputName)
        {
            return $"{outputName}.txt";
            return $"{_config.CodeFilePathPrefix}/{outputName}.txt";
        }

        public async Task OutputStubs(string outputName, Dictionary<string, Dictionary<string, object>> StubValues)
        {
            var stringified = JsonConvert.SerializeObject(StubValues,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.Indented
                });
            using (var stream = new FileStream(GetStubFullPath(outputName), FileMode.Create, FileAccess.Write, FileShare.Write, 4096))
            {
                var bytes = Encoding.UTF8.GetBytes(stringified);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public async Task OutputSnippets(string outputName, Dictionary<string, StubSnippet> SnippetValues)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in SnippetValues)
            {
                sb.AppendLine(item.Value.GetSnippet());
                sb.AppendLine();
            }
            using (var stream = new FileStream(GetCodeFullPath(outputName), FileMode.Create, FileAccess.Write, FileShare.Write, 4096))
            {
                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
