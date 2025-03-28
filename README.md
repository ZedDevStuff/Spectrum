# Spectrum

A simple and lightweight modloader for .NET games and apps.

## Installation

### NuGet

_Not published yet_
```bash
dotnet add package Spectrum
```

### Manual

Download the relevant archive in the releases section and extract it to your project's directory then add all dlls as dependencies.

## Usage

### Creating a mod

```cs
using Spectrum;
using Spectrum.Attributes;

[ModInfo("My Mod", "com.me.mymod", "Me", "1.0.0")]
public class MyMod : SimpleMod
{
    public override void Loaded(SpectrumLoader loader)
    {
        base.Loaded(loader);
        Console.WriteLine($"Loaded {Name} v{Version} by {Author}");
    }
}

```

### Loading mods

```cs
using Spectrum;

// Somewhere in your code
SpectrumLoader<SimpleMod> loader = SpectrumLoader.CreateDefault();
loader.PreloadMods("path/to/your/mod/folder");
loader.ModLoaded += (mod) => {
    // Do something with the loaded mods
};
loader.LoadMods();

```

### Adding dependencies

```cs
using Spectrum;
using Spectrum.Attributes;

[ModInfo("My Mod", "com.me.mymod", "Me", "1.0.0")]
[ModDependency("com.me.othermod", ">1.0.0")]
public class MyMod : SimpleMod
{
    public override void Loaded(SpectrumLoader loader)
    {
        base.Loaded(loader);
        Console.WriteLine($"Loaded {Name} v{Version} by {Author}");
    }
}
```

## Features

- Custom mod types: You can create your own mod types by implementing `IMod`
- Dependency management: Spectrum will look for dependencies after preloading mods and load them in the correct order if they are present.
- Semver compliant versioning: Spectrum uses Semver for versioning mods. It also supports version ranges for dependencies.

## Version Ranges

Spectrum uses 2 different formats for dependencies' version ranges:

`>=1.0.0` means any version equal or above 1.0.0

`(1.0.0, 2.0.0]` means any version above 1.0.0 and below or equal to 2.0.0

## Custom Mod Types

You can create your own mod types by implementing `IMod`. Here is an example of a custom mod type (implementation similar to `SimpleMod`):
```cs
using MyGame;
using Spectrum;

namespace MyModdingAPI;

public abstract class MyModType : IMod
{
    private ModInfo _modInfo;

    public string Name => _modInfo.Name;
    public string Version => _modInfo.Version;
    public string Author => _modInfo.Author;
    public string ModID => _modInfo.ModID;

    public void Loaded(SpectrumLoader loader)
    {
        _modInfo = loader.GetModInfo(this);
        Init();
    }

    public virtual void Init()
    {
        MyGameAPI.SayHello();
    }
}
```

Then you can use it like this:
```cs
using MyModdingAPI;
using Spectrum;
using Spectrum.Attributes;

[ModInfo("My Mod", "com.me.mymod", "Me", "1.0.0")]
public class MyMod : MyModType
{
    public override void Init()
    {
        base.Init();
    }
}
```

