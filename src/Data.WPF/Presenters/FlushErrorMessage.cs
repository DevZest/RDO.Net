using DevZest.Data;
using DevZest.Data.Primitives;
using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public class FlushErrorMessage : ValidationMessageBase<UIElement>
    {
        public FlushErrorMessage(FlushError inputError, UIElement source)
            : base(inputError.Id, ValidationSeverity.Error, inputError.Description, source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
        }
    }
}
