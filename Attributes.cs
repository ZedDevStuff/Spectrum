using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ModInfo : Attribute
{
    public string Name { get; set; }
    public string ModID { get; set; }
    public string Author { get; set; }
    public string Version { get; set; }

    public ModInfo(string name, string modID, string author, string version)
    {
        Name = name;
        ModID = modID;
        Author = author;
        Version = version;
    }
}
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ModDependency : Attribute
{
    /// <summary>
    /// The ID of the mod that is required.
    /// </summary>
    public string ModID { get; set; }
    /// <summary>
    /// The version or version range of the mod that is required.
    /// </summary>
    public string Version { get; set; }

    public ModDependency(string modID, string version)
    {
        ModID = modID;
        Version = version;
    }
}
