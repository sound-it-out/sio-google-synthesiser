using System;
using SIO.Infrastructure.Events;

namespace SIO.Domain.GoogleSynthesizes.Events
{
    public class GoogleSynthesizeStarted : Event
    {
        public GoogleSynthesizeStarted(string subject, int version) : base(subject, version)
        {
        }
    }
}
