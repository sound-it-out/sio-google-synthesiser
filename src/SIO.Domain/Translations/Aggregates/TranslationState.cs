using SIO.Infrastructure.Domain;

namespace SIO.Domain.Translations.Aggregates
{
    public sealed class TranslationState : IAggregateState
    {
        public string DocumentSubject { get; set; }
        public long CharacterCount { get; set; }
        public long CharacterProcessed { get; set; }

        public TranslationState()
        {

        }

        public TranslationState(TranslationState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            DocumentSubject = state.DocumentSubject;
            CharacterCount = state.CharacterCount;
            CharacterProcessed = state.CharacterProcessed;
        }
    }
}
