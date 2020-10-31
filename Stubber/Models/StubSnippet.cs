using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace StubberProject.Models
{
    public class StubSnippet
    {
        public IEnumerable<string> MethodParameters { get; }
        public IEnumerable<string> OutParameterDefinitions { get; }
        public string ReturnSignature { get; }
        private string MethodName { get; }
        private string MethodNameIndexed { get; }

        public StubSnippet(MethodBase methodBase, int index)
        {
            MethodName = methodBase.Name;
            MethodNameIndexed = $"{MethodName}___{index}";
            //TODO: namespaceleri confige bagla
            MethodParameters = GenerateMethodParameters(methodBase);
            OutParameterDefinitions = GenerateOutParameterDefinitions(methodBase);
            ReturnSignature = GeneateReturnSignature(methodBase);
        }

        public string GetSnippet()
        {
            var lineTabs = "\t";
            var methodTabs = "\t\t";
            var paramTabs = "\t\t\t";
            var outLines =  $"{lineTabs}{string.Join(";" + Environment.NewLine + lineTabs, OutParameterDefinitions)}";
            var methodSignature =  $"{Environment.NewLine}{paramTabs}{string.Join("," + Environment.NewLine + paramTabs, MethodParameters)}";
            return $"{outLines}" +
                $"{Environment.NewLine}{lineTabs}_discountServiceMock" +
                $"{Environment.NewLine}{methodTabs}.Setup(c => c.{MethodName}({methodSignature}))" +
                $"{Environment.NewLine}{methodTabs}.Returns({ReturnSignature});";
        }

        private string Process(ParameterInfo parameter) //TODO move to another class and cache this motherfucker  https://stackoverflow.com/a/43083361/8616482
        {
            var HasValue = "";
            Type ParameterType = (parameter.IsOut || parameter.ParameterType.IsByRef) ? parameter.ParameterType.GetElementType() : parameter.ParameterType;
            if (ParameterType.GetProperties().Count() == 2 && ParameterType.GetProperties()[0].Name.Equals("HasValue"))
            {
                HasValue = "?";
                ParameterType = ParameterType.GetProperties()[1].PropertyType;
            }
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                var expr = new CodeTypeReferenceExpression(ParameterType);
                var prov = new CSharpCodeProvider();
                prov.GenerateCodeFromExpression(expr, sw, new CodeGeneratorOptions());
            }
            //var result = string.Concat(sb.ToString(), HasValue, " ", parameter.Name);
            var result = string.Concat(sb.ToString(), HasValue);
            return result;
        }

        private string GeneateReturnSignature(MethodBase methodBase)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                var expr = new CodeTypeReferenceExpression((methodBase as MethodInfo).ReturnType.FullName);
                var prov = new CSharpCodeProvider();
                prov.GenerateCodeFromExpression(expr, sw, new CodeGeneratorOptions());
            }
            var returnType = sb.ToString();

            if (returnType.StartsWith("System.Threading.Tasks.Task<"))
            {
                returnType = returnType.Replace("System.Threading.Tasks.Task<", "");
                returnType = returnType.Remove(returnType.Length - 1, 1);
                return $"System.Threading.Tasks.Task.FromResult(source.Get<{returnType}>(\"{MethodNameIndexed}\", \"out\"))"; ;
            }
            else
            {
                return $"source.Get<{returnType}>(\"{MethodNameIndexed}\", \"out\")";
            }
        }

        private IEnumerable<string> GenerateMethodParameters(MethodBase methodBase)
        {
            return methodBase.GetParameters().Select(c =>
            {
                return c.IsOut ?
                    $"out {c.Name}" :
                    $"source.Get<{Process(c)}>(\"{MethodNameIndexed}\", \"in_{c.Name}\")";
            });
        }

        private IEnumerable<string> GenerateOutParameterDefinitions(MethodBase methodBase)
        {
            return methodBase.GetParameters().Where(c => c.IsOut).Select(c =>
            {
                return $"var {c.Name} = source.Get<{Process(c)}>(\"{MethodNameIndexed}\", \"out_{c.Name}\");";
            });
        }
    }
}
