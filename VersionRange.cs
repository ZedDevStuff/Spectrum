using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum;

public struct VersionRange
{
    public Version MinVersion { get; set; }
    public Version MaxVersion { get; set; }
    public bool IncludeMinVersion { get; set; }
    public bool IncludeMaxVersion { get; set; }

    public VersionRange(Version minVersion, Version maxVersion, bool includeMinVersion = true, bool includeMaxVersion = false)
    {
        MinVersion = minVersion;
        MaxVersion = maxVersion;
        IncludeMinVersion = includeMinVersion;
        IncludeMaxVersion = includeMaxVersion;
    }

    public readonly bool Contains(Version version)
    {
        return IncludeMinVersion && version == MinVersion || (IncludeMaxVersion && version == MaxVersion || version > MinVersion && version < MaxVersion);
    }

    public static VersionRange Parse(string input)
    {
        Version minVersion = Version.Min;
        Version maxVersion = Version.Max;
        bool includeMinVersion = true;
        bool includeMaxVersion = true;
        if (!(input.StartsWith('[') || input.StartsWith('(')) && !(input.EndsWith(']') || input.EndsWith(')')))
        {
            if (input.StartsWith(">="))
            {
                minVersion = Version.Parse(input[2..]);
            }
            else if (input.StartsWith('>'))
            {
                minVersion = Version.Parse(input[1..]);
                includeMinVersion = false;
            }
            else if (input.StartsWith("<="))
            {
                maxVersion = Version.Parse(input[2..]);
            }
            else if (input.StartsWith('<'))
            {
                maxVersion = Version.Parse(input[1..]);
                includeMaxVersion = false;
            }
            else
            {
                minVersion = Version.Parse(input);
                maxVersion = minVersion;
                includeMinVersion = true;
                includeMaxVersion = true;
            }
        }
        else
        {
            if (input.StartsWith('['))
                includeMinVersion = true;
            else if (input.StartsWith('('))
                includeMinVersion = false;
            if (input.EndsWith(']'))
                includeMaxVersion = true;
            else if (input.EndsWith(')'))
                includeMaxVersion = false;
            var versions = input[1..^1].Split(',');
            if (versions.Length != 2)
            {
                throw new FormatException("Invalid version range format.");
            }
            minVersion = Version.Parse(versions[0]);
            maxVersion = Version.Parse(versions[1]);
        }
        return new VersionRange(minVersion, maxVersion, includeMinVersion, includeMaxVersion);
    }
    public static bool TryParse(string input, out VersionRange range)
    {
        try
        {
            range = Parse(input);
            return true;
        }
        catch
        {
            range = default;
            return false;
        }
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is VersionRange range && MinVersion == range.MinVersion && MaxVersion == range.MaxVersion && IncludeMinVersion == range.IncludeMinVersion && IncludeMaxVersion == range.IncludeMaxVersion;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(MinVersion, MaxVersion, IncludeMinVersion, IncludeMaxVersion);
    }

    public override readonly string ToString()
    {
        return $"{(IncludeMinVersion ? "[" : "(")}{MinVersion}, {MaxVersion}{(IncludeMaxVersion ? "]" : ")")}";
    }

    public static bool operator ==(VersionRange left, VersionRange right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(VersionRange left, VersionRange right)
    {
        return !(left == right);
    }
}
