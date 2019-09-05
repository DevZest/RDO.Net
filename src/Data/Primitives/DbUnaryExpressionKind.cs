namespace DevZest.Data.Primitives
{
    /// <summary>Represents operation of unary expression.</summary>
    public enum DbUnaryExpressionKind
    {
        /// <summary>
        /// The logic NOT operation.
        /// </summary>
        Not,

        /// <summary>
        /// The negate operation.
        /// </summary>
        Negate,

        /// <summary>
        /// Bitwise one's complement operation.
        /// </summary>
        OnesComplement
    }
}
