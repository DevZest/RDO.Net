using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace DevZest.Data.Primitives
{
    public abstract class ColumnValidationAttribute : ColumnAttribute, IColumnValidationFactory
    {
        private sealed class ColumnValidation : DataValidation
        {
            internal ColumnValidation(string name, Column column, Func<Column, DataRow, bool> isValid, Func<Column, DataRow, string> getErrorMessage)
                : base(name)
            {
                Debug.Assert(column != null);
                Debug.Assert(isValid != null);
                Debug.Assert(getErrorMessage != null);

                _column = column;
                _isValid = isValid;
                _getErrorMessage = getErrorMessage;
            }

            private Column _column;
            private Func<Column, DataRow, bool> _isValid;
            private Func<Column, DataRow, string> _getErrorMessage;


            public override int ColumnCount
            {
                get { return 1; }
            }

            public override Column this[int index]
            {
                get
                {
                    if (index != 0)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    return _column;
                }
            }

            public override bool IsValid(DataRow dataRow)
            {
                return _isValid(_column, dataRow);
            }

            public override string GetErrorMessage(DataRow dataRow)
            {
                return _getErrorMessage(_column, dataRow);
            }
        }

        protected abstract string ValidationName { get; }

        protected abstract Func<Column, DataRow, bool> IsValidFunc { get; }

        protected abstract Func<Column, DataRow, string> DefaultErrorMessageFunc { get; }

        public string ErrorMessage { get; set; }

        public Type ErrorMessageFuncType { get; private set; }

        public string ErrorMessageFuncName { get; private set; }

        private Func<Column, DataRow, string> _customErrorMessageFunc;
        private Func<Column, DataRow, string> CustomErrorMessageFunc
        {
            get
            {
                if (ErrorMessageFuncType == null && ErrorMessageFuncName == null)
                    return null;

                if (_customErrorMessageFunc == null)
                    _customErrorMessageFunc = GetCustomErrorMessageFunc(ErrorMessageFuncType, ErrorMessageFuncName);

                return _customErrorMessageFunc;
            }
        }

        private static Func<Column, DataRow, string> GetCustomErrorMessageFunc(Type funcType, string funcName)
        {
            if (!(funcType != null && funcName != null))
                throw new InvalidOperationException(Strings.ColumnValidationAttribute_InvalidErrorMessageFunc(funcType, funcName));

            try
            {
                return funcType.GetErrorMessageFunc(funcName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Strings.ColumnValidationAttribute_InvalidErrorMessageFunc(funcType, funcName), ex);
            }
        }

        Func<Column, DataRow, string> ErrorMessageFunc
        {
            get
            {
                var customErrorMessageFunc = CustomErrorMessageFunc;
                if (customErrorMessageFunc != null)
                    return customErrorMessageFunc;

                if (ErrorMessage != null)
                    return (Column column, DataRow dataRow) => ErrorMessage;

                return DefaultErrorMessageFunc;
            }
        }

        IEnumerable<DataValidation> IColumnValidationFactory.GetValidations(Column column)
        {
            yield return new ColumnValidation(ValidationName, column, IsValidFunc, ErrorMessageFunc);
        }
    }
}
