using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class ValidationColumnGroupAttribute : ColumnGroupAttribute
    {
        protected ValidationColumnGroupAttribute(string name)
            : base(name)
        {
        }

        public string Message { get; set; }

        public Type ResourceType { get; set; }

        internal string GetMessage(IReadOnlyList<Column> columns, DataRow dataRow)
        {
            var messageFunc = MessageFunc;
            if (messageFunc != null)
                return messageFunc(columns, dataRow);

            if (Message != null)
                return Message;

            return GetDefaultMessage(columns, dataRow);
        }

        private Func<IReadOnlyList<Column>, DataRow, string> _messageFunc;
        private Func<IReadOnlyList<Column>, DataRow, string> MessageFunc
        {
            get
            {
                if (ResourceType == null)
                    return null;

                if (_messageFunc == null)
                    _messageFunc = GetMessageGetter(ResourceType, Message);

                return _messageFunc;
            }
        }

#if DEBUG
    internal // For unit test
#else
    private
#endif
        static Func<IReadOnlyList<Column>, DataRow, string> GetMessageGetter(Type funcType, string funcName)
        {
            if (string.IsNullOrWhiteSpace(funcName))
                throw new InvalidOperationException(Strings.ValidationColumnGroupAttribute_InvalidMessageFunc(funcType, funcName));

            try
            {
                return GetMessageFunc(funcType, funcName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Strings.ValidationColumnGroupAttribute_InvalidMessageFunc(funcType, funcName), ex);
            }
        }

        internal static Func<IReadOnlyList<Column>, DataRow, string> GetMessageFunc(Type funcType, string funcName)
        {
            Debug.Assert(funcType != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(funcName));

            var methodInfo = funcType.GetStaticMethodInfo(funcName);
            var paramColumns = Expression.Parameter(typeof(IReadOnlyList<Column>), methodInfo.GetParameters()[0].Name);
            var paramDataRow = Expression.Parameter(typeof(DataRow), methodInfo.GetParameters()[1].Name);
            var call = Expression.Call(methodInfo, paramColumns, paramDataRow);
            return Expression.Lambda<Func<IReadOnlyList<Column>, DataRow, string>>(call, paramColumns, paramDataRow).Compile();
        }

        protected abstract string GetDefaultMessage(IReadOnlyList<Column> columns, DataRow dataRow);
    }
}
