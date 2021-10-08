using SIO.Infrastructure.Events;

namespace SIO.Domain.Translations.Events
{
    public class TranslationSynthesiseFailed : Event
    {
        public long CharactersProcessed { get; set; }
        public string FileName { get; set; }

        public TranslationSynthesiseFailed(string subject, int version, string fileName, long charactersProcessed) : base(subject, version)
        {
            FileName = fileName;
            CharactersProcessed = charactersProcessed;
        }
    }
}
