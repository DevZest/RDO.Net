using DevZest.Data.Annotations.Primitives;
using System;
using System.Linq;

namespace DevZest.Data.Annotations
{
    [CrossReference(typeof(_UniqueConstraintAttribute))]
    [Implementation(true, typeof(ColumnSort[]))]
    public sealed class UniqueConstraintAttribute : DbIndexBaseAttribute
    {
        private sealed class Validator : IValidator
        {
            public Validator(UniqueConstraintAttribute uniqueAttribute, ColumnSort[] sortOrder)
            {
                _uniqueAttribute = uniqueAttribute;
                _columns = Columns.Empty;
                for (int i = 0; i < sortOrder.Length; i++)
                    _columns = _columns.Add(sortOrder[i].Column);
                _columns.Seal();
            }

            private readonly UniqueConstraintAttribute _uniqueAttribute;
            private readonly IColumns _columns;

            public DataValidationError Validate(DataRow dataRow)
            {
                return IsValid(dataRow) ? null : new DataValidationError(_uniqueAttribute.GetMessage(_columns), _columns);
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
                foreach (var column in _columns)
                {
                    if (column.Compare(x, y) != 0)
                        return false;
                }

                return true;
            }
        }

        public UniqueConstraintAttribute(string name)
            : base(name)
        {
        }

        public UniqueConstraintAttribute(string name, string message)
            : base(name)
        {
            message.VerifyNotEmpty(nameof(message));
            _message = message;
        }

        public UniqueConstraintAttribute(string name, Type messageResourceType, string message)
            : base(name)
        {
            _messageResourceType = messageResourceType.VerifyNotNull(nameof(messageResourceType));
            _messageGetter = messageResourceType.ResolveStaticGetter<string>(message.VerifyNotEmpty(nameof(message)));
        }

        private readonly string _message;
        public string Message
        {
            get { return _message; }
        }

        private readonly Type _messageResourceType;
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

        protected override void Wireup(Model model, string dbName, ColumnSort[] sortOrder)
        {
            model.DbUnique(dbName, Description, IsCluster, sortOrder);
            model.Validators.Add(new Validator(this, sortOrder));
        }
    }
}
