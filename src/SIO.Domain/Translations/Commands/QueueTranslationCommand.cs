using SIO.Infrastructure;
using SIO.Infrastructure.Commands;

namespace SIO.Domain.Translations.Commands
{
    public class QueueTranslationCommand : Command
    {
        public string DocumentSubject {  get; set; }

        public QueueTranslationCommand(string subject, CorrelationId? correlationId, int version, Actor actor, string documentSubject) : base(subject, correlationId, version, actor)
        {
            DocumentSubject = documentSubject;
        }
    }
}
