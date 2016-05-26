namespace DevZest.Data.Windows.Primitives
{
    internal struct Span
    {
        public readonly double Start;
        public readonly double End;

        public Span(double start, double end)
        {
            Start = start;
            End = end;
        }

        public double Length
        {
            get { return End - Start; }
        }
    }
}
