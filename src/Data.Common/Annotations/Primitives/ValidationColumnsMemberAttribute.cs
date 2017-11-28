using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class ValidationColumnsMemberAttribute : ColumnsMemberAttribute
    {
        protected abstract new class Manager<TValidatorColumnsAttribute, TValidatorColumnsMemberAttribute> : ColumnsMemberAttribute.Manager<TValidatorColumnsAttribute, TValidatorColumnsMemberAttribute>
            where TValidatorColumnsAttribute : ValidationColumnsAttribute
            where TValidatorColumnsMemberAttribute : ValidationColumnsMemberAttribute
        {
            protected interface IValidationContext
            {
                Model Model { get; }
                TValidatorColumnsAttribute ColumnsAttribute { get; }
                IReadOnlyList<Entry> Entries { get; }
                IColumns Columns { get; }
                IReadOnlyList<Column> ColumnList { get; }
            }

            private sealed class Validator : IValidator, IValidationContext
            {
                public Validator(Manager<TValidatorColumnsAttribute, TValidatorColumnsMemberAttribute> manager, Model model, TValidatorColumnsAttribute columnsAttribute, IReadOnlyList<Entry> entries)
                {
                    Debug.Assert(manager != null);
                    Debug.Assert(model != null);
                    Debug.Assert(columnsAttribute != null);
                    Debug.Assert(entries != null && entries.Count > 0);
                    _manager = manager;
                    _model = model;
                    _columnsAttribute = columnsAttribute;
                    _entries = entries;
                    _columnList = new Column[entries.Count];
                    _columns = Data.Columns.Empty;
                    for (int i = 0; i < entries.Count; i++)
                    {
                        var column = entries[i].Column;
                        _columnList[i] = column;
                        _columns = _columns.Add(column);
                    }
                    _columns = _columns.Seal();
                }

                private readonly Manager<TValidatorColumnsAttribute, TValidatorColumnsMemberAttribute> _manager;

                private readonly Model _model;
                public Model Model
                {
                    get { return _model; }
                }

                private readonly TValidatorColumnsAttribute _columnsAttribute;
                public TValidatorColumnsAttribute ColumnsAttribute
                {
                    get { return _columnsAttribute; }
                }

                private readonly IReadOnlyList<Entry> _entries;
                public IReadOnlyList<Entry> Entries
                {
                    get { return _entries; }
                }

                private readonly Column[] _columnList;
                public IReadOnlyList<Column> ColumnList
                {
                    get { return _columnList; }
                }

                private readonly IColumns _columns;
                public IColumns Columns
                {
                    get { return _columns; }
                }

                public IColumnValidationMessages Validate(DataRow dataRow)
                {
                    return IsValid(dataRow) ? ColumnValidationMessages.Empty
                        : new ColumnValidationMessage(MessageId, ValidationSeverity.Error, GetMessage(_columnList, dataRow), Columns);
                }

                private bool IsValid(DataRow dataRow)
                {
                    return _manager.IsValid(this, dataRow);
                }

                private string MessageId
                {
                    get { return ColumnsAttribute.MessageId; }
                }

                private string GetMessage(IReadOnlyList<Column> columns, DataRow dataRow)
                {
                    return ColumnsAttribute.GetMessage(columns, dataRow);
                }
            }

            protected override void Initialize(Model model, TValidatorColumnsAttribute columnsAttribute, IReadOnlyList<Entry> entries)
            {
                model.Validators.Add(new Validator(this, model, columnsAttribute, entries));
            }

            protected abstract bool IsValid(IValidationContext validationContext, DataRow dataRow);
        }


        protected ValidationColumnsMemberAttribute(string name)
            : base(name)
        {
        }
    }
}
