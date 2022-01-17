using SIO.Infrastructure.Events;

namespace SIO.Domain.GoogleSynthesizes.Events
{
    public class GoogleSynthesizeProcessSucceeded : Event
    {
        public string ProcessSubject { get; set; }

        public GoogleSynthesizeProcessSucceeded(string subject, int version, string processSubject) : base(subject, version)
        {
            ProcessSubject = processSubject;
        }
    }
}
