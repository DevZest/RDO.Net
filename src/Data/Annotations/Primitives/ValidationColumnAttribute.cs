using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Globalization;

namespace DevZest.Data.Annotations.Primitives
{
    /// <summary>
    /// Base class for attribute which validates a column.
    /// </summary>
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

            public IColumns SourceColumns => _column;

            public DataValidationError Validate(DataRow dataRow)
            {
                return _owner.Validate(_column, dataRow);
            }
        }

        private DataValidationError Validate(Column column, DataRow dataRow)
        {
            return IsValid(column, dataRow) ? null : new DataValidationError(FormatMessage(column.DisplayName), column);
        }

        /// <summary>
        /// Determines whether the specified column value is valid.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="dataRow">The data row.</param>
        /// <returns><see langword="true" /> if data value is valid, otherwise <see langword="false" />.</returns>
        protected abstract bool IsValid(Column column, DataRow dataRow);

        /// <summary>
        /// Gets or sets the error message if validation failed.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the resource type for localized error messages.
        /// </summary>
        public Type MessageResourceType { get; set; }

        /// <summary>
        /// Gets the error message string.
        /// </summary>
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

        /// <summary>
        /// Formats the error message for specified column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>The error message.</returns>
        public string FormatMessage(Column column)
        {
            column.VerifyNotNull(nameof(column));
            return FormatMessage(column.DisplayName);
        }

        /// <summary>
        /// Formats the error message for specified column display name.
        /// </summary>
        /// <param name="columnDisplayName">The display name of the column.</param>
        /// <returns>The error message.</returns>
        protected virtual string FormatMessage(string columnDisplayName)
        {
            return string.Format(CultureInfo.CurrentCulture, MessageString, columnDisplayName);
        }

        /// <summary>
        /// Gets the default error message.
        /// </summary>
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

        /// <inheritdoc />
        protected override void Wireup(Column column)
        {
            var model = column.ParentModel;
            model.Validators.Add(new Validator(this, column));
        }
    }
}
