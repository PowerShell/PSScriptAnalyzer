// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.CrossCompatibility.Data;
using Microsoft.PowerShell.CrossCompatibility.Retrieval;
using Microsoft.PowerShell.CrossCompatibility.Utility;
using Newtonsoft.Json;

namespace Microsoft.PowerShell.CrossCompatibility.Commands
{
    /// <summary>
    /// Class defining the ConvertTo-PSCompatibilityJson cmdlet.
    /// Turns given .NET objects into JSON strings.
    /// </summary>
    [Cmdlet(VerbsData.ConvertTo, CommandUtilities.MODULE_PREFIX + "Json")]
    [OutputType(typeof(string))]
    public class ConvertToPSCompatibilityJsonCommand : PSCmdlet
    {
        private JsonProfileSerializer _serializer;

        /// <summary>
        /// The object to convert to JSON.
        /// </summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public object[] Item { get; set; }

        /// <summary>
        /// If set, do not include whitespace like indentation or newlines in the output JSON.
        /// </summary>
        [Parameter]
        [Alias("Compress")]
        public SwitchParameter NoWhitespace { get; set; }

        protected override void BeginProcessing()
        {
            _serializer = JsonProfileSerializer.Create(NoWhitespace ? Formatting.None : Formatting.Indented);
        }

        protected override void ProcessRecord()
        {
            foreach (object obj in Item)
            {
                WriteObject(_serializer.Serialize(obj));
            }
            return;
        }
    }

    /// <summary>
    /// Class defining the ConvertFrom-PSCompatibilityJson JSON.
    /// Turns given JSON into .NET compatibility profile objects.
    /// </summary>
    [Cmdlet(VerbsData.ConvertFrom, CommandUtilities.MODULE_PREFIX + "Json", DefaultParameterSetName = "JsonSource")]
    public class ConvertFromPSCompatibilityJsonCommand : PSCmdlet
    {
        private JsonProfileSerializer _serializer;

        /// <summary>
        /// The source of the JSON to convert from. Can be a JSON string, a FileInfo or a TextReader object.
        /// </summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ParameterSetName = "JsonSource")]
        public object[] JsonSource { get; set; }

        /// <summary>
        /// The path of a file to convert JSON from.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Path")]
        [ValidateNotNullOrEmpty()]
        public string[] Path { get; set; }

        public ConvertFromPSCompatibilityJsonCommand()
        {
            _serializer = JsonProfileSerializer.Create();
        }

        protected override void ProcessRecord()
        {
            if (Path != null)
            {
                foreach (string filePath in Path)
                {
                    string absolutePath = this.GetNormalizedAbsolutePath(filePath);
                    WriteObject(_serializer.DeserializeFromFile(absolutePath));
                }
                return;
            }

            if (JsonSource != null)
            {
                foreach (object jsonSourceItem in JsonSource)
                {
                    switch (jsonSourceItem)
                    {
                        case string jsonString:
                            WriteObject(_serializer.Deserialize(jsonString));
                            continue;

                        case FileInfo jsonFile:
                            WriteObject(_serializer.Deserialize(jsonFile));
                            continue;

                        case TextReader jsonReader:
                            WriteObject(_serializer.Deserialize(jsonReader));
                            continue;

                        default:
                            this.WriteExceptionAsError(
                                new ArgumentException($"Unsupported type for {nameof(JsonSource)} parameter. Should be a string, FileInfo or TextReader object."),
                                errorId: "InvalidArgument",
                                errorCategory: ErrorCategory.InvalidArgument);
                            continue;
                    }
                }
            }
        }
    }
}