using DevZest.Data;
using DevZest.Data.Primitives;
using System;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public class FlushErrorMessage : ValidationMessageBase<UIElement>
    {
        public FlushErrorMessage(string id, string description, UIElement source)
            : base(id, ValidationSeverity.Error, description, source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
        }
    }
}
