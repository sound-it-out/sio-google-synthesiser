using SIO.Infrastructure.Events;

namespace SIO.Domain.Translations.Events
{
    public class TranslationStarted : Event
    {
        public long CharacterCount { get; set; }
        public int ProcessCount { get; set; }

        public TranslationStarted(string subject, int version, long characterCount, int processCount) : base(subject, version)
        {
            CharacterCount = characterCount;
            ProcessCount = processCount;
        }
    }
}
