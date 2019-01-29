using System;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.PowerShell.CrossCompatibility.Utility
{
    public class PowerShellVersion
    {
        public static PowerShellVersion Create(dynamic versionInput)
        {
            switch (versionInput)
            {
                case Version systemVersion:
                    return new PowerShellVersion(systemVersion);

                case string versionString:
                    return Parse(versionString);
            }

            return new PowerShellVersion(versionInput.Major, versionInput.Minor, versionInput.Patch, versionInput.PreReleaseLabel);
        }

        public static PowerShellVersion Parse(string versionStr)
        {
            if (versionStr == null)
            {
                throw new ArgumentNullException(nameof(versionStr));
            }

            int[] versionParts = new int[3];

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
                throw new ArgumentException($"Version string '{versionStr}' must contain at least one dot separator");
            }

            versionParts[dotCount] = int.Parse(versionStr.Substring(sectionStartOffset, i - sectionStartOffset));

            return new PowerShellVersion(versionParts[0], versionParts[1], versionParts[2], preReleaseLabel: null);
        }

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

        public static explicit operator Version(PowerShellVersion psVersion)
        {
            if (psVersion.PreReleaseLabel != null)
            {
                throw new InvalidCastException($"Cannot convert version '{psVersion}' to System.Version, since there is a pre-release label");
            }

            if (psVersion.Revision != null)
            {
                return new Version(psVersion.Major, psVersion.Minor, psVersion.Patch, psVersion.Revision.Value);
            }

            return new Version(psVersion.Major, psVersion.Minor, psVersion.Patch);
        }

        public static explicit operator PowerShellVersion(string versionString)
        {
            return PowerShellVersion.Parse(versionString);
        }

        public PowerShellVersion(Version version)
            : this(version.Major, version.Minor, version.Build, version.Revision)
        {
        }

        public PowerShellVersion(int major, int minor, int build, int revision)
        {
            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
        }

        public PowerShellVersion(int major, int minor, int patch, string preReleaseLabel)
        {
            Major = major;
            Minor = minor;
            Build = patch;
            PreReleaseLabel = preReleaseLabel;
        }

        public int Major { get; }

        public int Minor { get; }

        public int Build { get; }

        public int Patch => Build;

        public int? Revision { get; }

        public string PreReleaseLabel { get; }

        public bool IsSemVer => Revision == null;

        public override string ToString()
        {
            if (!IsSemVer)
            {
                return $"{Major}.{Minor}.{Build}.{Revision}";
            }

            var sb = new StringBuilder()
                .Append(Major).Append('.')
                .Append(Minor).Append('.')
                .Append(Patch);

            if (!string.IsNullOrEmpty(PreReleaseLabel))
            {
                sb.Append('-').Append(PreReleaseLabel);
            }

            return sb.ToString();
        }
    }

    public class PowerShellVersionJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PowerShellVersion)
                || objectType == typeof(Version)
                || objectType.FullName == "System.Management.Automation.SemanticVersion";
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            return PowerShellVersion.Parse(s);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() != typeof(string))
            {
                writer.WriteValue(value.ToString());
                return;
            }

            writer.WriteValue(PowerShellVersion.Create(value).ToString());
        }
    }
}