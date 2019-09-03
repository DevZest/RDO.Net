using DevZest.Data.Primitives;

namespace DevZest.Data
{
    /// <summary>
    /// Contains methods for CASE expression
    /// </summary>
    public static class Case
    {
        /// <summary>
        /// Constructs WHEN statement of CASE expression.
        /// </summary>
        /// <param name="when">The condition.</param>
        /// <returns>A <see cref="CaseWhen"/> struct for further expression construct.</returns>
        public static CaseWhen When(_Boolean when)
        {
            return new CaseWhen(when);
        }

        /// <summary>
        /// Constructs ON statement of CASE expression
        /// </summary>
        /// <typeparam name="T">Data type of the column.</typeparam>
        /// <param name="on">The column expression.</param>
        /// <returns>A <see cref="CaseOn{T}"/> struct for further expression construct.</returns>
        public static CaseOn<T> On<T>(Column<T> on)
        {
            on.VerifyNotNull(nameof(on));
            return new CaseOn<T>(on);
        }
    }
}
