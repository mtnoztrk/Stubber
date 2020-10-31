using System.Collections.Generic;
using System.Reflection;

namespace StubberProject
{
    public interface IProcessor
    {
        Dictionary<string, object> ProcessArguments(MethodBase methodMetadata, object[] args);
        Dictionary<string, object> ProcessResult(MethodBase methodMetadata, object[] args, object result);
        string ProcessSnippet(MethodBase methodBase, string jsonAccessor);
        string GenerateMethodEntry(MethodBase methodBase, string jsonAccessor, string outputName = null);
    }
}
