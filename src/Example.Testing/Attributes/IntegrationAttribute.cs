using System;
using Xunit;
using Xunit.Sdk;

namespace Example.Testing.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    [TraitDiscoverer("Example.Testing.Discoverers.IntegrationTraitDiscoverer", "Example.Testing")]
    public class IntegrationAttribute : FactAttribute, ITraitAttribute { }
}
