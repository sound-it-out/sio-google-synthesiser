using System;
using SIO.Infrastructure.Domain;
using SIO.IntegrationEvents.Documents;

namespace SIO.Domain.Documents.Aggregates
{
    public class DocumentState : IAggregateState
    {
        public string Subject { get; set; }
        public string User { get; set; }
        public TranslationType TranslationType { get; set; }
        public string TranslationOptionSubject { get; set; }
        public string FileName { get; set; }
        public int Version { get; set; }
        public bool Deleted { get; set; }

        public DocumentState() { }
        public DocumentState(DocumentState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            Subject = state.Subject;
            TranslationOptionSubject = state.TranslationOptionSubject;
            User = state.User;
            FileName = state.FileName;
            Version = state.Version;
        }
    }
}
