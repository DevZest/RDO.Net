using DevZest.Data.Primitives;
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
                var result = DataSet<BackupModel>.Create();
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

        /// <summary>Initializes a new instance of <see cref="DataRow"/> object.</summary>
        public DataRow()
        {
            Ordinal = -1;
            _index = -1;
        }

        internal DataRow(Model model)
            : this()
        {
            Model = model;
        }

        private DataSet[] _childDataSets;
        /// <summary>
        /// Gets the child DataSets owned by this DataRow.
        /// </summary>
        public IReadOnlyList<DataSet> ChildDataSets
        {
            get { return _childDataSets; }
        }

        /// <summary>Gets the <see cref="Model"/> which associated with this <see cref="DataRow"/>.</summary>
        public Model Model { get; private set; }

        internal void ResetModel()
        {
            Model = null;
        }

        /// <summary>
        /// Gets the ordinal of <see cref="BaseDataSet"/>.
        /// </summary>
        public int Ordinal { get; private set; }

        private int _index;
        /// <summary>
        /// Gets the index of <see cref="DataSet"/>.
        /// </summary>
        public int Index
        {
            get { return _index == -1 ? Ordinal : _index; }
        }

        /// <summary>Gets the parent <see cref="DataRow"/>.</summary>
        public DataRow ParentDataRow { get; private set; }

        /// <summary>
        /// Gets the base DataSet which contains all <see cref="DataRow"/> objects.
        /// </summary>
        public DataSet BaseDataSet
        {
            get { return Model?.DataSet; }
        }

        /// <summary>
        /// Gets the DataSet which contains child DataSet of <see cref="ParentDataRow"/>.
        /// </summary>
        public DataSet DataSet
        {
            get
            {
                var parentRow = ParentDataRow;
                return parentRow == null ? BaseDataSet : parentRow[Model];
            }
        }

        /// <summary>
        /// Gets a value indicates whether this <see cref="DataRow"/> is in editing mode.
        /// </summary>
        public bool IsEditing
        {
            get { return Model == null ? false : Model.EditingRow == this; }
        }

        /// <summary>
        /// Gets a value indicates whether this <see cref="DataRow"/> is in adding mode.
        /// </summary>
        public bool IsAdding
        {
            get { return Model == null ? false : Model.AddingRow == this; }
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
            _childDataSets = childModels.Count == 0 ? Array.Empty<DataSet>() : new DataSet[childModels.Count];
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

        /// <summary>
        /// Gets the child DataSet for specified child model.
        /// </summary>
        /// <param name="childModel">The specified child model.</param>
        /// <returns>The child DataSet.</returns>
        public DataSet this[Model childModel]
        {
            get
            {
                childModel.VerifyNotNull(nameof(childModel));
                if (childModel.ParentModel != Model)
                    throw new ArgumentException(DiagnosticMessages.DataRow_InvalidChildModel, nameof(childModel));
                return _childDataSets[childModel.Ordinal];
            }
        }

        /// <summary>
        /// Gets the child DataSet for specified child model ordinal.
        /// </summary>
        /// <param name="childModelOrdinal">The ordinal of child model.</param>
        /// <returns>The child DataSet.</returns>
        public DataSet this[int childModelOrdinal]
        {
            get
            {
                if (childModelOrdinal < 0 || childModelOrdinal >= _childDataSets.Length)
                    throw new ArgumentOutOfRangeException(nameof(childModelOrdinal));
                return _childDataSets[childModelOrdinal];
            }
        }

        private void ClearChildren()
        {
            foreach (var dataSet in _childDataSets)
                dataSet.Clear();
            _childDataSets = null;
        }

        /// <inheritdoc />
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

            if (!(parentDataRow.Model[childModelName] is Model childModel))
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
            if (!Int32.TryParse(input, out var result))
                throw new FormatException(DiagnosticMessages.DataRow_FromString_ParseInt(input));
            return result;
        }

        /// <summary>
        /// Validates data values of this DataRow.
        /// </summary>
        /// <returns>A collection of validation errors.</returns>
        public IDataValidationErrors Validate()
        {
            if (Model == null)
                throw new InvalidOperationException(DiagnosticMessages.DataRow_NullModel);
            return Model.Validate(this).Seal();
        }

        /// <summary>
        /// Copies data values from specified <see cref="DataRow"/>.
        /// </summary>
        /// <param name="from">The specified DataRow.</param>
        /// <param name="recursive">Specifies whether child DataSet should be copied.</param>
        public void CopyValuesFrom(DataRow from, bool recursive = true)
        {
            from.VerifyNotNull(nameof(from));
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

        /// <summary>
        /// Copies data values from other <see cref="DataRow"/>, for specified column mappings.
        /// </summary>
        /// <param name="from">The <see cref="DataRow"/> where data values will be copied from.</param>
        /// <param name="columnMappings">The specified column mappings.</param>
        public void CopyValuesFrom(DataRow from, IReadOnlyList<ColumnMapping> columnMappings)
        {
            from.VerifyNotNull(nameof(from));
            columnMappings.VerifyNotNull(nameof(columnMappings));
            for (int i = 0; i < columnMappings.Count; i++)
                columnMappings[i].CopyValue(from, this);
        }

        /// <summary>
        /// Moves this DataRow within current DataSet.
        /// </summary>
        /// <param name="offset">The offset to move this DataRow.</param>
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
                return dataRow => dataRow.Model.EndEdit(true);
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

        /// <summary>
        /// Enters into edit mode.
        /// </summary>
        /// <returns></returns>
        public bool BeginEdit()
        {
            if (Model == null || Model.EditingRow != null)
                return false;

            Model.BeginEdit(this);
            return true;
        }

        /// <summary>
        /// Ends the edit mode and saves the changes.
        /// </summary>
        /// <returns><see langword="true" /> if ends edit successfully, otherwise <see langword="false" />.</returns>
        public bool EndEdit()
        {
            if (!IsEditing || IsAdding)
                return false;

            SuspendValueChangedNotification(false);
            Model.DataSetContainer.SuspendComputation();
            Model.EndEdit(false);
            Model.DataSetContainer.ResumeComputation();
            ResumeValueChangedNotification();
            return true;
        }

        /// <summary>
        /// Cancels the edit mode.
        /// </summary>
        /// <returns><see langword="true" /> if edit mode cancelled successfully, otherwise <see langword="false" />.</returns>
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
        /// <summary>
        /// Gets a value indicating whether value changed notification is suspended.
        /// </summary>
        public bool IsValueChangedNotificationSuspended
        {
            get { return _suspendValueChangedCount > 0; }
        }

        /// <summary>
        /// Suspends value changed notification.
        /// </summary>
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

        /// <summary>
        /// Resumes value changed notification.
        /// </summary>
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
            if (IsEditing)
                return;

            BaseDataSet.UpdateRevision();
            if (DataSet != BaseDataSet)
                DataSet.UpdateRevision();
        }

        /// <summary>
        /// Gets or sets a value to specify whether primary key should be sealed.
        /// </summary>
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

        /// <summary>
        /// Constructs a DataRow predicate.
        /// </summary>
        /// <typeparam name="T">Type of the model.</typeparam>
        /// <param name="predicate"></param>
        /// <param name="ensureStatic"></param>
        /// <returns></returns>
        public static Predicate<DataRow> Where<T>(Func<T, DataRow, bool> predicate, bool ensureStatic = true)
            where T : Model
        {
            if (predicate == null)
                return null;

            if (ensureStatic && predicate.Target != null)
                throw new ArgumentException(DiagnosticMessages.DataRow_WhereExpressionMustBeStatic, nameof(predicate));

            return new DataRowFilter<T>(predicate).ToPredicate();
        }

        /// <summary>
        /// Constructs column comparer to sort DataRow.
        /// </summary>
        /// <param name="column">The column to sort DataRow.</param>
        /// <param name="direction">The sort direction.</param>
        /// <returns>The column comparer.</returns>
        public static IColumnComparer OrderBy(Column column, SortDirection direction = SortDirection.Ascending)
        {
            VerifyOrderBy(column, nameof(column));
            return column.ToColumnComparer(direction);
        }

        /// <summary>
        /// Constructs column comparer to sort DataRow.
        /// </summary>
        /// <typeparam name="T">The data type of the column.</typeparam>
        /// <param name="column">The column to sort DataRow.</param>
        /// <param name="direction">The sort direction.</param>
        /// <param name="valueComparer">The value comparer.</param>
        /// <returns>The column comparer.</returns>
        public static IColumnComparer OrderBy<T>(Column<T> column, SortDirection direction = SortDirection.Ascending, IComparer<T> valueComparer = null)
        {
            VerifyOrderBy(column, nameof(column));
            return column.ToColumnComparer(direction, valueComparer);
        }

        private static void VerifyOrderBy(Column column, string paramName)
        {
            column.VerifyNotNull(paramName);
            if (column.ScalarSourceModels.Count != 1)
                throw new ArgumentException(DiagnosticMessages.DataRow_OrderByColumnMustBeSingleSourceModel, paramName);
        }

        /// <summary>
        /// Serialize this DataRow into JSON string.
        /// </summary>
        /// <param name="isPretty">Specifies whether serialized JSON string should be indented.</param>
        /// <param name="customizer">The customizer which can customize the serialization.</param>
        /// <returns></returns>
        public string ToJsonString(bool isPretty, IJsonCustomizer customizer = null)
        {
            return JsonWriter.Create(customizer).Write(this).ToString(isPretty);
        }
    }
}
