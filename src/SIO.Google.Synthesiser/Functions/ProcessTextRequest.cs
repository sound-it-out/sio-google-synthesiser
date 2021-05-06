using SIO.Infrastructure.Events;

namespace SIO.Google.Synthesiser.Functions
{
    internal sealed class ProcessTextRequest
    {
        public string StreamId { get; set; }
        public string Subject { get; set; }
        public string FileName { get; set; }
        public string UserId { get; set; }
        public int Version { get; set; }
    }
}
