using System;

#if !CORECLR
using System.Collections.Concurrent;
#endif

namespace Microsoft.PowerShell.ScriptAnalyzer.Internal
{
    internal static class Polyfill
    {

#if !CORECLR
        private static ConcurrentDictionary<Type, Array> s_emptyArrays = new ConcurrentDictionary<Type, Array>();
#endif

        public static T[] GetEmptyArray<T>()
        {
#if CORECLR
            return Array.Empty<T>();
#else
            return (T[])s_emptyArrays.GetOrAdd(typeof(T), (_) => new T[0]);
#endif
        }
    }
}