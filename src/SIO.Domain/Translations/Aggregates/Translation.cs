using SIO.Infrastructure.Domain;
using SIO.IntegrationEvents.Translations;

namespace SIO.Domain.Translations.Aggregates
{
    public sealed class Translation : Aggregate<TranslationState>
    {
        public Translation(TranslationState state) : base(state)
        {
            Handles<TranslationQueued>(Handle);
            Handles<TranslationStarted>(Handle);
            Handles<TranslationCharactersProcessed>(Handle);
            Handles<TranslationSucceded>(Handle);
            Handles<TranslationFailed>(Handle);
        }

        public override TranslationState GetState() => new TranslationState(_state);


        public void Queue(string subject,
            string documentSubject)
        {
            Apply(new TranslationQueued(
                subject: subject,
                version: Version + 1,
                documentSubject: documentSubject
            ));
        }

        public void Start(long characterCount)
        {
            Apply(new TranslationStarted(
                subject: Id,
                version: Version + 1,
                documentSubject: _state.DocumentSubject,
                characterCount: characterCount
            ));
        }

        public void Process(long charactersProcessed)
        {
            Apply(new TranslationCharactersProcessed(
                subject: Id,
                version: Version + 1,
                documentSubject: _state.DocumentSubject,
                charactersProcess: charactersProcessed
            ));
        }

        public void Succeed()
        {
            Apply(new TranslationSucceded(
                subject: Id,
                version: Version + 1,
                documentSubject: _state.DocumentSubject
            ));
        }

        public void Fail(string error)
        {
            Apply(new TranslationFailed(
                subject: Id,
                version: Version + 1,
                documentSubject: _state.DocumentSubject,
                error: error
            ));
        }

        private void Handle(TranslationQueued @event)
        {
            Id = @event.Subject;
            _state.DocumentSubject = @event.DocumentSubject;
            Version = @event.Version;
        }

        private void Handle(TranslationStarted @event)
        {
            _state.CharacterCount = @event.CharacterCount;
            Version = @event.Version;
        }

        private void Handle(TranslationCharactersProcessed @event)
        {
            _state.CharacterProcessed += @event.CharactersProcessed;
            Version = @event.Version;
        }

        private void Handle(TranslationSucceded @event)
        {
            Version = @event.Version;
        }

        private void Handle(TranslationFailed @event)
        {
            Version = @event.Version;
        }        
    }
}
