using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using Microsoft.CodeAnalysis;

namespace DevZest.Data.Tools
{
    partial class CustomValidatorWindow
    {
        private sealed class Presenter : SimplePresenter
        {
            private readonly Scalar<string> _name;
            private readonly Scalar<string> _description;

            public Presenter(ModelMapper modelMapper, CustomValidatorWindow window)
            {
                _window = window;

                _name = NewScalar("VAL_").AddValidator(Extensions.ValidateRequired).AddValidator(modelMapper.ValidateIdentifier);
                _description = NewScalar<string>();

                Show(_window._view);
            }

            private readonly CustomValidatorWindow _window;

            public void Execute(AddCustomValidatorDelegate addValidator)
            {
                addValidator(_name.Value, _description.Value);
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder
                    .AddBinding(_window._textBoxName, _name.BindToTextBox())
                    .AddBinding(_window._textBoxDescription, _description.BindToTextBox());
            }
        }
    }
}
