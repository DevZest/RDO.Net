using System.Security;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    partial class BindingFactory
    {
        public static RowBinding<PasswordBox> BindToPasswordBox(this Column<SecureString> source)
        {
            return new RowBinding<PasswordBox>(onRefresh: null).WithInput(PasswordBox.PasswordChangedEvent, source, v => v.SecurePassword);
        }

        public static RowBinding<PasswordBox> BindToPasswordBox(this Column<string> source)
        {
            return new RowBinding<PasswordBox>(onRefresh: (v, p) =>
            {
                v.Password = p.GetValue(source);
            }).WithInput(PasswordBox.PasswordChangedEvent, source, v => v.Password);
        }
    }
}
