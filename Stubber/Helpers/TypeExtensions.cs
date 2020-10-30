using System;
using System.Collections;

namespace StubberProject.Helpers
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
    }
}
