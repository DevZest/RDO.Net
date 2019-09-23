using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents data binding conversion error before model updated reported by flushing validator.
    /// </summary>
    public class FlushingError : ValidationError<UIElement>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FlushingError"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="source">The source of the error.</param>
        public FlushingError(string message, UIElement source)
            : base(message, source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
        }
    }
}
