using SIO.Infrastructure.Events;

namespace SIO.Domain.GoogleSynthesizes.Events
{
    public class GoogleSynthesizeSucceded : Event
    {
        public GoogleSynthesizeSucceded(string subject, int version) : base(subject, version)
        {
        }
    }
}
