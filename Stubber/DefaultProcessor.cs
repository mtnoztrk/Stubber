using AgileObjects.ReadableExpressions;
using StubberProject.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace StubberProject
{
    internal class DefaultProcessor : IProcessor
    {
        public Dictionary<string, object> ProcessArguments(MethodBase methodMetadata, object[] args)
        {
            var arguments = new Dictionary<string, object>();
            var methodParameters = methodMetadata.GetParameters();
            for (int i = 0; i < methodParameters.Length; i++)
            {
                if (!methodParameters[i].IsOut) // out parameters should be saved after method call ended
                {
                    var temp = args[i];
                    if (temp is Expression exp)
                    {
                        temp = exp.ToReadableString();
                    }
                    else if (temp == null)
                    {
                        // nothing needed
                    }
                    else if (temp.GetType().IsFunc())
                    {
                        temp = "funcMethod"; //TODO: ??
                    }
                    arguments.Add($"in_{methodParameters[i].Name}", temp);
                }
            }
            return arguments;
        }

        public Dictionary<string, object> ProcessResult(MethodBase methodMetadata, object[] args, object result)
        {
            var localResults = new Dictionary<string, object>();
            localResults.Add("out", result);

            var methodParameters = methodMetadata.GetParameters();
            if (methodParameters.Any(c => c.IsOut))
            {
                for (int i = 0; i < methodParameters.Length; i++)
                {
                    if (methodParameters[i].IsOut)
                    {
                        localResults.Add($"out_{methodParameters[i].Name}", args[i]);
                    }
                }
            }
            return localResults;
        }
    }
}
