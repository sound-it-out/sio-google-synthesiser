using SIO.Infrastructure;
using SIO.Infrastructure.Commands;

namespace SIO.Domain.Translations.Commands
{
    internal class SynthesizeTextCommand : Command
    {
        public string FileName { get; set; }
        public int ProcessedVersion { get; set; }

        public SynthesizeTextCommand(
            string fileName,
            int processedVersion,
            string subject,
            CorrelationId? correlationId,
            int version,
            Actor actor) : base(subject, correlationId, version, actor)
        {
            FileName = fileName;
            ProcessedVersion = processedVersion;
        }
    }
}
