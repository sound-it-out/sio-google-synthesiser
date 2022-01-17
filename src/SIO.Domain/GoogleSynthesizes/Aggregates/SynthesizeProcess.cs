namespace SIO.Domain.GoogleSynthesizes.Aggregates
{
    public struct SynthesizeProcess : IEquatable<SynthesizeProcess>
    {
        public readonly int Order;
        public readonly string Subject;
        public readonly bool Processed;

        public SynthesizeProcess(int order, string subject, bool processed)
        {
            Order = order;
            Subject = subject;
            Processed = processed;
        }

        public bool Equals(SynthesizeProcess other) => this.Subject == other.Subject;
    }
}
