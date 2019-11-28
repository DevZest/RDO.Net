using DevZest.Data.Annotations;
using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using Microsoft.CodeAnalysis;
using System.Collections;

namespace DevZest.Data.Tools
{
    partial class ComputationWindow
    {
        private sealed class Presenter : SimplePresenter
        {
            private readonly Scalar<string> _name;
            private readonly Scalar<string> _description;
            private readonly Scalar<ComputationMode?> _mode;

            public Presenter(ModelMapper modelMapper, ComputationWindow window)
            {
                _window = window;

                _name = NewScalar("ComputeXxx").AddValidator(Extensions.ValidateRequired).AddValidator(modelMapper.ValidateIdentifier);
                _description = NewScalar<string>();
                _mode = NewScalar<ComputationMode?>();

                Show(_window._view);
            }

            private readonly ComputationWindow _window;

            public void Execute(AddComputationDelegate addDbTable)
            {
                addDbTable(_name.Value, _description.Value, _mode.Value);
            }

            private IEnumerable ModeSelection
            {
                get
                {
                    yield return new { Value = default(ComputationMode?), Display = string.Empty };
                    yield return new { Value = new ComputationMode?(ComputationMode.Aggregate), Display = UserMessages.ComputationWindow_Mode_Aggregate };
                    yield return new { Value = new ComputationMode?(ComputationMode.Inherit), Display = UserMessages.ComputationWindow_Mode_Inherit };
                }
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                builder
                    .AddBinding(_window._textBoxName, _name.BindToTextBox())
                    .AddBinding(_window._textBoxDescription, _description.BindToTextBox())
                    .AddBinding(_window._comboBoxMode, _mode.BindToComboBox(ModeSelection));
            }
        }
    }
}
