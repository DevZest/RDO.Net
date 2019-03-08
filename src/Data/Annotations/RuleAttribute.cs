using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    [CrossReference(typeof(_RuleAttribute))]
    [ModelDeclarationSpec(true, typeof(Rule))]
    public sealed class RuleAttribute : ModelDeclarationAttribute, IValidatorAttribute
    {
        private sealed class Validator : IValidator
        {
            public Validator(RuleAttribute ruleAttribute, Model model)
            {
                _ruleAttribute = ruleAttribute;
                Model = model;
                var rule = ruleAttribute._ruleGetter(model);
                _validate = rule.Validate;
                SourceColumns = rule.GetSourceColumns()?.Seal();
            }

            private readonly RuleAttribute _ruleAttribute;

            public Model Model { get; }

            private readonly Func<DataRow, string> _validate;

            public IColumns SourceColumns { get; }

            public IValidatorAttribute Attribute => _ruleAttribute;

            public DataValidationError Validate(DataRow dataRow)
            {
                var message = _validate(dataRow);
                return string.IsNullOrEmpty(message) ? null : new DataValidationError(message, SourceColumns);
            }
        }

        public RuleAttribute(string name)
            : base(name)
        {
        }

        private Func<Model, Rule> _ruleGetter;
        protected override void Initialize()
        {
            var getMethod = GetPropertyGetter(typeof(Rule));
            _ruleGetter = BuildRuleGetter(ModelType, getMethod);
        }

        private static Func<Model, Rule> BuildRuleGetter(Type modelType, MethodInfo getMethod)
        {
            var paramModel = Expression.Parameter(typeof(Model));
            var model = Expression.Convert(paramModel, modelType);
            var call = Expression.Call(model, getMethod);
            return Expression.Lambda<Func<Model, Rule>>(call, paramModel).Compile();
        }

        protected override ModelWireupEvent WireupEvent
        {
            get { return ModelWireupEvent.Initialized; }
        }

        protected override void Wireup(Model model)
        {
            model.Validators.Add(new Validator(this, model));
        }
    }
}
