namespace DevZest.Data.Windows.Primitives
{
    internal struct Span
    {
        public readonly double StartOffset;
        public readonly double EndOffset;

        public Span(double startOffset, double endOffset)
        {
            StartOffset = startOffset;
            EndOffset = endOffset;
        }

        public double Length
        {
            get { return EndOffset - StartOffset; }
        }
    }
}
