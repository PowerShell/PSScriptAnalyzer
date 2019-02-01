using System;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.PowerShell.CrossCompatibility
{
    public class PowerShellVersion
    {
        public static PowerShellVersion Create(dynamic versionInput)
        {
            switch (versionInput)
            {
                case Version systemVersion:
                    return (PowerShellVersion)systemVersion;

                case string versionString:
                    return Parse(versionString);
            }

            if (versionInput.BuildLabel != null)
            {
                return new PowerShellVersion(versionInput.Major, versionInput.Minor, versionInput.Patch, $"{versionInput.PreReleaseLabel}+{versionInput.BuildLabel}");
            }

            return new PowerShellVersion(versionInput.Major, versionInput.Minor, versionInput.Patch, versionInput.PreReleaseLabel);
        }

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
                throw new ArgumentException($"Version string '{versionStr}' must contain at least one dot separator");
            }

            versionParts[dotCount] = int.Parse(versionStr.Substring(sectionStartOffset, i - sectionStartOffset));

            return new PowerShellVersion(versionParts[0], versionParts[1], versionParts[2], label: null);
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

        public static explicit operator Version(PowerShellVersion psVersion)
        {
            if (psVersion.PreReleaseLabel != null)
            {
                throw new InvalidCastException($"Cannot convert version '{psVersion}' to System.Version, since there is a pre-release label");
            }

            return new Version(psVersion.Major, psVersion.Minor, psVersion.Patch, psVersion.Revision);
        }

        public static explicit operator PowerShellVersion(string versionString)
        {
            return PowerShellVersion.Parse(versionString);
        }

        public static explicit operator PowerShellVersion(Version version)
        {
            return new PowerShellVersion(version.Major, version.Minor, version.Build, version.Revision);
        }

        public PowerShellVersion(int major, int minor, int build, int revision)
        {
            ValidateVersionArguments(major, minor, build, revision, preReleaseLabel: null);
            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
        }

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

        public int Major { get; } = -1;

        public int Minor { get; } = -1;

        public int Build { get; } = -1;

        public int Patch => Build;

        public int Revision { get; } = -1;

        public string PreReleaseLabel { get; }

        public string BuildLabel { get; }

        public bool IsSemVer => Revision < 0;

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

    public class PowerShellVersionJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PowerShellVersion)
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