using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies the custom validator declaration of the model.
    /// </summary>
    [CrossReference(typeof(_CustomValidatorAttribute))]
    [ModelDeclarationSpec(true, typeof(CustomValidatorEntry))]
    public sealed class CustomValidatorAttribute : ModelDeclarationAttribute, IValidatorAttribute
    {
        private sealed class Validator : IValidator
        {
            public Validator(CustomValidatorAttribute customValidatorAttribute, Model model)
            {
                _customValidatorAttribute = customValidatorAttribute;
                Model = model;
                var entry = customValidatorAttribute._entryGetter(model);
                _validate = entry.Validate;
                SourceColumns = entry.GetSourceColumns()?.Seal();
            }

            private readonly CustomValidatorAttribute _customValidatorAttribute;

            public Model Model { get; }

            private readonly Func<DataRow, string> _validate;

            public IColumns SourceColumns { get; }

            public IValidatorAttribute Attribute => _customValidatorAttribute;

            public DataValidationError Validate(DataRow dataRow)
            {
                var message = _validate(dataRow);
                return string.IsNullOrEmpty(message) ? null : new DataValidationError(message, SourceColumns);
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CustomValidatorAttribute"/>.
        /// </summary>
        /// <param name="name">The name of the custom validator.</param>
        public CustomValidatorAttribute(string name)
            : base(name)
        {
        }

        private Func<Model, CustomValidatorEntry> _entryGetter;
        /// <inheritdoc />
        protected override void Initialize()
        {
            var getMethod = GetPropertyGetter(typeof(CustomValidatorEntry));
            _entryGetter = BuildEntryGetter(ModelType, getMethod);
        }

        private static Func<Model, CustomValidatorEntry> BuildEntryGetter(Type modelType, MethodInfo getMethod)
        {
            var paramModel = Expression.Parameter(typeof(Model));
            var model = Expression.Convert(paramModel, modelType);
            var call = Expression.Call(model, getMethod);
            return Expression.Lambda<Func<Model, CustomValidatorEntry>>(call, paramModel).Compile();
        }

        /// <inheritdoc />
        protected override ModelWireupEvent WireupEvent
        {
            get { return ModelWireupEvent.Initialized; }
        }

        /// <inheritdoc />
        protected override void Wireup(Model model)
        {
            model.Validators.Add(new Validator(this, model));
        }
    }
}
