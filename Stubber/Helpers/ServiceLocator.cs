using System;
using System.Collections.Generic;

namespace StubberProject.Helpers
{
    /// <summary>
    /// Only for <seealso cref="StubberAttribute"/> usage. AspectInjector library can not use normal dependency injection.
    /// </summary>
    internal static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> registeredServices = new Dictionary<Type, object>();

        public static T GetService<T>()
        {
            return (T)registeredServices[typeof(T)];
        }

        public static void RegisterService<T>(T service)
        {
            registeredServices[typeof(T)] = service;
        }

        public static Int32 Count
        {
            get { return registeredServices.Count; }
        }
    }
}
