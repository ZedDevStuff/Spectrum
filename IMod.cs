using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Spectrum.Attributes;

namespace Spectrum;

public interface IMod
{
    void Loaded(SpectrumLoader loader);
}
