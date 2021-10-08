using SIO.Infrastructure.Events;

namespace SIO.Domain.Translations.Events
{
    public class TranslationQueued : Event
    {
        public TranslationQueued(string subject, int version) : base(subject, version)
        {
        }
    }
}
