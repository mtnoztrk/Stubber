using AgileObjects.ReadableExpressions;
using Microsoft.CSharp;
using Microsoft.Extensions.Caching.Memory;
using StubberProject.Extensions;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace StubberProject
{
    internal class DefaultProcessor : IProcessor
    {
        private readonly string _inPrefix = "in_";
        private readonly string _outPrefix = "out_";
        private readonly string _sourceVariableName = "source";
        private readonly IMemoryCache _cache;

        public DefaultProcessor(IMemoryCache cache)
        {
            _cache = cache;
        }

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
                    arguments.Add($"{_inPrefix}{methodParameters[i].Name}", temp);
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
                        localResults.Add($"{_outPrefix}{methodParameters[i].Name}", args[i]);
                    }
                }
            }
            return localResults;
        }

        public string ProcessSnippet(MethodBase methodBase, string jsonAccessor)
        {
            var InterfaceName = (methodBase as MethodInfo).GetInterfacesForMethod().FirstOrDefault()?.Name;
            if (string.IsNullOrEmpty(InterfaceName)) return null; // if there is no interface found, this can't be stubbed!

            //TODO: namespaceleri confige bagla
            var methodParameters = GenerateMethodParameters(methodBase, jsonAccessor); // parameters used for calling subbed method.
            var outParameterDefinitions = GenerateOutParameterDefinitions(methodBase, jsonAccessor); // out variables can not be used directly, first we need to define variable and call it from stubbed method.
            var returnSignature = GeneateReturnSignature(methodBase, jsonAccessor);

            var lineTabs = "\t";
            var methodTabs = "\t\t";
            var paramTabs = "\t\t\t";

            var outLines = $"{lineTabs}{string.Join(";" + Environment.NewLine + lineTabs, outParameterDefinitions)}";
            var methodSignature = $"{Environment.NewLine}{paramTabs}{string.Join("," + Environment.NewLine + paramTabs, methodParameters)}";
            return $"{outLines}" +
                $"{Environment.NewLine}{lineTabs}_{InterfaceName}Mock" +
                $"{Environment.NewLine}{methodTabs}.Setup(c => c.{methodBase.Name}({methodSignature}))" +
                $"{Environment.NewLine}{methodTabs}.Returns({returnSignature});";
        }

        #region HelperMethods
        private string GetParameterInfoAsValidCode(ParameterInfo parameter) // special thanks https://stackoverflow.com/a/43083361/8616482
        {
            var HasValue = "";
            Type parameterType = (parameter.IsOut || parameter.ParameterType.IsByRef) ? parameter.ParameterType.GetElementType() : parameter.ParameterType;
            if (parameterType.GetProperties().Count() == 2 && parameterType.GetProperties()[0].Name.Equals("HasValue"))
            {
                HasValue = "?";
                parameterType = parameterType.GetProperties()[1].PropertyType;
            }
            var typeAsString = _cache.GetOrCreate(parameterType, (ce) =>
            {
                StringBuilder sb = new StringBuilder();
                using (StringWriter sw = new StringWriter(sb))
                {
                    var expr = new CodeTypeReferenceExpression(parameterType);
                    var prov = new CSharpCodeProvider();
                    prov.GenerateCodeFromExpression(expr, sw, new CodeGeneratorOptions());
                }
                return sb.ToString();
            });
            var result = string.Concat(typeAsString, HasValue);
            return result;
        }

        private string GetReturnType(MethodBase methodBase)
        {
            var returnTypeFullName = (methodBase as MethodInfo).ReturnType.FullName;
            return _cache.GetOrCreate(returnTypeFullName, (ce) =>
            {
                StringBuilder sb = new StringBuilder();
                using (StringWriter sw = new StringWriter(sb))
                {
                    var expr = new CodeTypeReferenceExpression(returnTypeFullName);
                    var prov = new CSharpCodeProvider();
                    prov.GenerateCodeFromExpression(expr, sw, new CodeGeneratorOptions());
                }
                return sb.ToString();
            });
        }

        private string GeneateReturnSignature(MethodBase methodBase, string jsonAccessor)
        {
            var returnType = GetReturnType(methodBase);

            if (returnType.StartsWith("System.Threading.Tasks.Task<"))
            {
                // async methods are used like this: Task.FromResult(returnVariable). string is reconstructed for it.
                returnType = returnType.Replace("System.Threading.Tasks.Task<", "");
                returnType = returnType.Remove(returnType.Length - 1, 1);
                return $"System.Threading.Tasks.Task.FromResult({_sourceVariableName}.Get<{returnType}>(\"{jsonAccessor}\", \"out\"))"; ;
            }
            else
            {
                return $"{_sourceVariableName}.Get<{returnType}>(\"{jsonAccessor}\", \"out\")";
            }
        }

        private IEnumerable<string> GenerateMethodParameters(MethodBase methodBase, string jsonAccessor)
        {
            return methodBase.GetParameters().Select(c =>
            {
                return c.IsOut ?
                    $"out {c.Name}" :
                    $"{_sourceVariableName}.Get<{GetParameterInfoAsValidCode(c)}>(\"{jsonAccessor}\", \"{_inPrefix}{c.Name}\")";
            });
        }

        private IEnumerable<string> GenerateOutParameterDefinitions(MethodBase methodBase, string jsonAccessor)
        {
            return methodBase.GetParameters().Where(c => c.IsOut).Select(c =>
            {
                return $"var {c.Name} = {_sourceVariableName}.Get<{GetParameterInfoAsValidCode(c)}>(\"{jsonAccessor}\", \"{_outPrefix}{c.Name}\");";
            });
        }
        #endregion
    }
}
