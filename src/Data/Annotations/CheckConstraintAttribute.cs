using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    [CrossReference(typeof(_CheckConstraintAttribute))]
    [ModelDeclarationSpec(true, typeof(_Boolean))]
    public sealed class CheckConstraintAttribute : ModelDeclarationAttribute, IValidatorAttribute
    {
        private sealed class Validator : IValidator
        {
            public Validator(CheckConstraintAttribute checkAttribute, Model model, _Boolean condition)
            {
                _checkAttribute = checkAttribute;
                Model = model;
                SourceColumns = GetSourceColumns(condition);
                _condition = condition;
            }

            private static IColumns GetSourceColumns(_Boolean condition)
            {
                var expression = condition.Expression;
                return expression == null ? Columns.Empty : expression.BaseColumns;
            }

            private readonly CheckConstraintAttribute _checkAttribute;
            private readonly _Boolean _condition;

            public IValidatorAttribute Attribute => _checkAttribute;

            public Model Model { get; }

            public IColumns SourceColumns { get; }

            DataValidationError IValidator.Validate(DataRow dataRow)
            {
                return IsValid(dataRow) ? null : new DataValidationError(_checkAttribute.GetMessage(), SourceColumns);
            }

            private bool IsValid(DataRow dataRow)
            {
                return _condition[dataRow] != false;
            }
        }

        public CheckConstraintAttribute(string name, string message)
            : base(name)
        {
            message.VerifyNotEmpty(nameof(message));
            Message = message;
        }

        public CheckConstraintAttribute(string name, Type messageResourceType, string message)
            : this(name, message)
        {
            MessageResourceType = messageResourceType.VerifyNotNull(nameof(messageResourceType));
            _messageGetter = messageResourceType.ResolveStaticGetter<string>(message);
        }

        public string Message { get; }

        public Type MessageResourceType { get; }

        private readonly Func<string> _messageGetter;

        private string GetMessage()
        {
            return _messageGetter != null ? _messageGetter() : Message;
        }

        private Func<Model, _Boolean> _conditionGetter;
        protected override void Initialize()
        {
            var getMethod = GetPropertyGetter(typeof(_Boolean));
            _conditionGetter = BuildConditionGetter(ModelType, getMethod);
        }

        private static Func<Model, _Boolean> BuildConditionGetter(Type modelType, MethodInfo getMethod)
        {
            var paramModel = Expression.Parameter(typeof(Model));
            var model = Expression.Convert(paramModel, modelType);
            var call = Expression.Call(model, getMethod);
            return Expression.Lambda<Func<Model, _Boolean>>(call, paramModel).Compile();
        }

        protected override ModelWireupEvent WireupEvent
        {
            get { return ModelWireupEvent.Initializing; }
        }

        private _Boolean GetCondition(Model model)
        {
            return _conditionGetter(model);
        }

        public bool DataSetOnly { get; set; }

        protected override void Wireup(Model model)
        {
            var condition = GetCondition(model);
            if (!DataSetOnly)
                model.AddDbCheckConstraint(Name, Description, condition);
            model.Validators.Add(new Validator(this, model, condition));
        }
    }
}
