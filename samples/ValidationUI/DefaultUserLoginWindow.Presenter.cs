using DevZest.Data.Presenters;

namespace ValidationUI
{
    partial class DefaultUserLoginWindow
    {
        private sealed class Presenter : DataPresenter<User>
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var emailAddressBinding = _.EmailAddress.BindToTextBox();
                var passwordBinding = _.Password.BindToPasswordBox();
                builder
                    .GridColumns("Auto", "*")
                    .GridRows("Auto", "Auto")
                    .AddBinding(0, 0, _.EmailAddress.BindToLabel(emailAddressBinding)).AddBinding(1, 0, emailAddressBinding)
                    .AddBinding(0, 1, _.Password.BindToLabel(passwordBinding)).AddBinding(1, 1, passwordBinding);
            }
        }
    }
}
