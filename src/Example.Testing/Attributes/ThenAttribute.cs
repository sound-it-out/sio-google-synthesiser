using System;
using Xunit;
using Xunit.Sdk;

namespace Example.Testing.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    [TraitDiscoverer("Example.Testing.Discoverers.ThenTraitDiscoverer", "Example.Testing")]
    public class ThenAttribute : FactAttribute, ITraitAttribute { }
}
