using DevZest.Data.Annotations.Primitives;
using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ModelValidatorAttribute : ValidationModelWireupAttribute
    {
        protected override void Initialize(Type modelType, MemberInfo memberInfo)
        {
            _validatorFunc = BuildValidatorFunc(modelType, memberInfo);
        }

        private Func<Model, DataRow, DataValidationError> BuildValidatorFunc(Type modelType, MemberInfo memberInfo)
        {
            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo == null)
                return null;

            var paramModel = Expression.Parameter(typeof(Model));
            var paramDataRow = Expression.Parameter(typeof(DataRow));

            var model = Expression.Convert(paramModel, modelType);
            var call = Expression.Call(model, methodInfo, paramDataRow);

            return Expression.Lambda<Func<Model, DataRow, DataValidationError>>(call, paramModel, paramDataRow).Compile();
        }

        protected override ModelWireupEvent WireupEvent
        {
            get { return ModelWireupEvent.Initialized; }
        }

        protected override Action<Model> WireupAction
        {
            get
            {
                if (_validatorFunc == null)
                    return null;
                else
                    return AddValidator;
            }
        }

        private Func<Model, DataRow, DataValidationError> _validatorFunc;
        protected override DataValidationError Validate(Model model, DataRow dataRow)
        {
            Debug.Assert(_validatorFunc != null);
            return _validatorFunc(model, dataRow);
        }
    }
}
