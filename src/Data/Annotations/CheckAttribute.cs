using DevZest.Data.Annotations.Primitives;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    public sealed class CheckAttribute : NamedModelAttribute
    {
        private sealed class Validator : IValidator
        {
            public Validator(CheckAttribute checkAttribute, _Boolean condition)
            {
                _checkAttribute = checkAttribute;
                _condition = condition;
            }

            private readonly CheckAttribute _checkAttribute;
            private readonly _Boolean _condition;

            DataValidationError IValidator.Validate(DataRow dataRow)
            {
                return IsValid(dataRow) ? null : new DataValidationError(_checkAttribute.GetMessage(), GetValidationSource());
            }

            private bool IsValid(DataRow dataRow)
            {
                return _condition[dataRow] != false;
            }

            private IColumns GetValidationSource()
            {
                var expression = _condition.Expression;
                return expression == null ? Columns.Empty : expression.BaseColumns;
            }
        }

        public CheckAttribute(string name, string message)
            : base(name)
        {
            message.VerifyNotEmpty(nameof(message));
            _message = message;
        }

        public CheckAttribute(string name, Type messageResourceType, string message)
            : base(name)
        {
            _messageResourceType = messageResourceType.VerifyNotNull(nameof(messageResourceType));
            _messageGetter = messageResourceType.ResolveStaticGetter<string>(message.VerifyNotEmpty(nameof(message)));
        }

        private readonly string _message;
        public string Message
        {
            get { return _message; }
        }

        private readonly Type _messageResourceType;
        public Type ResourceType
        {
            get { return _messageResourceType; }
        }

        private readonly Func<string> _messageGetter;

        private string GetMessage()
        {
            return _messageGetter != null ? _messageGetter() : _message;
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

        protected override void Wireup(Model model)
        {
            var condition = GetCondition(model);
            model.DbCheck(Name, Description, condition);
            model.Validators.Add(new Validator(this, condition));
        }

    }
}
