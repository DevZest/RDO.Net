using DevZest.Data.Utilities;
using System;
using System.Diagnostics;
using System.Globalization;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class ValidationColumnAttribute : ColumnAttribute
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

            private ValidationColumnAttribute _owner;
            private Column _column;

            public ColumnValidationMessage Validate(DataRow dataRow)
            {
                return _owner.Validate(_column, dataRow);
            }
        }

        private ColumnValidationMessage Validate(Column column, DataRow dataRow)
        {
            return IsValid(column, dataRow) ? null : new ColumnValidationMessage(Severity, FormatMessage(column.DisplayName), column);
        }

        protected abstract bool IsValid(Column column, DataRow dataRow);

        protected virtual ValidationSeverity Severity
        {
            get { return ValidationSeverity.Error; }
        }

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
                    _messageGetter = MessageResourceType.ResolveStringGetter(Message);

                return _messageGetter;
            }
        }

        protected override void Initialize(Column column)
        {
            var model = column.ParentModel;
            model.Validators.Add(new Validator(this, column));
        }
    }
}
