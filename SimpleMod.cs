using System;

using Spectrum.Attributes;

namespace Spectrum;

public abstract class SimpleMod : IMod
{
    private ModInfo? _modInfo;

    public string Name => _modInfo?.Name ?? "";
    public string ModID => _modInfo?.ModID ?? "";
    public string Author => _modInfo?.Author ?? "";
    public string Version => _modInfo?.Version ?? "";

    public virtual void Loaded(SpectrumLoader loader)
    {
        _modInfo = loader.GetModInfo(this)!;
        Console.WriteLine($"Mod {Name} loaded!");
    }
}
