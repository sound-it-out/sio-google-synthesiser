using SIO.Infrastructure;
using SIO.Infrastructure.Commands;

namespace SIO.Domain.Translations.Commands
{
    public class StartTranslationCommand : Command
    {
        public StartTranslationCommand(string subject, CorrelationId? correlationId, int version, Actor actor) : base(subject, correlationId, version, actor)
        {
        }
    }
}
