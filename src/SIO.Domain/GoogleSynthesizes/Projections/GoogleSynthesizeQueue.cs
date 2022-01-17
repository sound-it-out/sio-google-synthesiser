using System;
using SIO.Infrastructure.Projections;

namespace SIO.Domain.GoogleSynthesizes.Projections
{
    public class GoogleSynthesizeQueue : IProjection
    {
        public string Subject { get; set; }
        public string DocumentSubject { get; set; }
        public int Attempts { get; set; }  
    }
}
