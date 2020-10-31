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
            var path = $"{outputName}.json";
            if(!string.IsNullOrEmpty(_config.StubFilePathPrefix))
                path = Path.Combine(_config.StubFilePathPrefix, path);
            return path;
        }
        private string GetCodeFullPath(string outputName)
        {
            var path = $"{outputName}.txt";
            if (!string.IsNullOrEmpty(_config.CodeFilePathPrefix))
                path = Path.Combine(_config.CodeFilePathPrefix, path);
            return path;
        }

        public async Task OutputStubs(string outputName, Dictionary<string, Dictionary<string, object>> stubValues)
        {
            var stringified = JsonConvert.SerializeObject(stubValues,
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

        public async Task OutputSnippets(string outputName, Dictionary<string, string> snippetValues)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in snippetValues)
            {
                sb.AppendLine(item.Value);
            }
            using (var stream = new FileStream(GetCodeFullPath(outputName), FileMode.Create, FileAccess.Write, FileShare.Write, 4096))
            {
                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
