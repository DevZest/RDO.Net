﻿using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data.Annotations
{
    [CrossReference(typeof(_ValidatorAttribute))]
    [ModelDeclarationSpec(false, typeof(DataValidationError), typeof(DataRow))]
    public sealed class ValidatorAttribute : ModelDeclarationAttribute, IValidatorAttribute
    {
        private sealed class Validator : IValidator
        {
            public Validator(ValidatorAttribute validatorAttribute, Model model, IColumns sourceColumns)
            {
                _validatorAttribute = validatorAttribute;
                Model = model;
                SourceColumns = sourceColumns ?? Columns.Empty;
            }

            private readonly ValidatorAttribute _validatorAttribute;

            public Model Model { get; }

            public IColumns SourceColumns { get; }

            public IValidatorAttribute Attribute => _validatorAttribute;

            public DataValidationError Validate(DataRow dataRow)
            {
                return _validatorAttribute._validatorFunc(Model, dataRow);
            }
        }

        public ValidatorAttribute(string name)
            : base(name)
        {
        }

        public string[] SourceColumns { get; set; }

        private Func<Model, DataRow, DataValidationError> _validatorFunc;
        protected override void Initialize()
        {
            var methodInfo = GetMethodInfo(new Type[] { typeof(DataRow) }, typeof(DataValidationError));
            _validatorFunc = BuildValidatorFunc(methodInfo);
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

        protected override void Wireup(Model model)
        {
            model.Validators.Add(new Validator(this, model, GetSourceColumns(model)));
        }

        private IColumns GetSourceColumns(Model model)
        {
            var result = Columns.Empty;

            if (SourceColumns == null || SourceColumns.Length == 0)
                return result;

            var columns = model.Columns;
            for (int i = 0; i < SourceColumns.Length; i++)
            {
                var sourceColumn = columns[SourceColumns[i]];
                result = result.Add(sourceColumn);
            }

            return result;
        }
    }
}
