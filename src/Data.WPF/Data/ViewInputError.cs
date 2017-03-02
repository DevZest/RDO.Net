using DevZest.Data;
using DevZest.Data.Primitives;
using System;
using System.Windows;

namespace DevZest.Windows.Data
{
    public class ViewInputError : ValidationMessage<UIElement>
    {
        public ViewInputError(InputError inputError, UIElement source)
            : base(inputError.Id, ValidationSeverity.Error, inputError.Description, source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
        }
    }
}
