using SIO.Infrastructure.Events;

namespace SIO.Domain.Translations.Events
{
    public class TranslationFailed : Event
    {
        public string Error { get; set; }

        public TranslationFailed(string subject, int version) : base(subject, version)
        {
        }
    }
}
