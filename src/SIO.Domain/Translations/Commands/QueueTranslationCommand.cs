using SIO.Infrastructure;
using SIO.Infrastructure.Commands;
using SIO.Infrastructure.Events;

namespace SIO.Domain.Translations.Commands
{
    public class QueueTranslationCommand : Command
    {
        public string FileName { get; }

        public QueueTranslationCommand(string fileName,
            string subject,            
            CorrelationId? correlationId,
            int version,
            Actor actor) : base(subject, correlationId, version, actor)
        {
            FileName = fileName;
        }
    }
}
