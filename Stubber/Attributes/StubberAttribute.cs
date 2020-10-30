using AgileObjects.ReadableExpressions;
using AspectInjector.Broker;
using Newtonsoft.Json;
using StubberProject.Helpers;
using StubberProject.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StubberProject.Attributes
{
    [Aspect(Scope.Global)]
#if DEBUG
    [Injection(typeof(StubberAttribute))]
#endif
    public sealed class StubberAttribute : Attribute
    {
        // below approach is being used
        // https://github.com/pamidur/aspect-injector/issues/77#issuecomment-443518810
        // If there are any problems with async methods(deadlock etc) author suggessted below example too.
        // https://github.com/pamidur/aspect-injector/blob/master/samples/UniversalWrapper/UniversalWrapper.cs

        private static MethodInfo _asyncTimezoneConverter = typeof(StubberAttribute).GetMethod(nameof(WrapAsync), BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo _syncTimezoneConverter = typeof(StubberAttribute).GetMethod(nameof(WrapSync), BindingFlags.NonPublic | BindingFlags.Static);

        [Advice(Kind.Around, Targets = Target.Method)]
        public object Stubber(
            [Argument(Source.Target)] Func<object[], object> target,
            [Argument(Source.Arguments)] object[] args,
            [Argument(Source.ReturnType)] Type retType,
            [Argument(Source.Metadata)] MethodBase methodMetadata
            )
        {
            if (retType == typeof(void) || retType == typeof(Task)) // return type is not an object. method signature is void or Task
            {
                return target(args);
            }

            if (typeof(Task).IsAssignableFrom(retType)) // return type is Task<object>. method signature is Task<object>
            {
                var syncResultType = retType.IsConstructedGenericType ? retType.GenericTypeArguments[0] : typeof(object);
                return _asyncTimezoneConverter.MakeGenericMethod(syncResultType).Invoke(this, new object[] { target, args, methodMetadata });
            }
            else // return type is object. method signature is object
            {
                return _syncTimezoneConverter.MakeGenericMethod(retType).Invoke(this, new object[] { target, args, methodMetadata });
            }
        }

        private static int IndexCounter = 0;

        /// <summary>
        /// Calls output method before and after service method call for sync methods
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="args"></param>
        /// <param name="methodMetadata"></param>
        /// <returns></returns>
        private static T WrapSync<T>(Func<object[], object> target, object[] args, MethodBase methodMetadata)
        {
            var localIndex = ++IndexCounter; // this needs to be copied to another variable, so same index does not get carried over
            ProcessArguments(methodMetadata, args, localIndex);
            var result = (T)target(args);
            ProcessResult(methodMetadata, args, result, localIndex);
            return result;
        }

        /// <summary>
        /// Calls output method before and after service method call for async methods
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="args"></param>
        /// <param name="methodMetadata"></param>
        /// <returns></returns>
        private static async Task<T> WrapAsync<T>(Func<object[], object> target, object[] args, MethodBase methodMetadata)
        {
            var localIndex = ++IndexCounter; // this needs to be copied to another variable, so same index does not get carried over
            ProcessArguments(methodMetadata, args, localIndex);
            var result = await (Task<T>)target(args);
            ProcessResult(methodMetadata, args, result, localIndex);
            return result;
        }

        private static string StubFileName = "STUB_SOURCE.json";
        private static string CodeFileName = "CODE.txt";

        private static Dictionary<string, Dictionary<string, object>> StubValues = new Dictionary<string, Dictionary<string, object>>();
        private static Dictionary<string, StubSnippet> MethodSignatures = new Dictionary<string, StubSnippet>();

        private static void AddToStubValues(string methodName, Dictionary<string, object> localResults)
        {
            if (StubValues.ContainsKey(methodName))
            {
                // might throw eksepsiyon :(
                var combinedDictionary = StubValues[methodName].Union(localResults).ToDictionary(k => k.Key, v => v.Value);
                StubValues[methodName] = combinedDictionary;
            }
            else
            {
                StubValues.Add(methodName, localResults);
            }
        }

        private static void ProcessArguments(MethodBase methodMetadata, object[] args, int index) // input parameters
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
            AddToStubValues($"{methodMetadata.Name}___{index}", arguments);
            MethodSignatures.Add($"{methodMetadata.Name}___{index}", new StubSnippet(methodMetadata, index));
        }

        private static void ProcessResult(MethodBase methodMetadata, object[] args, object result, int index) // output
        {
            //TODO: can split till end region to interface
            var localResults = new Dictionary<string, object>();
            localResults.Add("out", result);

            #region outParameters
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
            #endregion

            AddToStubValues($"{methodMetadata.Name}___{index}", localResults);
            //TODO: below can be split as well
            // every time object is rewritten entirely. couldn't find better solution. maybe i can try to add middleware so we can flush the file once request ends.
            var stringified = JsonConvert.SerializeObject(StubValues,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.Indented
                });
            using (var stream = new FileStream(StubFileName, FileMode.Create, FileAccess.Write, FileShare.Write, 4096))
            {
                var bytes = Encoding.UTF8.GetBytes(stringified);
                stream.Write(bytes, 0, bytes.Length);
            }

            AllTheWayUp();
        }

        private static void AllTheWayUp()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in MethodSignatures)
            {
                sb.AppendLine(item.Value.GetSnippet());
                sb.AppendLine();
            }
            using (var stream = new FileStream(CodeFileName, FileMode.Create, FileAccess.Write, FileShare.Write, 4096))
            {
                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
