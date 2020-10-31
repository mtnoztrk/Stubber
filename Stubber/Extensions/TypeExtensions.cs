using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StubberProject.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsFunc(this Type type)
        {
            Type generic = null;
            if (type.IsGenericTypeDefinition) generic = type;
            else if (type.IsGenericType) generic = type.GetGenericTypeDefinition();
            if (generic == null) return false;
            if (generic == typeof(Func<>)) return true;
            if (generic == typeof(Func<,>)) return true;
            if (generic == typeof(Func<,,>)) return true;
            if (generic == typeof(Func<,,>)) return true;
            return false;
        }

        public static bool IsList(this Type type)
        {
            return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static IEnumerable<InterfaceMapping> GetAllInterfaceMaps(this Type aType)
        {
            return aType.GetTypeInfo()
                .ImplementedInterfaces
                .Select(ii => aType.GetInterfaceMap(ii));
        }

        public static Type[] GetInterfacesForMethod(this MethodInfo mi)
        {
            return mi.ReflectedType
                .GetAllInterfaceMaps()
                .Where(im => im.TargetMethods.Any(tm => tm == mi))
                .Select(im => im.InterfaceType)
                .ToArray();
        }
    }
}
