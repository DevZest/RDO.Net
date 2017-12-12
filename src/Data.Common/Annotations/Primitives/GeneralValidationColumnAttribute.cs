using DevZest.Data.Utilities;
using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class GeneralValidationColumnAttribute : ValidationColumnAttribute
    {
        protected sealed override IColumnValidationMessages Validate(Column column, DataRow dataRow)
        {
            return IsValid(column, dataRow) ? ColumnValidationMessages.Empty : new ColumnValidationMessage(Severity, GetMessage(column, dataRow), column);
        }

        protected abstract bool IsValid(Column column, DataRow dataRow);

        protected virtual ValidationSeverity Severity
        {
            get { return ValidationSeverity.Error; }
        }

        public string Message { get; set; }

        public Type ResourceType { get; set; }

        private string GetMessage(Column column, DataRow dataRow)
        {
            var messageFunc = MessageGetter;
            if (messageFunc != null)
                return messageFunc(column, dataRow);

            if (Message != null)
                return Message;

            return GetDefaultMessage(column, dataRow);
        }

        protected abstract string GetDefaultMessage(Column column, DataRow dataRow);

        private Func<Column, DataRow, string> _messageGetter;
        private Func<Column, DataRow, string> MessageGetter
        {
            get
            {
                if (ResourceType == null)
                    return null;

                if (_messageGetter == null)
                    _messageGetter = GetMessageGetter(ResourceType, Message);

                return _messageGetter;
            }
        }

#if DEBUG
        internal    // For unit test
#else
        private
#endif
        static Func<Column, DataRow, string> GetMessageGetter(Type funcType, string funcName)
        {
            Debug.Assert(funcType != null);
            if (string.IsNullOrWhiteSpace(funcName))
                throw new InvalidOperationException(Strings.GeneralValidationColumnAttribute_InvalidMessageFunc(funcType, funcName));

            try
            {
                return GetMessageFunc(funcType, funcName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Strings.GeneralValidationColumnAttribute_InvalidMessageFunc(funcType, funcName), ex);
            }
        }

        internal static Func<Column, DataRow, string> GetMessageFunc(Type funcType, string funcName)
        {
            Debug.Assert(funcType != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(funcName));

            var methodInfo = funcType.GetStaticMethodInfo(funcName);
            var paramColumn = Expression.Parameter(typeof(Column), methodInfo.GetParameters()[0].Name);
            var paramDataRow = Expression.Parameter(typeof(DataRow), methodInfo.GetParameters()[1].Name);
            var call = Expression.Call(methodInfo, paramColumn, paramDataRow);
            return Expression.Lambda<Func<Column, DataRow, string>>(call, paramColumn, paramDataRow).Compile();
        }
    }
}
