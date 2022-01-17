using System;
using SIO.Domain.GoogleSynthesizes.Events;
using SIO.Infrastructure.Domain;

namespace SIO.Domain.GoogleSynthesizes.Aggregates
{
    public sealed class GoogleSynthesize : Aggregate<GoogleSynthesizeState>
    {
        public GoogleSynthesize(GoogleSynthesizeState state) : base(state)
        {
            Handles<GoogleSynthesizeQueued>(Handle);
            Handles<GoogleSynthesizeFailed>(Handle);
            Handles<GoogleSynthesizeSucceded>(Handle);
            Handles<GoogleSynthesizeProcessQueued>(Handle);
            Handles<GoogleSynthesizeProcessSucceeded>(Handle);
        }

        public override GoogleSynthesizeState GetState() => new GoogleSynthesizeState(_state);

        public void Queue(string subject,
            DateTimeOffset? publicationDate,
            string eventSubject)
        {
            Apply(new GoogleSynthesizeQueued(
                subject: subject,
                version: Version + 1,
                publicationDate: publicationDate,
                documentSubject: eventSubject
            ));
        }

        public void Fail(string error)
        {
            Apply(new GoogleSynthesizeFailed(
                error: error,
                subject: Id,
                version: Version + 1,
                documentSubject: _state.DocumentSubject
            ));
        }

        public void Succeed()
        {
            Apply(new GoogleSynthesizeSucceded(
                subject: Id,
                version: Version + 1
            ));
        }

        public void QueueProcess(int order, string processSubject)
        {
            Apply(new GoogleSynthesizeProcessQueued(
                subject: Id,
                version: Version + 1,
                order: order,
                processSubject: processSubject
            ));
        }

        public void SucceedProcess(string processSubject)
        {
            Apply(new GoogleSynthesizeProcessSucceeded(
                subject: Id,
                version: Version + 1,
                processSubject: processSubject
            ));
        }

        private void Handle(GoogleSynthesizeQueued @event)
        {
            Id = @event.Subject;
            _state.Attempts = 0;
            _state.Status = GoogleSynthesizeStatus.Queued;
            _state.DocumentSubject = @event.DocumentSubject;
            Version = @event.Version;
        }

        private void Handle(GoogleSynthesizeFailed @event)
        {
            _state.Attempts++;
            _state.Status = GoogleSynthesizeStatus.Failed;
            Version = @event.Version;
        }

        private void Handle(GoogleSynthesizeSucceded @event)
        {
            _state.Attempts++;
            _state.Status = GoogleSynthesizeStatus.Succeeded;
            Version = @event.Version;
        }

        private void Handle(GoogleSynthesizeProcessQueued @event)
        {
            _state.SynthesizeProcesses.Add(new SynthesizeProcess(@event.Order, @event.Subject, false));
            Version = @event.Version;
        }

        private void Handle(GoogleSynthesizeProcessSucceeded @event)
        {
            var process = _state.SynthesizeProcesses.First(sp => sp.Subject == @event.ProcessSubject);

            _state.SynthesizeProcesses.Remove(process);
            _state.SynthesizeProcesses.Add(new SynthesizeProcess(process.Order, process.Subject, true));

            Version = @event.Version;
        }
    }
}
