using AspectInjector.Broker;
using StubberProject.Helpers;
using StubberProject.Models;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace StubberProject.Attributes
{
    [Aspect(Scope.Global)]
#if DEBUG
    [Injection(typeof(StubberTargetAttribute))]
#endif
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class StubberTargetAttribute : Attribute
    {
        // below approach is being used
        // https://github.com/pamidur/aspect-injector/issues/77#issuecomment-443518810
        // If there are any problems with async methods(deadlock etc) author suggessted below example too.
        // https://github.com/pamidur/aspect-injector/blob/master/samples/UniversalWrapper/UniversalWrapper.cs

        private static MethodInfo _asyncTimezoneConverter = typeof(StubberTargetAttribute).GetMethod(nameof(WrapAsync), BindingFlags.NonPublic | BindingFlags.Static);
        private static MethodInfo _syncTimezoneConverter = typeof(StubberTargetAttribute).GetMethod(nameof(WrapSync), BindingFlags.NonPublic | BindingFlags.Static);

        [Advice(Kind.Around, Targets = Target.Method)]
        public object StubberTarget(
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

        /// <summary>
        /// Calls methods before and after method call for sync methods
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="args"></param>
        /// <param name="methodMetadata"></param>
        /// <returns></returns>
        private static T WrapSync<T>(Func<object[], object> target, object[] args, MethodBase methodMetadata)
        {
            BeforeExecution(methodMetadata, args);
            var result = (T)target(args);
            AfterExecution(methodMetadata, args, result);
            return result;
        }

        /// <summary>
        /// Calls methods before and after method call for async methods
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="args"></param>
        /// <param name="methodMetadata"></param>
        /// <returns></returns>
        private static async Task<T> WrapAsync<T>(Func<object[], object> target, object[] args, MethodBase methodMetadata)
        {
            BeforeExecution(methodMetadata, args);
            var result = await (Task<T>)target(args);
            AfterExecution(methodMetadata, args, result);
            return result;
        }

        private static void BeforeExecution(MethodBase methodMetadata, object[] args)
        {
            var manager = ServiceLocator.GetService<IStubberManager>();
            manager.StartRecording(methodMetadata.Name);
            var arguments = manager.ProcessArguments(methodMetadata, args);
            manager.AddToStubValues("Target", arguments);
        }

        private static void AfterExecution(MethodBase methodMetadata, object[] args, object result)
        {
            var manager = ServiceLocator.GetService<IStubberManager>();
            var localResults = manager.ProcessResult(methodMetadata, args, result);
            manager.AddToStubValues($"Target", localResults);
            manager.StopRecording();
        }
    }
}
