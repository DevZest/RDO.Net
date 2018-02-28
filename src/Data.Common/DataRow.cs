using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a row of in-memory data.
    /// </summary>
    public class DataRow
    {
        private sealed class BackupModel : Model
        {
            public static DataSet<BackupModel> Backup(Model origin)
            {
                var result = DataSet<BackupModel>.New();
                result._.Initialize(origin);
                return result;
            }

            private Model _origin;

            private void Initialize(Model origin)
            {
                if (origin.IsInitialized)
                {
                    _origin = origin;
                    var originChildModels = origin.ChildModels;
                    for (int i = 0; i < originChildModels.Count; i++)
                    {
                        var childBackupModel = new BackupModel();
                        childBackupModel.Construct(this, this.GetType(), "ChildModel" + i.ToString(CultureInfo.InvariantCulture));
                        childBackupModel.Initialize(originChildModels[i]);
                    }
                }
            }

            protected override void OnChildDataSetsCreated()
            {
                if (_origin != null)
                {
                    MapColumnsFrom(_origin);
                    _origin = null;
                }
                base.OnChildDataSetsCreated();
            }

            private void MapColumnsFrom(Model origin)
            {
                MapColumnsFrom(origin.Columns);
                MapColumnsFrom(origin.LocalColumns);
            }

            private void MapColumnsFrom(IReadOnlyList<Column> columns)
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    if (!column.IsExpression)
                    {
                        var backupColumn = column.CreateBackup(this);
                        _backupMappings.Add(backupColumn.MapFrom(column));
                        _restoreMappings.Add(column.MapFrom(backupColumn));
                    }
                }
            }

            private List<ColumnMapping> _backupMappings = new List<ColumnMapping>();
            public IReadOnlyList<ColumnMapping> BackupMappings
            {
                get { return _backupMappings; }
            }

            private List<ColumnMapping> _restoreMappings = new List<ColumnMapping>();
            public IReadOnlyList<ColumnMapping> RestoreMappings
            {
                get { return _restoreMappings; }
            }
        }

        internal static readonly DataRow Placeholder = CreatePlaceholderDataRow();

        private static DataRow CreatePlaceholderDataRow()
        {
            var result = new DataRow();
            result.SuspendValueChangedNotification(true);
            return result;
        }

        /// <summary>Initializes a new instance of <see cref="DataRow"/> object.</summary>
        public DataRow()
        {
            Ordinal = -1;
            _index = -1;
        }

        private DataSet[] _childDataSets;
        internal IReadOnlyCollection<DataSet> ChildDataSets
        {
            get { return _childDataSets; }
        }

        /// <summary>Gets the <see cref="Model"/> which associated with this <see cref="DataRow"/>.</summary>
        public Model Model { get; private set; }

        public int Ordinal { get; private set; }

        private int _index;
        public int Index
        {
            get { return _index == -1 ? Ordinal : _index; }
        }

        /// <summary>Gets the parent <see cref="DataRow"/>.</summary>
        public DataRow ParentDataRow { get; private set; }

        public DataSet BaseDataSet
        {
            get { return Model == null ? null : Model.DataSet; }
        }

        public DataSet DataSet
        {
            get
            {
                var parentRow = ParentDataRow;
                return parentRow == null ? BaseDataSet : parentRow[Model];
            }
        }

        internal void InitializeByChildDataSet(DataRow parent, int index)
        {
            Debug.Assert(ParentDataRow == null);
            Debug.Assert(parent != null);

            ParentDataRow = parent;
            _index = index;
        }

        internal void DisposeByChildDataSet()
        {
            ParentDataRow = null;
            _index = -1;
        }

        internal void InitializeByBaseDataSet(Model model, int ordinal)
        {
            Debug.Assert(Model == null);
            Debug.Assert(model != null);

            Model = model;
            Ordinal = ordinal;

            model.EnsureInitialized();
            var childModels = model.ChildModels;
            _childDataSets = childModels.Count == 0 ? Array<DataSet>.Empty : new DataSet[childModels.Count];
            for (int i = 0; i < childModels.Count; i++)
                _childDataSets[i] = childModels[i].DataSet.CreateChildDataSet(this);

            var columns = model.Columns;
            foreach (var column in columns)
                column.InsertRow(this);
        }

        internal void DisposeByBaseDataSet()
        {
            ClearChildren();

            var columns = Model.Columns;
            foreach (var column in columns)
                column.RemoveRow(this);

            Model = null;
            Ordinal = -1;
        }

        internal void AdjustOrdinal(int value)
        {
            Debug.Assert(Ordinal != value);
            Ordinal = value;
        }

        internal void AdjustIndex(int value)
        {
            Debug.Assert(_index != value);
            _index = value;
        }

        public DataSet this[Model childModel]
        {
            get
            {
                Check.NotNull(childModel, nameof(childModel));
                if (childModel.ParentModel != Model)
                    throw new ArgumentException(DiagnosticMessages.InvalidChildModel, nameof(childModel));
                return _childDataSets[childModel.Ordinal];
            }
        }

        public DataSet this[int modelOrdinal]
        {
            get
            {
                if (modelOrdinal < 0 || modelOrdinal >= _childDataSets.Length)
                    throw new ArgumentOutOfRangeException(nameof(modelOrdinal));
                return _childDataSets[modelOrdinal];
            }
        }

        private void ClearChildren()
        {
            foreach (var dataSet in _childDataSets)
                dataSet.Clear();
            _childDataSets = null;
        }

        /// <summary>Gets the children data set of this <see cref="DataRow"/>.</summary>
        /// <typeparam name="T">The type of child model.</typeparam>
        /// <param name="childModel">The child model.</param>
        /// <returns>The children data set.</returns>
        public DataSet<T> Children<T>(T childModel)
            where T : Model, new()
        {
            Check.NotNull(childModel, nameof(childModel));
            if (childModel.ParentModel != Model)
                throw new ArgumentException(DiagnosticMessages.InvalidChildModel, nameof(childModel));

            return (DataSet<T>)this[childModel.Ordinal];
        }

        public override string ToString()
        {
            var parentDataRow = this.ParentDataRow;
            if (this.ParentDataRow == null)
                return string.Format(CultureInfo.InvariantCulture, "/[{0}]", Ordinal);

            var result = string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", Model.Name, _index);
            return parentDataRow.ToString() + "/" + result;
        }

        internal static DataRow FromString(DataSet dataSet, string input)
        {
            Debug.Assert(dataSet.Model.ParentModel == null);

            var inputIndex = 0;
            ExpectString(input, ref inputIndex, '/');
            var leftSquareBracketIndex = inputIndex;
            var dataRowOrdinal = ExpectInt(input, ref inputIndex, '[', ']');
            var dataRow = GetDataRow(dataSet, dataRowOrdinal, input, leftSquareBracketIndex);
            return inputIndex == input.Length ? dataRow : Deserialize(dataRow, input, inputIndex);
        }

        private static DataRow GetDataRow(DataSet dataSet, int ordinal, string input, int leftSquareBracketIndex)
        {
            if (ordinal < 0 || ordinal >= dataSet.Count)
                throw new FormatException(DiagnosticMessages.DataRow_FromString_InvalidDataRowOrdinal(ordinal, input.Substring(0, leftSquareBracketIndex)));
            return dataSet[ordinal];
        }

        private static DataRow Deserialize(DataRow parentDataRow, string input, int inputIndex)
        {
            var dataRowPathEndIndex = inputIndex;
            var childModelName = ExpectString(input, ref inputIndex, '/', '[');
            var leftSquareBracketIndex = inputIndex - 1;
            var dataRowOrdinal = ExpectInt(input, ref inputIndex, ']');

            var childModel = parentDataRow.Model[childModelName] as Model;
            if (childModel == null)
                throw new FormatException(DiagnosticMessages.DataRow_FromString_InvalidChildModelName(childModelName, input.Substring(0, dataRowPathEndIndex)));

            var result = GetDataRow(parentDataRow[childModel], dataRowOrdinal, input, leftSquareBracketIndex);
            return inputIndex == input.Length ? result : Deserialize(result, input, inputIndex);
        }

        private static string ExpectString(string input, ref int inputIndex, char startChar, char endChar)
        {
            if (input[inputIndex] != startChar)
                throw new FormatException(DiagnosticMessages.DataRow_FromString_ExpectChar(startChar, input.Substring(0, inputIndex)));

            inputIndex++;
            return ExpectString(input, ref inputIndex, endChar);
        }

        private static string ExpectString(string input, ref int inputIndex, char endChar)
        {
            var startIndex = inputIndex;
            while (inputIndex < input.Length && input[inputIndex] != endChar)
                inputIndex++;

            if (inputIndex == input.Length)
                throw new FormatException(DiagnosticMessages.DataRow_FromString_ExpectChar(endChar, input.Substring(0, startIndex)));

            var result = input.Substring(startIndex, inputIndex - startIndex);
            inputIndex++;
            return result;
        }

        private static int ExpectInt(string input, ref int inputIndex, char endChar)
        {
            return ParseInt(ExpectString(input, ref inputIndex, endChar));
        }

        private static int ExpectInt(string input, ref int inputIndex, char startChar, char endChar)
        {
            return ParseInt(ExpectString(input, ref inputIndex, startChar, endChar));
        }

        private static int ParseInt(string input)
        {
            int result;
            if (!Int32.TryParse(input, out result))
                throw new FormatException(DiagnosticMessages.DataRow_FromString_ParseInt(input));
            return result;
        }

        public IDataValidationErrors Validate()
        {
            if (Model == null)
                throw new InvalidOperationException(DiagnosticMessages.DataRow_NullModel);
            return Model.Validate(this).Seal();
        }

        public void CopyValuesFrom(DataRow from, bool recursive = true)
        {
            Check.NotNull(from, nameof(from));
            if (from.Model == null)
                throw new ArgumentException(DiagnosticMessages.DataRow_NullModel, nameof(from));
            if (Model == null)
                throw new InvalidOperationException(DiagnosticMessages.DataRow_NullModel);
            DoCopyValuesFrom(from, recursive);
        }

        private void DoCopyValuesFrom(DataRow from, bool recursive)
        {
            var fromColumns = from.Model.Columns;
            var thisColumns = Model.Columns;
            var count = Math.Min(fromColumns.Count, thisColumns.Count);
            for (int i = 0; i < count; i++)
                thisColumns[i].MapFrom(fromColumns[i]).CopyValue(from, this);

            var fromLocalColumns = from.Model.LocalColumns;
            var thisLocalColumns = Model.LocalColumns;
            count = Math.Min(fromLocalColumns.Count, thisLocalColumns.Count);
            for (int i = 0; i < fromLocalColumns.Count; i++)
                thisLocalColumns[i].MapFrom(fromLocalColumns[i]).CopyValue(from, this);

            if (recursive)
                CopyChildren(from);
        }

        private void CopyChildren(DataRow from)
        {
            for (int i = 0; i < _childDataSets.Length; i++)
            {
                var children = _childDataSets[i];
                var fromChildren = from._childDataSets[i];
                for (int j = 0; j < fromChildren.Count; j++)
                    children.AddRow(x => x.DoCopyValuesFrom(fromChildren[j], true));
            }
        }

        public void CopyValuesFrom(DataRow from, IReadOnlyList<ColumnMapping> columnMappings)
        {
            Check.NotNull(from, nameof(from));
            Check.NotNull(columnMappings, nameof(columnMappings));
            for (int i = 0; i < columnMappings.Count; i++)
                columnMappings[i].CopyValue(from, this);
        }

        public void Move(int offset)
        {
            if (offset == 0)
                return;

            var dataSet = DataSet;
            if (dataSet == null)
                return;

            var newIndex = Index + offset;
            if (newIndex < 0 || newIndex > dataSet.Count - 1)
                throw new ArgumentOutOfRangeException(nameof(offset));

            var restore = Backup();
            Model.LockEditingRow(() => dataSet.Remove(this));
            dataSet.Insert(newIndex, this, restore);
        }

        private Action<DataRow> Backup()
        {
            if (Model.EditingRow == null && !HasChild)
            {
                Model.BeginEdit(this);
                return dataRow => dataRow.Model.EndEdit(dataRow, true);
            }
            else
            {
                var backupDataSet = BackupModel.Backup(Model);
                var backupDataRow = backupDataSet.AddRow(dataRow => dataRow.Backup(this));
                return dataRow => dataRow.Restore(backupDataRow);
            }
        }

        private void Backup(DataRow from)
        {
            var backupModel = (BackupModel)Model;
            CopyValuesFrom(from, backupModel.BackupMappings);
            BackupChildren(from);
        }

        private void BackupChildren(DataRow from)
        {
            for (int i = 0; i < _childDataSets.Length; i++)
            {
                var children = _childDataSets[i];
                var fromChildren = from._childDataSets[i];
                for (int j = 0; j < fromChildren.Count; j++)
                    children.AddRow(x => x.Backup(fromChildren[j]));
            }
        }

        private void Restore(DataRow from)
        {
            var backupModel = (BackupModel)from.Model;
            CopyValuesFrom(from, backupModel.RestoreMappings);
            RestoreChildren(from);
        }

        private void RestoreChildren(DataRow from)
        {
            for (int i = 0; i < _childDataSets.Length; i++)
            {
                var children = _childDataSets[i];
                var fromChildren = from._childDataSets[i];
                for (int j = 0; j < fromChildren.Count; j++)
                    children.AddRow(x => x.Restore(fromChildren[j]));
            }
        }

        private bool HasChild
        {
            get
            {
                for (int i = 0; i < _childDataSets.Length; i++)
                {
                    if (_childDataSets[i].Count > 0)
                        return true;
                }

                return false;
            }
        }

        public bool BeginEdit()
        {
            if (Model == null || Model.EditingRow != null)
                return false;

            Model.BeginEdit(this);
            return true;
        }

        public bool EndEdit()
        {
            if (Model == null || Model.EditingRow != this)
                return false;

            SuspendValueChangedNotification(false);
            Model.DataSetContainer.SuspendComputation();
            Model.EndEdit(this, false);
            Model.DataSetContainer.ResumeComputation();
            ResumeValueChangedNotification();
            return true;
        }

        public bool CancelEdit()
        {
            if (Model == null || Model.EditingRow != this)
                return false;

            Model.CancelEdit();
            return true;
        }

        internal DataRow AncestorOf(int ancestorLevel)
        {
            var dataRow = this;
            Debug.Assert(ancestorLevel >= 0);
            for (int i = 0; i < ancestorLevel; i++)
                dataRow = dataRow.ParentDataRow;
            return dataRow;
        }

        private int _suspendValueChangedCount;
        public bool IsValueChangedNotificationSuspended
        {
            get { return _suspendValueChangedCount > 0; }
        }

        public void SuspendValueChangedNotification()
        {
            if (_pendingValueChangedColumns == null)
                SuspendValueChangedNotification(true);  // calling inside insert or remove.
            else
                SuspendValueChangedNotification(false);
        }

        internal void SuspendValueChangedNotification(bool discardChanges)
        {
            var value = discardChanges ? null : Columns.Empty;
            Debug.Assert(_suspendValueChangedCount == 0 || _pendingValueChangedColumns == value);
            _pendingValueChangedColumns = value;
            _suspendValueChangedCount++;
        }

        public void ResumeValueChangedNotification()
        {
            if (_suspendValueChangedCount <= 0)
                throw new InvalidOperationException(DiagnosticMessages.DataRow_ResumeValueChangedNotificationWithoutSuspend);
            _suspendValueChangedCount--;
            if (_suspendValueChangedCount == 0)
            {
                if (_pendingValueChangedColumns != null && _pendingValueChangedColumns.Count > 0)
                    NotifyValueChanged(_pendingValueChangedColumns.Seal());
                _pendingValueChangedColumns = Columns.Empty;
            }
        }

        private IColumns _pendingValueChangedColumns = Columns.Empty;
        internal void OnValueChanged(Column column)
        {
            if (IsValueChangedNotificationSuspended)
            {
                if (_pendingValueChangedColumns != null)
                    _pendingValueChangedColumns = _pendingValueChangedColumns.Add(column);
                return;
            }
            NotifyValueChanged(column);
        }

        private void NotifyValueChanged(IColumns columns)
        {
            Debug.Assert(columns.IsSealed);
            UpdateDataSetRevision();
            Model.HandlesValueChanged(this, columns);
        }

        private void UpdateDataSetRevision()
        {
            BaseDataSet.UpdateRevision();
            if (DataSet != BaseDataSet)
                DataSet.UpdateRevision();
        }

        public bool IsPrimaryKeySealed { get; set; }

        private sealed class DataRowFilter<T>
            where T : Model
        {
            public DataRowFilter(Func<T, DataRow, bool> where)
            {
                Debug.Assert(where != null);
                _where = where;
            }

            private readonly Func<T, DataRow, bool> _where;

            private bool Evaluate(DataRow dataRow)
            {
                return _where((T)dataRow.Model, dataRow);
            }

            public Predicate<DataRow> ToPredicate()
            {
                return Evaluate;
            }
        }

        public static Predicate<DataRow> Where<T>(Func<T, DataRow, bool> predicate, bool ensureStatic = true)
            where T : Model
        {
            if (predicate == null)
                return null;

            if (ensureStatic && predicate.Target != null)
                throw new ArgumentException(DiagnosticMessages.DataRow_WhereExpressionMustBeStatic, nameof(predicate));

            return new DataRowFilter<T>(predicate).ToPredicate();
        }

        public static IColumnComparer OrderBy(Column column, SortDirection direction = SortDirection.Ascending)
        {
            VerifyOrderBy(column, nameof(column));
            return column.ToColumnComparer(direction);
        }

        public static IColumnComparer OrderBy<T>(Column<T> column, SortDirection direction = SortDirection.Ascending, IComparer<T> valueComparer = null)
        {
            VerifyOrderBy(column, nameof(column));
            return column.ToColumnComparer(direction, valueComparer);
        }

        private static void VerifyOrderBy(Column column, string paramName)
        {
            Check.NotNull(column, paramName);
            if (column.ScalarSourceModels.Count != 1)
                throw new ArgumentException(DiagnosticMessages.DataRow_OrderByColumnMustBeSingleSourceModel, paramName);
        }
    }
}
