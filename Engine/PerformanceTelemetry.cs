// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    internal static class PerformanceTelemetry
    {
        private static volatile bool enabled;
        public static bool Enabled { get => enabled; set => enabled = value; }

        internal static long LookupMisses, LookupBypasses, MetadataQueries, ManifestValidations;
        internal static long LookupResolutionFailures, LookupRetries;
        internal static long MetadataFailures, MetadataRetries;
        private static long lockWaitTicks, lockHoldTicks;

        internal static void Increment(ref long counter)
        {
            if (Enabled)
            {
                Interlocked.Increment(ref counter);
            }
        }

        // Reset only between measurements, when no analysis is running.
        public static void Reset()
        {
            Interlocked.Exchange(ref LookupMisses, 0);
            Interlocked.Exchange(ref LookupBypasses, 0);
            Interlocked.Exchange(ref LookupResolutionFailures, 0);
            Interlocked.Exchange(ref LookupRetries, 0);
            Interlocked.Exchange(ref MetadataQueries, 0);
            Interlocked.Exchange(ref MetadataFailures, 0);
            Interlocked.Exchange(ref MetadataRetries, 0);
            Interlocked.Exchange(ref ManifestValidations, 0);
            Interlocked.Exchange(ref lockWaitTicks, 0);
            Interlocked.Exchange(ref lockHoldTicks, 0);
        }

        public static Dictionary<string, long> Snapshot()
        {
            return new Dictionary<string, long>
            {
                { "LookupMisses", Interlocked.Read(ref LookupMisses) },
                { "LookupBypasses", Interlocked.Read(ref LookupBypasses) },
                { "LookupResolutionFailures", Interlocked.Read(ref LookupResolutionFailures) },
                { "LookupRetries", Interlocked.Read(ref LookupRetries) },
                { "MetadataQueries", Interlocked.Read(ref MetadataQueries) },
                { "MetadataFailures", Interlocked.Read(ref MetadataFailures) },
                { "MetadataRetries", Interlocked.Read(ref MetadataRetries) },
                { "ManifestValidations", Interlocked.Read(ref ManifestValidations) },
                { "LockWaitTicks", Interlocked.Read(ref lockWaitTicks) },
                { "LockHoldTicks", Interlocked.Read(ref lockHoldTicks) },
            };
        }

        internal static LockScope EnterLock(object syncRoot) => new LockScope(syncRoot);

        internal struct LockScope : IDisposable
        {
            private readonly object syncRoot;
            private readonly bool measured;
            private readonly long acquired;

            internal LockScope(object syncRoot)
            {
                this.syncRoot = syncRoot;
                measured = Enabled;
                long start = measured ? Stopwatch.GetTimestamp() : 0;
                Monitor.Enter(syncRoot);
                acquired = measured ? Stopwatch.GetTimestamp() : 0;
                if (measured)
                {
                    Interlocked.Add(ref lockWaitTicks, acquired - start);
                }
            }

            public void Dispose()
            {
                if (measured)
                {
                    Interlocked.Add(ref lockHoldTicks, Stopwatch.GetTimestamp() - acquired);
                }
                Monitor.Exit(syncRoot);
            }
        }
    }
}
