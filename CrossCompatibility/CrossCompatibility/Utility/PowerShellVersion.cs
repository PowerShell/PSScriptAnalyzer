using System;
using System.Text;

namespace Microsoft.PowerShell.CrossCompatibility.Utility
{
    public class PowerShellVersion
    {
        public static PowerShellVersion FromSemVer(dynamic semver)
        {
            if (semver.GetType().FullName != "System.Management.Automation.SemanticVersion")
            {
                throw new ArgumentException($"{nameof(semver)} must be of type 'System.Management.Automation.SemanticVersion'");
            }

            return new PowerShellVersion(semver.Major, semver.Minor, semver.Patch, semver.PreReleaseLabel);
        }

        public static PowerShellVersion Parse(string versionStr)
        {
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
}