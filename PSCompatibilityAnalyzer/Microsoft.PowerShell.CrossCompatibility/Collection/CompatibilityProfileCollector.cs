using System;
using System.Collections.Generic;
using Microsoft.PowerShell.CrossCompatibility.Data;
using Microsoft.PowerShell.CrossCompatibility.Data.Platform;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using SMA = System.Management.Automation;

namespace Microsoft.PowerShell.CrossCompatibility.Collection
{
    public class CompatibilityProfileCollector : IDisposable
    {
        private SMA.PowerShell _pwsh;

        private readonly PowerShellDataCollector _pwshDataCollector;

        private readonly PlatformInformationCollector _platformInfoCollector;

        public CompatibilityProfileCollector(SMA.PowerShell pwsh)
        {
            _pwsh = pwsh;
            _pwshDataCollector = new PowerShellDataCollector(pwsh);
            _platformInfoCollector = new PlatformInformationCollector(pwsh);
        }

        public CompatibilityProfileData GetCompatibilityData()
        {
            return GetCompatibilityData(platformId: null);
        }

        public CompatibilityProfileData GetCompatibilityData(string platformId)
        {
            PlatformData platformData = _platformInfoCollector.GetPlatformData();

            return new CompatibilityProfileData()
            {
                Id = platformId ?? PlatformNaming.GetPlatformName(platformData),
                Platform = platformData,
                Runtime = GetRuntimeData()
            };
        }

        public RuntimeData GetRuntimeData()
        {
            return new RuntimeData()
            {
                Types = TypeDataCollection.GetAvailableTypeData(out IEnumerable<CompatibilityAnalysisException> errors),
                Common = GetCommonPowerShellData(),
                NativeCommands = GetNativeCommandData(),
            };
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _pwsh.Dispose();
                }

                _pwsh = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CompatibilityProfileCollector() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}