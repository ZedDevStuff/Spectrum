using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Spectrum;

public partial struct Version
{
    public static Regex SemverRegex { get; } = new Regex(@"(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?");

    private string _versionString = "";
    public int Major { get; private set; }
    public int Minor { get; private set; }
    public int Patch { get; private set; }
    public string PreRelease { get; private set; }
    public string Metadata { get; private set; }

    public Version(int major, int minor, int patch, string? prerelease = null, string? metadata = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = prerelease ?? "";
        Metadata = metadata ?? "";
    }

    public static Version Parse(string input)
    {
        var match = SemverRegex.Match(input);
        if (!match.Success)
        {
            throw new FormatException("Input string was not in a correct format.");
        }
        var major = int.Parse(match.Groups["major"].Value);
        var minor = int.Parse(match.Groups["minor"].Value);
        var patch = int.Parse(match.Groups["patch"].Value);
        var prerelease = match.Groups["prerelease"].Value;
        var metadata = match.Groups["buildmetadata"].Value;
        return new Version(major, minor, patch, prerelease, metadata) { _versionString = input };
    }
    public static bool TryParse(string input, out Version result)
    {
        var match = SemverRegex.Match(input);
        if (!match.Success)
        {
            result = default;
            return false;
        }
        var major = int.Parse(match.Groups["major"].Value);
        var minor = int.Parse(match.Groups["minor"].Value);
        var patch = int.Parse(match.Groups["patch"].Value);
        var prerelease = match.Groups["prerelease"].Value;
        var metadata = match.Groups["buildmetadata"].Value;
        result = new Version(major, minor, patch, prerelease, metadata) { _versionString = input };
        return true;
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Version version && Equals(version);
    }

    public readonly bool Equals(Version other)
    {
        return Major == other.Major && Minor == other.Minor && Patch == other.Patch && PreRelease == other.PreRelease && Metadata == other.Metadata;
    }

    public static bool operator ==(Version left, Version right) => left.Equals(right);
    public static bool operator !=(Version left, Version right) => !left.Equals(right);

    private readonly int CompareTo(Version right)
    {
        if (Major != right.Major)
        {
            return Major.CompareTo(right.Major);
        }
        if (Minor != right.Minor)
        {
            return Minor.CompareTo(right.Minor);
        }
        if (Patch != right.Patch)
        {
            return Patch.CompareTo(right.Patch);
        }
        if (PreRelease != right.PreRelease)
        {
            if (PreRelease == null)
            {
                return 1;
            }
            if (right.PreRelease == null)
            {
                return -1;
            }
            var leftParts = PreRelease.Split('.');
            var rightParts = right.PreRelease.Split('.');
            for (int i = 0; i < Math.Max(leftParts.Length, rightParts.Length); i++)
            {
                if (i >= leftParts.Length)
                {
                    return -1;
                }
                if (i >= rightParts.Length)
                {
                    return 1;
                }
                if (int.TryParse(leftParts[i], out var leftInt) && int.TryParse(rightParts[i], out var rightInt))
                {
                    var comparison = leftInt.CompareTo(rightInt);
                    if (comparison != 0)
                    {
                        return comparison;
                    }
                }
                else
                {
                    var comparison = string.Compare(leftParts[i], rightParts[i], StringComparison.Ordinal);
                    if (comparison != 0)
                    {
                        return comparison;
                    }
                }
            }
        }
        return 0;
    }

    public static bool operator <(Version left, Version right) => left.CompareTo(right) < 0;
    public static bool operator <=(Version left, Version right) => left.CompareTo(right) <= 0;
    public static bool operator >(Version left, Version right) => left.CompareTo(right) > 0;
    public static bool operator >=(Version left, Version right) => left.CompareTo(right) >= 0;

    public override readonly int GetHashCode() => HashCode.Combine(Major, Minor, Patch, PreRelease, Metadata);

    public override string ToString()
    {
        return _versionString ??= $"{Major}.{Minor}.{Patch}{(PreRelease != null ? $"-{PreRelease}" : "")}{(Metadata != null ? $"+{Metadata}" : "")}";
    }

    public static Version Max => new(int.MaxValue, int.MaxValue, int.MaxValue);
    public static Version Min => new(0, 0, 0);
}
