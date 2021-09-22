using SIO.Infrastructure.Events;

namespace SIO.Domain.Translations.Events
{
    public class TranslationFailed : Event
    {
        public string Error { get; set; }

        public TranslationFailed(string subject, int version, string error) : base(subject, version)
        {
            Error = error;
        }
    }
}
