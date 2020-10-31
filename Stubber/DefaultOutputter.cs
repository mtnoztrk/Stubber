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

        private string GetStubFullPath(string methodName)
        {
            return "STUB_SOURCE.json";
            return $"{_config.StubFilePathPrefix}/{methodName}.json";
        }
        private string GetCodeFullPath(string methodName)
        {
            return "CODE.txt";
            return $"{_config.CodeFilePathPrefix}/{methodName}.txt";
        }

        public async Task OutputStubs(string methodName, Dictionary<string, Dictionary<string, object>> StubValues)
        {
            var stringified = JsonConvert.SerializeObject(StubValues,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.Indented
                });
            using (var stream = new FileStream(GetStubFullPath(methodName), FileMode.Create, FileAccess.Write, FileShare.Write, 4096))
            {
                var bytes = Encoding.UTF8.GetBytes(stringified);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public async Task OutputSnippets(string methodName, Dictionary<string, StubSnippet> SnippetValues)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in SnippetValues)
            {
                sb.AppendLine(item.Value.GetSnippet());
                sb.AppendLine();
            }
            using (var stream = new FileStream(GetCodeFullPath(methodName), FileMode.Create, FileAccess.Write, FileShare.Write, 4096))
            {
                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
