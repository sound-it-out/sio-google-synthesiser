using SIO.Infrastructure.Events;

namespace SIO.Domain.Translations.Events
{
    public class TranslationSynthesiseQueued : Event
    {
        public string FileName { get; set; }
        public int ProcessedVersion { get; set; }

        public TranslationSynthesiseQueued(int processedVersion, string fileName, string subject, int version) : base(subject, version)
        {
            ProcessedVersion = processedVersion;
            FileName = fileName;
        }
    }
}
