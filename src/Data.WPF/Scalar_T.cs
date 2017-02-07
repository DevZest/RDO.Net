namespace DevZest.Data.Windows
{
    public sealed class Scalar<T>
    {
        public Scalar()
        {
        }

        public Scalar(T value)
        {
            Value = value;
        }

        public T Value { get; set; }
    }
}
