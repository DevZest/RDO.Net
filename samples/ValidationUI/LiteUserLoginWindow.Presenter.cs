using DevZest.Data.Presenters;

namespace ValidationUI
{
    partial class LiteUserLoginWindow
    {
        private sealed class Presenter : DataPresenter<User>
        {
            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var emailAddressBinding = _.EmailAddress.AsTextBox();
                var passwordBinding = _.Password.AsPasswordBox();
                builder
                    .WithRowValidationMode(ValidationMode.Implicit)
                    .GridColumns("Auto", "*", "20")
                    .GridRows("Auto", "Auto")
                    .AddBinding(0, 0, _.EmailAddress.AsLabel(emailAddressBinding)).AddBinding(1, 0, emailAddressBinding).AddBinding(2, 0, _.EmailAddress.AsValidationSeverityIndicator())
                    .AddBinding(0, 1, _.Password.AsLabel(passwordBinding)).AddBinding(1, 1, passwordBinding).AddBinding(2, 1, _.Password.AsValidationSeverityIndicator());
            }
        }
    }
}
