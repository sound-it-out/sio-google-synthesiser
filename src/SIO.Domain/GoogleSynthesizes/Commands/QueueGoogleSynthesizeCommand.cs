using System;
using SIO.Infrastructure;
using SIO.Infrastructure.Commands;

namespace SIO.Domain.GoogleSynthesizes.Commands
{
    internal class QueueGoogleSynthesizeCommand : Command
    {
        public DateTimeOffset? PublicationDate { get; }
        public string EventSubject { get; }
        public QueueGoogleSynthesizeCommand(string subject,
            CorrelationId? correlationId,
            int version,
            Actor actor,
            DateTimeOffset? publicationDate,
            string eventSubject) : base(subject, correlationId, version, actor)
        {
            PublicationDate = publicationDate;
            EventSubject = eventSubject;
        }
    }
}
