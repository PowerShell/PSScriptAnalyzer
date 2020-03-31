// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.CrossCompatibility.Collection;
using Microsoft.PowerShell.CrossCompatibility.Data;
using Microsoft.PowerShell.CrossCompatibility.Retrieval;
using Newtonsoft.Json;
using SMA = System.Management.Automation;

namespace Microsoft.PowerShell.CrossCompatibility.Commands
{
    /// <summary>
    /// Class defining the New-PSCompatibilityProfile cmdlet.
    /// Creates a new PowerShell compatibility profile for the current PowerShell session
    /// </summary>
    [Cmdlet(VerbsCommon.New, CommandUtilities.MODULE_PREFIX + "Profile", DefaultParameterSetName = "OutFile")]
    [OutputType(typeof(CompatibilityProfileData))]
    public class NewPSCompatibilityProfileCommand : PSCmdlet
    {
        /// <summary>
        /// The name of the default profile directory.
        /// This directory lives in the module root.
        /// </summary>
        private const string DEFAULT_PROFILE_DIR_NAME = "profiles";

        /// <summary>
        /// The path of the profile file to create.
        /// </summary>
        [Parameter(ParameterSetName = "OutFile", Position = 0)]
        [ValidateNotNullOrEmpty]
        public string OutFile { get; set; }

        /// <summary>
        /// When set, no file is created and the profile object is returned to the pipeline.
        /// </summary>
        /// <value></value>
        [Parameter(ParameterSetName = "PassThru")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// When set, the profile is serialized with whitespace-formatted JSON.
        /// </summary>
        [Parameter(ParameterSetName = "OutFile")]
        [Parameter(ParameterSetName = "ProfileName")]
        public SwitchParameter NoCompress { get; set; }

        /// <summary>
        /// The ID of the platform to set in the profile.
        /// If OutFile and ProfileName are not set, this will also be the filename of the profile.
        /// </summary>
        /// <value></value>
        [Parameter]
        public string PlatformId { get; set; }

        /// <summary>
        /// Sets the name of the profile file to be created in the default profile directory.
        /// </summary>
        [Parameter(ParameterSetName = "ProfileName")]
        [ValidateNotNullOrEmpty]
        public string ProfileName { get; set; }

        /// <summary>
        /// If set, validate the generated profile before returning it.
        /// </summary>
        [Parameter]
        public SwitchParameter Validate { get; set; }

        /// <summary>
        /// Modules on paths starting with any of these will be excluded.
        /// </summary>
        /// <value></value>
        [Parameter]
        [ValidateNotNullOrEmpty]
        public string[] ExcludeModulePathPrefix { get; set; }

        /// <summary>
        /// Assemblies on paths starting with any of these will be excluded.
        /// </summary>
        /// <value></value>
        [Parameter]
        [ValidateNotNullOrEmpty]
        public string[] ExcludeAssemblyPathPrefix { get; set; }

        protected override void EndProcessing()
        {
            CompatibilityProfileData profile;
            IEnumerable<Exception> errors;
            using (var pwsh = SMA.PowerShell.Create())
            {
                var collectorBuilder = new CompatibilityProfileCollector.Builder();

                if (ExcludeModulePathPrefix != null && ExcludeModulePathPrefix.Length > 0)
                {
                    collectorBuilder.ExcludedModulePathPrefixes = ExcludeModulePathPrefix;
                }

                if (ExcludeAssemblyPathPrefix != null && ExcludeAssemblyPathPrefix.Length > 0)
                {
                    collectorBuilder.ExcludedAssemblyPathPrefixes = ExcludeAssemblyPathPrefix;
                }

                using (var profileCollector = collectorBuilder.Build(pwsh))
                {
                    profile = string.IsNullOrEmpty(PlatformId)
                        ? profileCollector.GetCompatibilityData(out errors)
                        : profileCollector.GetCompatibilityData(PlatformId, out errors);
                }
            }

            // Report any problems we hit
            foreach (Exception e in errors)
            {
                this.WriteExceptionAsWarning(e);
            }

            if (Validate)
            {
                var validator = new QuickCheckValidator();
                if (validator.IsProfileValid(profile, out IEnumerable<Exception> validationErrors))
                {
                    foreach (Exception validationError in validationErrors)
                    {
                        this.WriteExceptionAsWarning(validationError);
                    }
                }
            }

            // If PassThru is set, just pass the object back and we're done
            if (PassThru)
            {
                WriteObject(profile);
                return;
            }

            // Set the default profile path if it was not provided
            string outFilePath;
            if (string.IsNullOrEmpty(OutFile))
            {
                string profileName = ProfileName ?? profile.Id;
                outFilePath = GetProfilePath(profileName);
            }
            else
            {
                // Normalize the path to the output file we were given
                outFilePath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(OutFile);
            }

            // Create the directory containing the profile
            // If it already exists, this call will do nothing
            // If it cannot be created or is a non-directory file, an exception will be raised, which we let users handle
            Directory.CreateDirectory(Path.GetDirectoryName(outFilePath));

            // Create the JSON serializer
            JsonProfileSerializer jsonSerializer = NoCompress
                ? JsonProfileSerializer.Create(Formatting.Indented)
                : JsonProfileSerializer.Create(Formatting.None);

            // Write the file
            var outFile = new FileInfo(outFilePath);
            jsonSerializer.SerializeToFile(profile, outFile);

            // Return the FileInfo object for the profile
            WriteObject(outFile);
        }

        private string GetProfilePath(string profileName, string profileDirPath = null)
        {
            string dirPath = profileDirPath;
            if (dirPath == null)
            {
                string moduleRoot = Path.GetDirectoryName(MyInvocation.MyCommand.Module.Path);
                dirPath = Path.Combine(moduleRoot, DEFAULT_PROFILE_DIR_NAME);
            }
            return Path.Combine(dirPath, profileName + ".json");
        }
    }
}