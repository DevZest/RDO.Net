using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Tools
{
    partial class DbInitInputWindow
    {
        private sealed class Presenter : DataPresenter<DbInitInput>
        {
            private sealed class TextBoxVisibility : RowBindingBehavior<TextBox>
            {
                public TextBoxVisibility(LocalColumn<bool> isPassword)
                {
                    _isPassword = isPassword;
                }

                private readonly LocalColumn<bool> _isPassword;

                protected override void Setup(TextBox view, RowPresenter presenter)
                {
                    view.Visibility = presenter.GetValue(_isPassword) ? Visibility.Collapsed : Visibility.Visible;
                }

                protected override void Refresh(TextBox view, RowPresenter presenter)
                {
                }

                protected override void Cleanup(TextBox view, RowPresenter presenter)
                {
                }
            }

            private sealed class PasswordBoxVisibility : RowBindingBehavior<PasswordBox>
            {
                public PasswordBoxVisibility(LocalColumn<bool> isPassword)
                {
                    _isPassword = isPassword;
                }

                private readonly LocalColumn<bool> _isPassword;

                protected override void Setup(PasswordBox view, RowPresenter presenter)
                {
                    view.Visibility = presenter.GetValue(_isPassword) ? Visibility.Visible : Visibility.Collapsed;
                }

                protected override void Refresh(PasswordBox view, RowPresenter presenter)
                {
                }

                protected override void Cleanup(PasswordBox view, RowPresenter presenter)
                {
                }
            }

            public Presenter(DbInitInputWindow window, DataSet<DbInitInput> input)
            {
                Show(window._dataView, input);
            }

            private static string ValidateNotEmpty(TypeSymbolEntry dbInitType)
            {
                return dbInitType.IsDefault ? UserMessages.Validation_Required : null;
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder.GridRows("Auto", "Auto")
                    .GridColumns("*")
                    .Layout(Orientation.Vertical)
                    .AddBinding(0, 0, _.Title.BindToTextBlock())
                    .AddBinding(0, 1, _.Value.BindToTextBox().AddBehavior(new TextBoxVisibility(_.IsPassword)))
                    .AddBinding(0, 1, _.Value.BindToPasswordBox().AddBehavior(new PasswordBoxVisibility(_.IsPassword)));
            }
        }
    }
}
