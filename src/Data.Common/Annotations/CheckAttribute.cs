using DevZest.Data.Annotations.Primitives;
using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CheckAttribute : ModelWireupAttribute
    {
        public string Name { get; set; }

        public string Description { get; set; }

        private Func<Model, _Boolean> _conditionGetter;
        protected internal override void Initialize(Type modelType, MemberInfo memberInfo)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo == null)
                return;

            var methodInfo = propertyInfo.GetGetMethod();
            if (methodInfo == null)
                return;

            var paramModel = Expression.Parameter(typeof(Model));
            var model = Expression.Convert(paramModel, modelType);
            var call = Expression.Call(model, methodInfo);
            var _conditionGetter = Expression.Lambda<Func<Model, _Boolean>>(call, paramModel).Compile();
        }

        protected internal override ModelWireupEvent WireupEvent
        {
            get { return ModelWireupEvent.Initializing; }
        }

        protected internal override Action<Model> WireupAction
        {
            get { return Wireup; }
        }

        private sealed class Validator : IValidator
        {
            public Validator(CheckAttribute checkAttribute, _Boolean condition)
            {
                Debug.Assert(checkAttribute != null);
                Debug.Assert(condition != null);

                _checkAttribute = checkAttribute;
                _condition = condition;
            }

            private readonly CheckAttribute _checkAttribute;
            private readonly _Boolean _condition;

            public IColumnValidationMessages Validate(DataRow dataRow)
            {
                return _condition[dataRow] == false ? new ColumnValidationMessage(ValidationSeverity.Error, null, _condition.Expression.BaseColumns) : ColumnValidationMessages.Empty;
            }
        }

        private void Wireup(Model model)
        {
            var condition = _conditionGetter(model);
            if (condition != null)
            {
                model.DbCheck(Name, Description, condition);
                model.Validators.Add(new Validator(this, condition));
            }
        }
    }
}
