using System.Security;
using System.Windows.Controls;

namespace DevZest.Data.Presenters
{
    partial class BindingFactory
    {
        /// <summary>
        /// Binds <see cref="SecureString"/> column to <see cref="PasswordBox"/>.
        /// </summary>
        /// <param name="source">The source column.</param>
        /// <returns>The row binding objedct.</returns>
        public static RowBinding<PasswordBox> BindToPasswordBox(this Column<SecureString> source)
        {
            return new RowBinding<PasswordBox>(onRefresh: null).WithInput(PasswordBox.PasswordChangedEvent, PasswordBox.LostFocusEvent, source, v => v.SecurePassword);
        }

        /// <summary>
        /// Binds string column to <see cref="PasswordBox"/>.
        /// </summary>
        /// <param name="source">The source column.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<PasswordBox> BindToPasswordBox(this Column<string> source)
        {
            return new RowBinding<PasswordBox>(onRefresh: (v, p) =>
            {
                var password = p.GetValue(source);
                if (v.Password != password) // PasswordBox.Password is not a dependency property, update only when value changed.
                    v.Password = password;
            }).WithInput(PasswordBox.PasswordChangedEvent, PasswordBox.LostFocusEvent, source, v => v.Password);
        }

        /// <summary>
        /// Binds string scalar data to <see cref="PasswordBox"/>.
        /// </summary>
        /// <param name="source">The source scalar data.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<PasswordBox> BindToPasswordBox(this Scalar<string> source)
        {
            return new ScalarBinding<PasswordBox>(onRefresh: (v, p) =>
            {
                var password = source.GetValue();
                if (v.Password != password) // PasswordBox.Password is not a dependency property, update only when value changed.
                    v.Password = password;
            }).WithInput(PasswordBox.PasswordChangedEvent, PasswordBox.LostFocusEvent, source, v => v.Password);
        }
    }
}
