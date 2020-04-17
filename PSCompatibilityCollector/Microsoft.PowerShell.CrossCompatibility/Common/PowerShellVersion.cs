// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.PowerShell.CrossCompatibility
{
    /// <summary>
    /// Class created to bridge the gap between System.Version and System.Management.Automation.SemanticVersion.
    /// Allows a version encoding either to be deserialized into this type.
    /// </summary>
    public class PowerShellVersion
    {
        /// <summary>
        /// Create a PowerShellVersion from another version object.
        /// Currently accepts a dynamic input to allow SemanticVersions.
        /// </summary>
        /// <param name="versionInput">A version-like object describing a PowerShell version.</param>
        /// <returns>A PowerShellVersion object.</returns>
        public static PowerShellVersion Create(dynamic versionInput)
        {
            switch (versionInput)
            {
                case Version systemVersion:
                    return (PowerShellVersion)systemVersion;

                case string versionString:
                    return Parse(versionString);

                default:
                    if (versionInput.BuildLabel != null)
                    {
                        return new PowerShellVersion(versionInput.Major, versionInput.Minor, versionInput.Patch, $"{versionInput.PreReleaseLabel}+{versionInput.BuildLabel}");
                    }

                    return new PowerShellVersion(versionInput.Major, versionInput.Minor, versionInput.Patch, versionInput.PreReleaseLabel);
            }
        }

        /// <summary>
        /// Parse a PowerShellVersion from a string.
        /// </summary>
        /// <param name="versionStr">The version-describing string to parse.</param>
        /// <returns>A PowerShellVersion, as described by the string.</returns>
        public static PowerShellVersion Parse(string versionStr)
        {
            if (versionStr == null)
            {
                throw new ArgumentNullException(nameof(versionStr));
            }

            int[] versionParts = new int[3] { -1, -1, -1 };

            int sectionStartOffset = 0;
            int dotCount = 0;
            int i;
            for (i = 0; i < versionStr.Length; i++)
            {
                switch (versionStr[i])
                {
                    case '.':
                        // Parse the part of the string before this dot into an integer
                        versionParts[dotCount] = int.Parse(versionStr.Substring(sectionStartOffset, i - sectionStartOffset));
                        sectionStartOffset = i + 1;
                        dotCount++;

                        // If we have 3 dots, we have seen all we can, so collect up and parse
                        if (dotCount == 3)
                        {
                            int revision = int.Parse(versionStr.Substring(i + 1));
                            return new PowerShellVersion(versionParts[0], versionParts[1], versionParts[2], revision);
                        }
                        continue;

                    case '-':
                        if (dotCount > 2)
                        {
                            throw new ArgumentException($"Semantic version string '{versionStr}' contains too many dot separators to be a v2 semantic version");
                        }

                        versionParts[dotCount] = int.Parse(versionStr.Substring(sectionStartOffset, i - sectionStartOffset));
                        string label = versionStr.Substring(i + 1);
                        return new PowerShellVersion(versionParts[0], versionParts[1], versionParts[2], label);
                }
            }

            if (dotCount == 0)
            {
                var majorVersion = int.Parse(versionStr);
                return new PowerShellVersion(majorVersion, -1, -1, label: null);
            }

            versionParts[dotCount] = int.Parse(versionStr.Substring(sectionStartOffset, i - sectionStartOffset));

            return new PowerShellVersion(versionParts[0], versionParts[1], versionParts[2], label: null);
        }

        /// <summary>
        /// Attempt to parse a string as a version.
        /// </summary>
        /// <param name="versionStr">The string to parse.</param>
        /// <param name="version">The parsed version, if successful.</param>
        /// <returns>True if the string was successfully parsed, false otherwise.</returns>
        public static bool TryParse(string versionStr, out PowerShellVersion version)
        {
            try
            {
                version = Parse(versionStr);
                return true;
            }
            catch
            {
                version = null;
                return false;
            }
        }

        /// <summary>
        /// Validates direct version arguments for a PowerShellVersion.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="build">The build version number.</param>
        /// <param name="revision">The revision version number. Cannot be positive when the prerelease label is given.</param>
        /// <param name="preReleaseLabel">The prerelease label. Cannot be non-null when the revision version number is non-negative.</param>
        public static void ValidateVersionArguments(int major, int minor, int build, int revision, string preReleaseLabel)
        {
            if (major < 0)
            {
                throw new ArgumentException();
            }

            if (minor < 0 && (build >= 0 || revision >= 0))
            {
                throw new ArgumentException();
            }

            if (build < 0 && revision >= 0)
            {
                throw new ArgumentException();
            }

            if (revision >= 0 && preReleaseLabel != null)
            {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// Explicit conversion from a PowerShellVersion to a System.Version.
        /// Will fail if the PowerShellVersion has a pre-release label.
        /// </summary>
        /// <param name="psVersion">The PowerShellVersion object to convert.</param>
        public static explicit operator Version(PowerShellVersion psVersion)
        {
            if (psVersion.PreReleaseLabel != null)
            {
                throw new InvalidCastException($"Cannot convert version '{psVersion}' to System.Version, since there is a pre-release label");
            }

            if (psVersion.Revision >= 0)
            {
                return new Version(psVersion.Major, psVersion.Minor, psVersion.Patch, psVersion.Revision);
            }

            if (psVersion.Patch >= 0)
            {
                return new Version(psVersion.Major, psVersion.Minor, psVersion.Patch);
            }

            return new Version(psVersion.Major, psVersion.Minor >= 0 ? psVersion.Minor : 0);
        }

        /// <summary>
        /// Explicit conversion to a PowerShellVersion to a string,
        /// allows PowerShell to cast from a string to a PowerShellVersion.
        /// </summary>
        /// <param name="versionString"></param>
        public static explicit operator PowerShellVersion(string versionString)
        {
            return PowerShellVersion.Parse(versionString);
        }

        /// <summary>
        /// Explicit conversion from a System.Version to a PowerShellVersion,
        /// for simpler casting in PowerShell.
        /// </summary>
        /// <param name="version"></param>
        public static explicit operator PowerShellVersion(Version version)
        {
            return new PowerShellVersion(version.Major, version.Minor, version.Build, version.Revision);
        }

        /// <summary>
        /// Create a new PowerShellVersion from four version numbers.
        /// </summary>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="build"></param>
        /// <param name="revision"></param>
        public PowerShellVersion(int major, int minor, int build, int revision)
        {
            ValidateVersionArguments(major, minor, build, revision, preReleaseLabel: null);
            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
        }

        /// <summary>
        /// Create a new PowerShellVersion from three version numbers and a build label.
        /// </summary>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="patch"></param>
        /// <param name="label"></param>
        public PowerShellVersion(int major, int minor, int patch, string label)
        {
            ValidateVersionArguments(major, minor, patch, -1, label);
            Major = major;
            Minor = minor;
            Build = patch;

            int plusIdx = label?.IndexOf('+') ?? -1;
            if (plusIdx < 0)
            {
                PreReleaseLabel = label;
            }
            else
            {
                PreReleaseLabel = label.Substring(0, plusIdx);
                BuildLabel = label.Substring(plusIdx + 1, label.Length - plusIdx - 1);
            }
        }

        /// <summary>
        /// The major version number.
        /// </summary>
        public int Major { get; } = -1;

        /// <summary>
        /// The minor version number.
        /// </summary>
        public int Minor { get; } = -1;

        /// <summary>
        /// The build version number.
        /// </summary>
        public int Build { get; } = -1;

        /// <summary>
        /// The patch version number, an alias of the build for compatibility with SemanticVersion.
        /// </summary>
        public int Patch => Build;

        /// <summary>
        /// The semver v1 revision version number.
        /// Mutually exclusive with the PreReleaseLabel.
        /// </summary>
        /// <value></value>
        public int Revision { get; } = -1;

        /// <summary>
        /// The semver v2 prerelease label.
        /// Mutually exclusive with the Revision version number.
        /// </summary>
        public string PreReleaseLabel { get; }

        /// <summary>
        /// The build label, as specified after the '+' in the PreReleaseLabel.
        /// </summary>
        public string BuildLabel { get; }

        /// <summary>
        /// True if this represents a semver v2, false otherwise.
        /// </summary>
        public bool IsSemVer => Revision < 0;

        /// <summary>
        /// Renders a PowerShellVersion as a version string.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder().Append(Major);

            if (Minor < 0)
            {
                return sb.ToString();
            }

            sb.Append('.').Append(Minor);

            if (Build < 0)
            {
                return sb.ToString();
            }

            sb.Append('.').Append(Build);

            if (Revision >= 0)
            {
                sb.Append('.').Append(Revision);
            }
            else if (PreReleaseLabel != null)
            {
                sb.Append('-').Append(PreReleaseLabel);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// A JsonConverter for PowerShellVersions that performs serialization
    /// and deserialization to version strings.
    /// </summary>
    public class PowerShellVersionJsonConverter : JsonConverter
    {
        /// <summary>
        /// Determines whether a given type can be converted to or from
        /// a PowerShellVersion.
        /// </summary>
        /// <param name="objectType">The type to assess for conversion.</param>
        /// <returns>True if the type can be converted, false otherwise.</returns>
        public override bool CanConvert(Type objectType)
        {
            return 
                objectType == typeof(Version)
                || objectType == typeof(PowerShellVersion)
                || objectType.FullName == "System.Management.Automation.SemanticVersion";
        }

        /// <summary>
        /// Read a PowerShellVersion object from a JSON string representing a version.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;

            if (s == null)
            {
                return null;
            }

            if (objectType == typeof(Version))
            {
                return (Version)PowerShellVersion.Parse(s);
            }

            return PowerShellVersion.Parse(s);
        }

        /// <summary>
        /// Serialize a PowerShellVersion (or SemanticVersion) object to a string for JSON.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
