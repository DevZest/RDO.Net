namespace DevZest.Data.Windows.Primitives
{
    internal struct Clip
    {
        public static readonly Clip Empty = new Clip(0, 0);

        public readonly double Head;
        public readonly double Tail;

        public Clip(double head, double tail)
        {
            Head = head;
            Tail = tail;
        }

        public bool IsEmpty
        {
            get { return Head == 0 && Tail == 0; }
        }
    }
}
