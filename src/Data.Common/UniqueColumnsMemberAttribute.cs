using System.Collections.Generic;
using DevZest.Data.Primitives;
using System.Diagnostics;
using System;

namespace DevZest.Data
{
    public sealed class UniqueColumnsMemberAttribute : ColumnsMemberAttribute
    {
        private sealed class Manager : Manager<UniqueColumnsAttribute, UniqueColumnsMemberAttribute>
        {
            private sealed class Validator : IValidator
            {
                public Validator(Model model, UniqueColumnsAttribute uniqueColumnsAttribute, ColumnSort[] orderByList)
                {
                    Debug.Assert(model != null);
                    Debug.Assert(uniqueColumnsAttribute != null);
                    Debug.Assert(orderByList != null && orderByList.Length > 0);
                    _model = model;
                    _uniqueColumnsAttribute = uniqueColumnsAttribute;
                    _orderByList = orderByList;
                    _columns = Columns.Empty;
                    for (int i = 0; i < _orderByList.Length; i++)
                        _columns = _columns.Add(_orderByList[i].Column);
                    _columns = _columns.Seal();
                }

                private readonly Model _model;
                private readonly UniqueColumnsAttribute _uniqueColumnsAttribute;
                private readonly ColumnSort[] _orderByList;
                private readonly IColumns _columns;

                public IColumnValidationMessages Validate(DataRow dataRow)
                {
                    return IsValid(dataRow) ? ColumnValidationMessages.Empty
                        : new ColumnValidationMessage(MessageId, ValidationSeverity.Error, GetMessage(AttributeName, _columns, dataRow), _columns);
                }

                private string AttributeName
                {
                    get { return _uniqueColumnsAttribute.Name; }
                }

                private string MessageId
                {
                    get { return _uniqueColumnsAttribute.MessageId; }
                }

                private string GetMessage(string attributeName, IColumns columns, DataRow dataRow)
                {
                    throw new NotImplementedException();
                }

                private bool IsValid(DataRow dataRow)
                {
                    var dataSet = _model.DataSet;
                    foreach (var other in dataSet)
                    {
                        if (dataRow != other && AreEqual(dataRow, other))
                            return false;
                    }
                    return true;
                }

                private bool AreEqual(DataRow dataRow, DataRow other)
                {
                    Debug.Assert(dataRow != other);
                    for (int i = 0; i < _orderByList.Length; i++)
                    {
                        var orderBy = _orderByList[i];
                        if (orderBy.Column.Compare(other, dataRow, orderBy.Direction) != 0)
                            return false;
                    }
                    return true;
                }
            }

            public static readonly Manager Singleton = new Manager();

            private Manager()
            {
            }

            protected override void Initialize(Model model, UniqueColumnsAttribute columnsAttribute, IReadOnlyList<Entry> entries)
            {
                var orderByList = GetOrderByList(entries);
                model.Unique(columnsAttribute.Name, columnsAttribute.IsClustered, orderByList);
                model.Validators.Add(new Validator(model, columnsAttribute, orderByList));
            }

            private static ColumnSort[] GetOrderByList(IReadOnlyList<Entry> entries)
            {
                var result = new ColumnSort[entries.Count];
                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    var sortDirection = entry.MemberAttribute.SortDirection;
                    var column = entry.Column;
                    result[i] = sortDirection == SortDirection.Descending ? column.Desc() : column.Asc();
                }
                return result;
            }
        }

        private static readonly Manager s_manager = Manager.Singleton;

        public UniqueColumnsMemberAttribute(string name)
            : base(name)
        {
        }

        protected internal override void Initialize(Column column)
        {
            s_manager.Initialize(this, column);
        }

        public SortDirection SortDirection { get; set; }
    }
}
