using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public class FlushError : ValidationError<UIElement>
    {
        public FlushError(string message, UIElement source)
            : base(message, source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
        }
    }
}
