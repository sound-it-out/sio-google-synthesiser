using SIO.Infrastructure.Events;

namespace SIO.Domain.GoogleSynthesizes.Events
{
    public class GoogleSynthesizeProcessQueued : Event
    {
        public string ProcessSubject { get; set; }
        public int Order { get; set; }

        public GoogleSynthesizeProcessQueued(string subject, int version, int order, string processSubject) : base(subject, version)
        {
            Order = order;
            ProcessSubject = processSubject;
        }
    }
}
