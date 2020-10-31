using System.Collections.Generic;
using System.Reflection;

namespace StubberProject
{
    public interface IProcessor
    {
        Dictionary<string, object> ProcessArguments(MethodBase methodMetadata, object[] args);
        Dictionary<string, object> ProcessResult(MethodBase methodMetadata, object[] args, object result);
    }
}
