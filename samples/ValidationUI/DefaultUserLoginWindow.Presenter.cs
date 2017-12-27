﻿using DevZest.Data.Presenters;

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
                    .GridRows("Auto", "Auto", "Auto", "Auto")
                    .AddBinding(0, 0, _.EmailAddress.BindToLabel(emailAddressBinding)).AddBinding(1, 0, emailAddressBinding)
                    .AddBinding(1, 1, _.BindToTextBlock(@"(Required, must be a valid email address)"))
                    .AddBinding(0, 2, _.Password.BindToLabel(passwordBinding)).AddBinding(1, 2, passwordBinding)
                    .AddBinding(1, 3, _.BindToTextBlock(@"(Required, at least 6 characters long)"));
            }
        }
    }
}
