using SIO.Infrastructure.Projections;

namespace SIO.Domain.GoogleSynthesizes.Projections
{
    public class GoogleSynthesizeFailure : IProjection
    {
        public string Subject { get; set; }
        public string DocumentSubject { get; set; }
        public string Error { get; set; }
    }
}
