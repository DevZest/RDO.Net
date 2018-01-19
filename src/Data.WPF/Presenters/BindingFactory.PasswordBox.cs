using System.Security;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    partial class BindingFactory
    {
        public static RowBinding<PasswordBox> BindToPasswordBox(this Column<SecureString> source)
        {
            return new RowBinding<PasswordBox>(onRefresh: null).WithInput(PasswordBox.PasswordChangedEvent, PasswordBox.LostFocusEvent, source, v => v.SecurePassword);
        }

        public static RowBinding<PasswordBox> BindToPasswordBox(this Column<string> source)
        {
            return new RowBinding<PasswordBox>(onRefresh: (v, p) =>
            {
                var password = p.GetValue(source);
                if (v.Password != password) // PasswordBox.Password is not a dependency property, update only when value changed.
                    v.Password = password;
            }).WithInput(PasswordBox.PasswordChangedEvent, PasswordBox.LostFocusEvent, source, v => v.Password);
        }

        public static ScalarBinding<PasswordBox> BindToPasswordBox(this Scalar<string> source)
        {
            return new ScalarBinding<PasswordBox>(onRefresh: (v, p) =>
            {
                var password = source.Value;
                if (v.Password != password) // PasswordBox.Password is not a dependency property, update only when value changed.
                    v.Password = password;
            }).WithInput(PasswordBox.PasswordChangedEvent, PasswordBox.LostFocusEvent, source, v => v.Password);
        }
    }
}
