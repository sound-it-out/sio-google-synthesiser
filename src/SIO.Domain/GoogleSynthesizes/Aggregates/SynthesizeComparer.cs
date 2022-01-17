namespace SIO.Domain.GoogleSynthesizes.Aggregates
{
    internal class SynthesizeComparer : IComparer<SynthesizeProcess>
    {
        public int Compare(SynthesizeProcess x, SynthesizeProcess y) => x.Order.CompareTo(y.Order);
    }
}
