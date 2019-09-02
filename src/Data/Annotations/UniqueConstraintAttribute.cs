using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Linq;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies the unique constraint.
    /// </summary>
    [CrossReference(typeof(_UniqueConstraintAttribute))]
    [ModelDeclarationSpec(true, typeof(ColumnSort[]))]
    public sealed class UniqueConstraintAttribute : DbIndexBaseAttribute, IValidatorAttribute
    {
        private sealed class Validator : IValidator
        {
            public Validator(UniqueConstraintAttribute uniqueAttribute, Model model, ColumnSort[] sortOrder)
            {
                _uniqueAttribute = uniqueAttribute;
                Model = model;
                SourceColumns = GetSourceColumns(sortOrder);
            }

            private static IColumns GetSourceColumns(ColumnSort[] sortOrder)
            {
                var result = Columns.Empty;
                for (int i = 0; i < sortOrder.Length; i++)
                    result = result.Add(sortOrder[i].Column);
                return result.Seal();
            }

            private readonly UniqueConstraintAttribute _uniqueAttribute;

            public IValidatorAttribute Attribute => _uniqueAttribute;

            public Model Model { get; }

            public IColumns SourceColumns { get; }

            public DataValidationError Validate(DataRow dataRow)
            {
                return IsValid(dataRow) ? null : new DataValidationError(_uniqueAttribute.GetMessage(SourceColumns), SourceColumns);
            }

            private bool IsValid(DataRow dataRow)
            {
                var model = dataRow.Model;
                var dataSet = model.DataSet;
                foreach (var other in dataSet)
                {
                    if (other == dataRow)
                        continue;
                    if (AreEqual(dataRow, other))
                        return false;
                }
                return true;
            }

            private bool AreEqual(DataRow x, DataRow y)
            {
                foreach (var column in SourceColumns)
                {
                    if (column.Compare(x, y) != 0)
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UniqueConstraintAttribute"/>.
        /// </summary>
        /// <param name="name">The name of the unique constraint.</param>
        public UniqueConstraintAttribute(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UniqueConstraintAttribute"/> with error message.
        /// </summary>
        /// <param name="name">The name of the unique constraint.</param>
        /// <param name="message">The error message for validation.</param>
        public UniqueConstraintAttribute(string name, string message)
            : base(name)
        {
            message.VerifyNotEmpty(nameof(message));
            _message = message;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UniqueConstraintAttribute"/> with localized error message.
        /// </summary>
        /// <param name="name">The name of the unique constraint.</param>
        /// <param name="messageResourceType">The resource type of the localized error message.</param>
        /// <param name="message">The property to retrieve the localized error message.</param>
        public UniqueConstraintAttribute(string name, Type messageResourceType, string message)
            : base(name)
        {
            _messageResourceType = messageResourceType.VerifyNotNull(nameof(messageResourceType));
            _messageGetter = messageResourceType.ResolveStaticGetter<string>(message.VerifyNotEmpty(nameof(message)));
        }

        private readonly string _message;
        /// <summary>
        /// Gets the error message for validation.
        /// </summary>
        public string Message
        {
            get { return _message; }
        }

        private readonly Type _messageResourceType;
        /// <summary>
        /// Gets the resource type for localized error message.
        /// </summary>
        public Type ResourceType
        {
            get { return _messageResourceType; }
        }

        private readonly Func<string> _messageGetter;

        private string GetMessage(IColumns columns)
        {
            var result = _messageGetter != null ? _messageGetter() : _message;
            return string.IsNullOrEmpty(result) ? string.Format(UserMessages.UniqueAttribute, GetDisplayNames(columns)) : result;
        }

        private string GetDisplayNames(IColumns columns)
        {
            if (columns.Count == 0)
                return null;
            else if (columns.Count == 1)
                return columns.Single().DisplayName;
            else
                return string.Format("({0})", string.Join(", ", columns.Select(x => x.DisplayName)));
        }

        /// <inheritdoc />
        protected override void Wireup(Model model, string dbName, ColumnSort[] sortOrder)
        {
            model.AddDbUniqueConstraint(dbName, Description, IsCluster, sortOrder);
            model.Validators.Add(new Validator(this, model, sortOrder));
        }
    }
}
