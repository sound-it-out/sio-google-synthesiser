using System;
using System.Linq;
using System.Runtime.CompilerServices;
using SIO.Domain.GoogleSynthesizes.Events;
[assembly: InternalsVisibleTo("SIO.Domain.Tests")]
namespace SIO.Domain
{
    public static class EventHelper
    {
        public static Type[] AllEvents = new IntegrationEvents.AllEvents().Concat(new Type[]
        {
            typeof(GoogleSynthesizeQueued),
            typeof(GoogleSynthesizeFailed),
            typeof(GoogleSynthesizeSucceded),
            typeof(GoogleSynthesizeStarted),
            typeof(GoogleSynthesizeProcessQueued),
            typeof(GoogleSynthesizeProcessSucceeded)
        }).ToArray();
    }
}
