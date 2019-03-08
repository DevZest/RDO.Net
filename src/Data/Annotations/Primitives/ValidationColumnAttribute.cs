using System;
using System.Diagnostics;
using System.Globalization;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class ValidationColumnAttribute : ColumnAttribute, IValidatorAttribute
    {
        private sealed class Validator : IValidator
        {
            internal Validator(ValidationColumnAttribute owner, Column column)
            {
                Debug.Assert(owner != null);
                Debug.Assert(column != null);

                _owner = owner;
                _column = column;
            }

            private readonly ValidationColumnAttribute _owner;
            private readonly Column _column;

            public IValidatorAttribute Attribute => _owner;

            public Model Model => _column.ParentModel;

            public DataValidationError Validate(DataRow dataRow)
            {
                return _owner.Validate(_column, dataRow);
            }
        }

        private DataValidationError Validate(Column column, DataRow dataRow)
        {
            return IsValid(column, dataRow) ? null : new DataValidationError(FormatMessage(column.DisplayName), column);
        }

        protected abstract bool IsValid(Column column, DataRow dataRow);

        public string Message { get; set; }

        public Type MessageResourceType { get; set; }

        protected string MessageString
        {
            get
            {
                var messageGetter = MessageGetter;
                if (messageGetter != null)
                    return messageGetter();

                if (Message != null)
                    return Message;

                return DefaultMessageString;
            }
        }

        protected virtual string FormatMessage(string columnDisplayName)
        {
            return string.Format(CultureInfo.CurrentCulture, MessageString, columnDisplayName);
        }

        protected abstract string DefaultMessageString { get; }

        private Func<string> _messageGetter;
        private Func<string> MessageGetter
        {
            get
            {
                if (MessageResourceType == null)
                    return null;

                if (_messageGetter == null)
                    _messageGetter = MessageResourceType.ResolveStaticGetter<string>(Message);

                return _messageGetter;
            }
        }

        protected override void Wireup(Column column)
        {
            var model = column.ParentModel;
            model.Validators.Add(new Validator(this, column));
        }
    }
}
