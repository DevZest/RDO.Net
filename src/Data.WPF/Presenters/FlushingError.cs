using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public class FlushingError : ValidationError<UIElement>
    {
        public FlushingError(string message, UIElement source)
            : base(message, source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
        }
    }
}
