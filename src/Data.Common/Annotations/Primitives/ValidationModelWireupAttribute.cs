using DevZest.Data.Utilities;
using System.Diagnostics;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class ValidationModelWireupAttribute : ModelWireupAttribute
    {
        private sealed class Validator : IValidator
        {
            internal Validator(ValidationModelWireupAttribute owner, Model model)
            {
                Debug.Assert(owner != null);
                Debug.Assert(model != null);

                _owner = owner;
                _model = model;
            }

            private ValidationModelWireupAttribute _owner;
            private Model _model;

            public ColumnValidationMessage Validate(DataRow dataRow)
            {
                return _owner.Validate(_model, dataRow);
            }
        }

        protected abstract ColumnValidationMessage Validate(Model model, DataRow dataRow);

        protected void AddValidator(Model model)
        {
            Check.NotNull(model, nameof(model));
            model.Validators.Add(new Validator(this, model));
        }
    }
}
