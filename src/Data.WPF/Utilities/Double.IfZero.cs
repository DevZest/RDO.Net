namespace DevZest
{
    static partial class Extension
    {
        public static double IfZero(this double value, double ifZeroValue)
        {
            return value == 0 ? ifZeroValue : value;
        }
    }
}
