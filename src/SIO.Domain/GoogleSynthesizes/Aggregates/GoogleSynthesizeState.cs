using System;
using SIO.Infrastructure.Domain;

namespace SIO.Domain.GoogleSynthesizes.Aggregates
{
    public sealed class GoogleSynthesizeState : IAggregateState
    {
        public int Attempts { get; set; }
        public GoogleSynthesizeStatus Status {  get; set; }
        public string? DocumentSubject { get; set; }
        public SortedSet<SynthesizeProcess> SynthesizeProcesses { get; set; }

        public GoogleSynthesizeState() 
        {
            SynthesizeProcesses = new(new SynthesizeComparer());
        }
        public GoogleSynthesizeState(GoogleSynthesizeState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            Attempts = state.Attempts;
            Status = state.Status;
            DocumentSubject = state.DocumentSubject;
            SynthesizeProcesses = state.SynthesizeProcesses;
        }
    }
}
