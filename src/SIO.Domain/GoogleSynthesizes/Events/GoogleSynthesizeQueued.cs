using System;
using SIO.Infrastructure.Events;

namespace SIO.Domain.GoogleSynthesizes.Events
{
    public class GoogleSynthesizeQueued : Event
    {
        public DateTimeOffset? PublicationDate { get; }
        public string DocumentSubject { get; }

        public GoogleSynthesizeQueued(string subject, int version, DateTimeOffset? publicationDate, string documentSubject) : base(subject, version)
        {
            PublicationDate = publicationDate;
            DocumentSubject = documentSubject;
        }
    }
}
