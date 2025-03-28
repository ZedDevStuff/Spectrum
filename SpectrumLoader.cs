using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Mono.Cecil;

using Spectrum.Attributes;

namespace Spectrum;

public class SpectrumLoader
{
    /// <summary>
    /// Creates a new instance of SpectrumLoader with the default mod type.
    /// </summary>
    /// <param name="crashOnDependencyError">Whether the loader should crash if a dependency error happens. If false, the loader will ignore affected mods.</param>
    /// <returns>The loader</returns>
    public static SpectrumLoader<SimpleMod> CreateDefault(bool crashOnDependencyError = true)
    {
        return new SpectrumLoader<SimpleMod>(crashOnDependencyError);
    }
    public virtual ModInfo? GetModInfo(IMod mod) { return null; }
}
public class SpectrumLoader<TModType> : SpectrumLoader where TModType : class, IMod
{
    private readonly bool _crashOnDependencyError = true;
    private Dictionary<string, (ModInfo info, FileInfo assembly, string modType)> _mods = new();
    private readonly Dictionary<string, List<ModDependency>> _dependencies = new();
    private readonly Dictionary<string, TModType> _loadedMods = new();
    private readonly Dictionary<IMod, ModInfo> _modInfos = new();

    public event Action<TModType>? ModLoaded;

    /// <summary>
    /// Creates a new instance of SpectrumLoader.
    /// </summary>
    /// <param name="crashOnDependencyError">Whether the loader should crash if a dependency error happens. If false, the loader will ignore affected mods.</param>
    public SpectrumLoader(bool crashOnDependencyError = true)
    {
        _crashOnDependencyError = crashOnDependencyError;
    }
    /// <summary>
    /// Finds and preloads mods from the specified directory.
    /// </summary>
    /// <param name="directory"></param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public void PreloadMods(string directory)
    {
        DirectoryInfo dir = new(directory);
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"The directory {directory} does not exist.");

        FileInfo[] files = dir.GetFiles("*.dll", SearchOption.AllDirectories);
        foreach (FileInfo file in files)
        {
            if (IsModAssembly(file))
            {
                PreloadAssembly(file);
            }
        }
        CheckDependencies();
        SortByDependencies();
    }

    public override ModInfo? GetModInfo(IMod mod) => _modInfos.GetValueOrDefault(mod);

    /// <summary>
    /// Loads all ready-to-load mods.
    /// </summary>
    public void LoadMods() => LoadModsInternal();

    private static bool IsModAssembly(string path) => IsModAssembly(new FileInfo(path));
    private static bool IsModAssembly(FileInfo file)
    {
        try
        {
            if (!file.Exists)
                return false;

            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(file.FullName);
            return assembly.Modules
                .SelectMany(module => module.GetTypes())
                .Any(type => type.CustomAttributes.Any(a => a.AttributeType.FullName == typeof(ModInfo).FullName));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while checking if {file.FullName} is a mod assembly: {e.Message}");
            return false;
        }
    }

    private void PreloadAssembly(FileInfo file)
    {
        AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(file.FullName);
        string modid = "";
        var types = assembly.Modules.SelectMany(module => module.GetTypes());
        foreach (TypeDefinition type in types)
        {
            var modInfoAttributes = type.CustomAttributes.Where(attr => attr.AttributeType.FullName == typeof(ModInfo).FullName);
            if(modInfoAttributes.Count() > 1 || !modInfoAttributes.Any()) continue;
            CustomAttribute modInfoType = modInfoAttributes.First();
            string name = modInfoType.ConstructorArguments[0].Value.ToString()!;
            modid = modInfoType.ConstructorArguments[1].Value.ToString()!;
            string author = modInfoType.ConstructorArguments[2].Value.ToString()!;
            string version = modInfoType.ConstructorArguments[3].Value.ToString()!;
            ModInfo info = new(name, modid, author, version);
            if(_mods.TryGetValue(modid, out (ModInfo info, FileInfo assembly, string modType) value))
            {
                Console.WriteLine($"Duplicate mod id '{modid}' found in '{file.FullName}' and '{value.assembly.FullName}'");
                if (_crashOnDependencyError)
                    Environment.Exit(1);
                else
                    continue;
            }
            _mods.Add(modid, (info, file, $"{type.FullName}, {assembly.Name.FullName}"));

            var dependencies = type.CustomAttributes
                .Where(attr => attr.AttributeType.FullName == typeof(ModDependency).FullName);

            if (dependencies != null && dependencies.Any())
            {
                List<ModDependency> modDependencies = new();
                foreach (CustomAttribute dependency in dependencies)
                {
                    string depModid = dependency.ConstructorArguments[0].Value.ToString()!;
                    string depVersion = dependency.ConstructorArguments[1].Value.ToString()!;
                    modDependencies.Add(new ModDependency(depModid, depVersion));
                }
                this._dependencies.Add(modid, modDependencies);
            }
        }
    }
    
    private void CheckDependencies()
    {
        foreach (var mod in _mods)
        {
            if (_dependencies.TryGetValue(mod.Key, out List<ModDependency>? value))
            {
                foreach (var dependency in value)
                {
                    if (!_mods.ContainsKey(dependency.ModID))
                    {
                        Console.WriteLine($"'{mod.Key}' requires '{dependency.ModID}' but it couldn't be found.");
                        if (_crashOnDependencyError)
                            Environment.Exit(1);
                    }
                    else
                    {
                        if (VersionRange.TryParse(dependency.Version, out VersionRange range))
                        {
                            if(Version.TryParse(_mods[dependency.ModID].info.Version, out Version version))
                            {
                                if (!range.Contains(version))
                                {
                                    Console.WriteLine($"'{mod.Key}' requires '{dependency.ModID}' version '{dependency.Version}' but version '{_mods[dependency.ModID].info.Version}' was found.");
                                    if (_crashOnDependencyError)
                                        Environment.Exit(1);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Invalid version declared in '{dependency.ModID}'");
                                if (_crashOnDependencyError)
                                    Environment.Exit(1);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Invalid dependency version range declared for '{dependency.ModID}' dependency.");
                            if (_crashOnDependencyError)
                                Environment.Exit(1);
                        }
                    }
                }
            }
        }
    }
    
    private void SortByDependencies()
    {
        _mods = _mods.OrderBy(entry => _dependencies.TryGetValue(entry.Key, out List<ModDependency>? value) ? value.Count : 0).OrderBy(entry => _dependencies.TryGetValue(entry.Key, out List<ModDependency>? value) ? value.Sum(dep => _mods.ContainsKey(dep.ModID) ? 1 : 0) : 0).ToDictionary(x => x.Key, y => y.Value);
    }

    private void LoadModsInternal()
    {
        foreach (var entry in _mods)
        {
            try
            {
                if (!AppDomain.CurrentDomain.GetAssemblies().Any(assembly => assembly.Location == entry.Value.assembly.FullName))
                {
                    Assembly.LoadFrom(entry.Value.assembly.FullName);
                }
                TModType mod = (Activator.CreateInstance(Type.GetType(entry.Value.modType)!) as TModType)!;
                _modInfos.Add(mod, entry.Value.info);
                _loadedMods.Add(entry.Key, mod);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while loading mod '{entry.Key}': {e.Message}");
            }
        }
        Console.WriteLine($"Loaded mods {_loadedMods.Count}:");
        foreach (var mod in _loadedMods)
        {
            Console.WriteLine($"- {mod.Key} {GetModInfo(mod.Value)!.Version}");
        }
        Console.WriteLine();
        foreach (var mod in _loadedMods)
        {
            mod.Value.Loaded(this);
        }
    }
}
