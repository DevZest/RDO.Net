using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public class FlushErrorMessage : ValidationMessageBase<UIElement>
    {
        public FlushErrorMessage(string description, UIElement source)
            : base(ValidationSeverity.Error, description, source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
        }
    }
}
