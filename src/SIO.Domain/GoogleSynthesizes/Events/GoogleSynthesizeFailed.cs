using SIO.Infrastructure.Events;

namespace SIO.Domain.GoogleSynthesizes.Events
{
    public class GoogleSynthesizeFailed : Event
    {
        public string Error { get; }
        public string DocumentSubject { get; set; }

        public GoogleSynthesizeFailed(string error, string subject, int version, string documentSubject) : base(subject, version)
        {
            Error = error;
            DocumentSubject = subject;
        }
    }
}
