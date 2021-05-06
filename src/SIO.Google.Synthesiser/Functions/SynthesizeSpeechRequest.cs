namespace SIO.Google.Synthesiser.Functions
{
    public sealed class SynthesizeSpeechRequest
    {
        public string StreamId { get; set; }
        public int Sequence { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public string UserId { get; set; }
        public int Version { get; set; }
        public int Delay { get; set; }
    }
}
