using DevZest.Data.Annotations.Primitives;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    public sealed class ModelValidatorAttribute : NamedModelAttribute, IValidator
    {
        public ModelValidatorAttribute(string name)
            : base(name)
        {
        }

        private Func<Model, DataRow, DataValidationError> _validatorFunc;
        protected override Action<Model> Initialize()
        {
            var methodInfo = GetMethodInfo(new Type[] { typeof(DataRow) }, typeof(DataValidationError));
            _validatorFunc = BuildValidatorFunc(methodInfo);
            return Wireup;
        }

        private Func<Model, DataRow, DataValidationError> BuildValidatorFunc(MethodInfo methodInfo)
        {
            var paramModel = Expression.Parameter(typeof(Model));
            var paramDataRow = Expression.Parameter(typeof(DataRow));
            var model = Expression.Convert(paramModel, ModelType);
            var call = Expression.Call(model, methodInfo, paramDataRow);

            return Expression.Lambda<Func<Model, DataRow, DataValidationError>>(call, paramModel, paramDataRow).Compile();
        }

        protected override ModelWireupEvent WireupEvent
        {
            get { return ModelWireupEvent.Initialized; }
        }

        private void Wireup(Model model)
        {
            model.Validators.Add(this);
        }

        DataValidationError IValidator.Validate(DataRow dataRow)
        {
            var model = dataRow.Model;
            return _validatorFunc(model, dataRow);
        }
    }
}
