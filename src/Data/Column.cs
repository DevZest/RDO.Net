using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using DevZest.Data.Addons;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a data column.
    /// </summary>
    public abstract class Column : ModelMember, IColumns, IComparer<DataRow>
    {
        /// <summary>
        /// Creates a new instance of <see cref="Column"/>.
        /// </summary>
        protected Column()
        {
        }

        /// <summary>
        /// Gets the original owner type of this <see cref="Column"/>.
        /// </summary>
        /// <remarks>This property forms <see cref="ColumnId"/> of this <see cref="Column"/>.</remarks>
        public Type OriginalDeclaringType { get; internal set; }

        /// <summary>
        /// Gets the name of the <see cref="Column"/>.
        /// </summary>
        public new string Name
        {
            get { return base.Name; }
        }

        /// <summary>
        /// Gets the namespace of the <see cref="Column"/>.
        /// </summary>
        public string Namespace
        {
            get { return ParentModel?.Namespace; }
        }

        /// <summary>
        /// Gets the full name of the <see cref="Column"/>, including <see cref="Namespace"/> if this column is a <see cref="Projection"/>.
        /// </summary>
        public string FullName
        {
            get
            {
                var @namespace = Namespace;
                return string.IsNullOrEmpty(@namespace) ? Name : $"{@namespace}.{Name}";
            }
        }

        /// <summary>
        /// Gets the original name of this <see cref="Column"/>.
        /// </summary>
        /// <remarks>This property forms <see cref="ColumnId"/> of this <see cref="Column"/>.</remarks>
        public string OriginalName { get; internal set; }

        private Type OwnerModelType
        {
            get { return ParentModel?.OwnerModel.GetType(); }
        }

        /// <summary>
        /// Gets the Id of the column.
        /// </summary>
        /// <remarks>Column Id contains the declaring type and name, which can uniquely identify the column.
        /// It is used as first candidate to perform automatic column mapping.</remarks>
        public ColumnId Id
        {
            get { return new ColumnId(OwnerModelType ?? DeclaringType, FullName); }
        }

        /// <summary>
        /// Gets the original Id of the column.
        /// </summary>
        /// <remarks>
        /// Original Id is inherited from existing <see cref="Mounter{T}"/> object by calling
        /// see cref="Model.RegisterColumn{TModel, TColumn}(System.Linq.Expressions.Expression{Func{TModel, TColumn}}, Mounter{TColumn})"/> method.
        /// It is used as second candiate to perform automatic column mapping.
        /// </remarks>
        public ColumnId OriginalId
        {
            get { return new ColumnId(OriginalDeclaringType, OriginalName); }
        }

        /// <summary>
        /// Gets a value indicates whether this column is part of primary key.
        /// </summary>
        public abstract bool IsPrimaryKey { get; }

        /// <summary>
        /// Gets a value indicates whether this column is identity (auto increment).
        /// </summary>
        public virtual bool IsIdentity
        {
            get { return false; }
        }

        /// <summary>Gets the zero-based position of the column in the <see cref="Model.Columns"/> collection.</summary>
        /// <remarks>If the column is not added to any <see cref="Model"/>, -1 is returned.</remarks>
        public int Index { get; private set; } = -1;

        /// <summary>Gets the zero-based position of the column for all columns by owner model, including recursive projection(s).</summary>
        /// <remarks>If the column is not added to any <see cref="Model"/>, -1 is returned.</remarks>
        public int Ordinal
        {
            get
            {
                if (ParentModel == null)
                    return Index;

                if (ParentModel == OwnerModel)
                {
                    if (!IsSystem)
                        return Index;

                    // system column is at tail of the collection.
                    var columns = ParentModel.Columns;
                    Debug.Assert(columns.SystemColumnCount > 0);
                    return Index + ParentModel.TotalColumnCount - columns.Count;
                }

                Debug.Assert(!IsSystem);
                return ParentModel.ColumnOrdinalOffset + Index;
            }
        }

        private string _dbColumnName;
        /// <summary>Gets or sets the column name in database.</summary>
        public string DbColumnName
        {
            get { return _dbColumnName ?? Name; }
            set
            {
                VerifyDesignMode();
                _dbColumnName = value;
            }
        }

        private string _dbColumnDescription;
        /// <summary>
        /// Gets or sets the column description in database.
        /// </summary>
        public string DbColumnDescription
        {
            get { return _dbColumnDescription; }
            set
            {
                VerifyDesignMode();
                _dbColumnDescription = value;
            }
        }

        /// <summary>Gets a value indicates whether current column is expression.</summary>
        public abstract bool IsExpression { get; }

        /// <summary>
        /// Gets a value indicates whether this column is an expression and not a member of model.
        /// </summary>
        public bool IsAbsoluteExpression
        {
            get { return ParentModel == null && IsExpression; }
        }

        internal static T Create<T>(Type originalDeclaringType, string originalName)
            where T : Column, new()
        {
            return Create(() => new T(), originalDeclaringType, originalName);
        }

        internal static T Create<T>(Func<T> create, Type originalDeclaringType, string originalName)
            where T : Column
        {
            var result = create();
            if (result != null)
            {
                result.OriginalDeclaringType = originalDeclaringType;
                result.OriginalName = originalName;
            }
            return result;
        }

        private Action<Column> _initializer;

        internal void Initialize(Model parentModel, Type declaringType, string name, ColumnKind kind, Action<Column> initializer)
        {
            if (Kind != ColumnKind.None)
                throw new InvalidOperationException(DiagnosticMessages.Column_AlreadyInitialized);

            Debug.Assert(parentModel != null);
            ConstructModelMember(parentModel, declaringType, name);
            Kind = kind;
            if (OriginalDeclaringType == null)
                OriginalDeclaringType = DeclaringType;
            if (string.IsNullOrEmpty(OriginalName))
                OriginalName = name;

            _initializer = initializer;
            Index = ParentModel.Add(this);

            _initializer?.Invoke(this);
        }

        /// <summary>Gets the <see cref="ColumnKind"/> of this column.</summary>
        public ColumnKind Kind { get; internal set; }

        /// <summary>Gets a value indicates whether this is a system column.</summary>
        /// <value><see langword="true"/> if this is a system column, otherwise <see langword="false"/>.</value>
        public bool IsSystem
        {
            get { return Kind == ColumnKind.SystemRowId || Kind == ColumnKind.SystemParentRowId || Kind == ColumnKind.SystemCustom; }
        }

        /// <summary>Gets the <see cref="DbExpression"/> of this column for SQL generation.</summary>
        public abstract DbExpression DbExpression { get; }

        /// <summary>
        /// Gets the expression of this column.
        /// </summary>
        /// <returns>The expression.</returns>
        public abstract ColumnExpression GetExpression();

        /// <summary>
        /// Gets the database computation expression.
        /// </summary>
        public abstract DbExpression DbComputedExpression { get; }

        /// <summary>
        /// Gets the referenced columns of the expression.
        /// </summary>
        public abstract IColumns BaseColumns { get; }

        /// <summary>Gets the data type of this <see cref="Column"/>.</summary>
        public abstract Type DataType { get; }

        internal abstract void InsertRow(DataRow dataRow);

        internal abstract void RemoveRow(DataRow dataRow);

        /// <inheritdoc/>
        public override string ToString()
        {
            return IsExpression ? "[Expression]" : FullName;
        }

        /// <summary>Gets the set of <see cref="Model"/> objects directly combined this <see cref="Column"/>.</summary>
        public abstract IModels ScalarSourceModels { get; }

        /// <summary>Gets the set of parent <see cref="Model"/> objects aggregated to this <see cref="Column"/>.</summary>
        public abstract IModels AggregateSourceModels { get; }

        /// <summary>Verifies whether this column belongs to provided <see cref="DbReader"/>.</summary>
        /// <param name="reader">The <see cref="DbReader"/> object to be verified.</param>
        /// <remarks>Inherited class should call this method before reading value from <see cref="DbReader"/>.</remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">This column does not belong to the <paramref name="reader"/>.</exception>
        protected void VerifyDbReader(DbReader reader)
        {
            reader.VerifyNotNull(nameof(reader));
            if (reader.Model != this.ParentModel)
                throw new ArgumentException(DiagnosticMessages.Column_InvalidDbReader, nameof(reader));
        }

        internal Column Clone(Model parentModel)
        {
            var result = (Column)Activator.CreateInstance(this.GetType());
            result.OriginalDeclaringType = parentModel.GetType();
            result.OriginalName = this.Name;
            result.Initialize(parentModel, parentModel.GetType(), this.Name, this.Kind, this._initializer);
            return result;
        }

        internal void InitDbColumnName(Dictionary<string, int> dbColumnNameSuffixes)
        {
            if (_dbColumnName != null)
                _dbColumnName = dbColumnNameSuffixes.GetUniqueName(_dbColumnName);
        }

        internal abstract void InitValueManager();

        /// <summary>
        /// Gets a value indicates whether this column is serializable.
        /// </summary>
        public virtual bool IsSerializable
        {
            get { return IsDeserializable; }
        }

        /// <summary>
        /// Gets a value indicates whether this column is deserializable.
        /// </summary>
        public abstract bool IsDeserializable { get; }

        /// <summary>Serializes the value at given <see cref="DataRow"/> oridinal as JSON.</summary>
        /// <param name="rowOrdinal">The <see cref="DataRow"/> ordinal.</param>
        /// <returns>The serialized <see cref="JsonValue"/>.</returns>
        public abstract JsonValue Serialize(int rowOrdinal);

        /// <summary>Deserializes the value at given <see cref="DataRow"/> ordinal from JSON.</summary>
        /// <param name="rowOrdinal">The <see cref="DataRow"/> ordinal.</param>
        /// <param name="value">The JSON value to be deserialized.</param>
        public abstract void Deserialize(int rowOrdinal, JsonValue value);

        /// <summary>Creates column of parameter expression using value of given <see cref="DataRow"/>.</summary>
        /// <param name="dataRow">The <see cref="DataRow"/> object.</param>
        /// <returns>The created column of parameter expression.</returns>
        protected internal abstract Column CreateParam(DataRow dataRow);

        internal abstract ColumnDefault CreateDefault(string name, string description);

        /// <summary>Gets the <see cref="Identity"/> object if this column is identity column.</summary>
        /// <param name="isTempTable"><see langword="true"/> to return the <see cref="Identity"/> declared for temp table(s),
        /// otherwise to return the <see cref="Identity"/> declared for permanent table.</param>
        /// <returns>The <see cref="Identity"/> object, or <see langword="null"/> if this is not a identity column.</returns>
        public Identity GetIdentity(bool isTempTable)
        {
            return (Identity)GetAddon(isTempTable ? typeof(TempTableIdentity) : typeof(Identity));
        }

        /// <summary>Gets this column as asending sorted.</summary>
        /// <returns>The <see cref="ColumnSort"/> structure.</returns>
        public ColumnSort Asc()
        {
            return new ColumnSort(this, SortDirection.Ascending);
        }

        /// <summary>Gets this column as descending sorted.</summary>
        /// <returns>The <see cref="ColumnSort"/> structure.</returns>
        public ColumnSort Desc()
        {
            return new ColumnSort(this, SortDirection.Descending);
        }

        /// <summary>Declares nullability of this column.</summary>
        /// <param name="isNullable"><see langword="true"/> if this column is nullable, otherwise <see langword="false"/>.</param>
        /// <seealso cref="IsNullable"/>
        public void Nullable(bool isNullable)
        {
            if (isNullable)
            {
                var key = ColumnNotNull.Singleton.Key;
                if (ContainsAddon(key))
                    RemoveAddon(key);
            }
            else
                AddOrUpdate(ColumnNotNull.Singleton);
        }

        /// <summary>Gets a value indicates whether this column is nullable.</summary>
        /// <value><see langword="true"/> if this column is nullable, otherwise <see langword="false"/>.</value>
        /// <seealso cref="Nullable(bool)"/>
        public bool IsNullable
        {
            get
            {
                if (!DataType.IsNullable())
                    return false;
                if (IsPrimaryKey || GetIdentity(true) != null || GetIdentity(false) != null)
                    return false;
                var key = ColumnNotNull.Singleton.Key;
                return !ContainsAddon(key);
            }
        }

        /// <summary>Gets the <see cref="ColumnDefault"/> object associated with this column.</summary>
        /// <returns>The <see cref="ColumnDefault"/> object associated with this column.</returns>
        public ColumnDefault GetDefault()
        {
            return GetAddon<ColumnDefault>();
        }

        /// <summary>
        /// Gets a value indicates whether this column is computed in database.
        /// </summary>
        public abstract bool IsDbComputed { get; }

        /// <summary>Determines whether the value of given <see cref="DataRow"/> is null.</summary>
        /// <param name="dataRow">The <see cref="DataRow"/> object.</param>
        /// <returns><see langword="true"/> if the value of given <see cref="DataRow"/> is null, otherwise <see langword="false"/>.</returns>
        public abstract bool IsNull(DataRow dataRow);

        /// <summary>Creates <see cref="ColumnMapping"/> from given <see cref="Column"/>.</summary>
        /// <param name="column">The source <see cref="Column"/>.</param>
        /// <returns>The created <see cref="ColumnMapping"/>.</returns>
        public abstract ColumnMapping MapFrom(Column column);

        /// <summary>
        /// Gets the data value of specified <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dataRow">The specified <see cref="DataRow"/>.</param>
        /// <param name="beforeEdit">Determines whether should return the value before <see cref="DataRow"/> entering edit mode.</param>
        /// <returns>The data value.</returns>
        public abstract object GetValue(DataRow dataRow, bool beforeEdit = false);

        /// <summary>
        /// Sets the data value of specified <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dataRow">The specified <see cref="DataRow"/>.</param>
        /// <param name="value">The data value.</param>
        /// <param name="beforeEdit">Determines whether should set the original value before <see cref="DataRow"/> entering edit mode.</param>
        public abstract void SetValue(DataRow dataRow, object value, bool beforeEdit = false);

        #region IColumnSet

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        bool IColumns.Contains(Column column)
        {
            return column == this;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<Column>.Count
        {
            get { return 1; }
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<Column> IEnumerable<Column>.GetEnumerator()
        {
            yield return this;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        bool IColumns.IsSealed
        {
            get { return true; }
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IColumns IColumns.Seal()
        {
            return this;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IColumns IColumns.Add(Column value)
        {
            value.VerifyNotNull(nameof(value));
            if (value == this)
                return this;
            return Columns.New(this, value);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IColumns IColumns.Remove(Column value)
        {
            value.VerifyNotNull(nameof(value));
            if (value == this)
                return Columns.Empty;
            else
                return this;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IColumns IColumns.Clear()
        {
            return Columns.Empty;
        }

        #endregion

        internal abstract void BeginEdit(DataRow dataRow);

        internal abstract void EndEdit(DataRow dataRow);

        internal abstract void CopyValue(DataRow sourceDataRow, Column targetColumn, DataRow targetDataRow);

        /// <summary>
        /// Casts this column into a <see cref="_String"/> column.
        /// </summary>
        /// <returns></returns>
        public abstract _String CastToString();

        private Func<string> _displayShortNameGetter;
        /// <summary>Gets or sets a value that is used for display the short name in the UI.</summary>
		/// <returns>A value that is used for display the short name in the UI.</returns>
        /// <remarks>The <see cref="DisplayShortName"/> is typically used as the grid column caption.</remarks>
        public string DisplayShortName
        {
            get { return _displayShortNameGetter != null ? _displayShortNameGetter() : DisplayName; }
            set
            {
                VerifyDesignMode();
                _displayShortNameGetter = () => value;
            }
        }

        /// <summary>Sets the <see cref="DisplayShortName"/> value getter.</summary>
        /// <param name="displayShortNameGetter">The <see cref="DisplayShortName"/> value getter.</param>
        /// <remarks>This method is typically used to provide localizable <see cref="DisplayShortName"/> value.</remarks>
        public void SetDisplayShortName(Func<string> displayShortNameGetter)
        {
            VerifyDesignMode();
            _displayShortNameGetter = displayShortNameGetter;
        }

        private Func<string> _displayNameGetter;
        /// <summary>Gets or sets a value that is used for display the name in the UI.</summary>
		/// <returns>A value that is used for display the name in the UI.</returns>
        /// <remarks>The <see cref="DisplayName"/> is typically used as the field label for a UI element that is bound to this <see cref="Column"/>.</remarks>
        public string DisplayName
        {
            get { return _displayNameGetter != null ? _displayNameGetter() : Name; }
            set
            {
                VerifyDesignMode();
                _displayNameGetter = () => value;
            }
        }

        /// <summary>Sets the <see cref="DisplayName"/> value getter.</summary>
        /// <param name="displayNameGetter">The <see cref="DisplayName"/> value getter.</param>
        /// <remarks>This method is typically used to provide localizable <see cref="DisplayName"/> value.</remarks>
        public void SetDisplayName(Func<string> displayNameGetter)
        {
            VerifyDesignMode();
            _displayNameGetter = displayNameGetter;
        }

        private Func<string> _displayDescriptionGetter;
        /// <summary>Gets or sets a value that is used to display a description in the UI.</summary>
		/// <returns>The value that is used to display a description in the UI.</returns>
        /// <remarks>The <see cref="DisplayDescription"/> is typically used as a tooltip or description UI element that is bound to this <see cref="Column"/>.</remarks>
        public string DisplayDescription
        {
            get { return _displayDescriptionGetter != null ? _displayDescriptionGetter() : null; }
            set
            {
                VerifyDesignMode();
                _displayDescriptionGetter = () => value;
            }
        }

        /// <summary>Sets the <see cref="DisplayDescription"/> value getter.</summary>
        /// <param name="displayDescriptionGetter">The <see cref="DisplayDescription"/> value getter.</param>
        /// <remarks>This method is typically used to provide localizable <see cref="DisplayDescription"/> value.</remarks>
        public void SetDisplayDescription(Func<string> displayDescriptionGetter)
        {
            VerifyDesignMode();
            _displayDescriptionGetter = displayDescriptionGetter;
        }

        private Func<string> _displayPromptGetter;
        /// <summary>Gets or sets a value that will be used to set the watermark for prompts in the UI.</summary>
		/// <returns>A value that will be used to display a watermark in the UI.</returns>
        public string DisplayPrompt
        {
            get { return _displayPromptGetter != null ? _displayPromptGetter() : null; }
            set
            {
                VerifyDesignMode();
                _displayPromptGetter = () => value;
            }
        }

        /// <summary>Sets the <see cref="DisplayPrompt"/> value getter.</summary>
        /// <param name="displayPromptGetter">The <see cref="DisplayPrompt"/> value getter.</param>
        /// <remarks>This method is typically used to provide localizable <see cref="DisplayPrompt"/> value.</remarks>
        public void SetDisplayPrompt(Func<string> displayPromptGetter)
        {
            VerifyDesignMode();
            _displayPromptGetter = displayPromptGetter;
        }

        /// <summary>
        /// Gets a value indicates whether a default comparer exists for underlying data type.
        /// </summary>
        public abstract bool HasDefaultComparer { get; }

        /// <summary>
        /// Compares values of two <see cref="DataRow"/> objects.
        /// </summary>
        /// <param name="x">The left <see cref="DataRow"/>.</param>
        /// <param name="y">The right <see cref="DataRow"/>.</param>
        /// <returns>0 if two values are equal, 1 if left value is greater than right value, otherwise -1.</returns>
        public int Compare(DataRow x, DataRow y)
        {
            return Compare(x, y, SortDirection.Ascending);
        }

        /// <summary>
        /// Compares values of two <see cref="DataRow"/> objects, with specified sorting order.
        /// </summary>
        /// <param name="x">The left <see cref="DataRow"/>.</param>
        /// <param name="y">The right <see cref="DataRow"/>.</param>
        /// <param name="direction">The specified sorting order.</param>
        /// <returns>0 if two values are equal, 1 if left value is greater than right value, otherwise -1.</returns>
        public abstract int Compare(DataRow x, DataRow y, SortDirection direction);

        internal abstract void InitAsChild(Column parentColumn);

        /// <summary>
        /// Gets a value indicates whether this column stores concrete data values.
        /// </summary>
        public abstract bool IsConcrete { get; }

        internal abstract void TryMakeConcrete();

        internal abstract void RefreshComputation(DataRow dataRow);

        internal abstract Column CreateBackup(Model model);

        internal abstract IColumnComparer ToColumnComparer(SortDirection direction);

        /// <summary>
        /// Gets a value indicates whether this column has value comparer.
        /// </summary>
        public abstract bool HasValueComparer { get; }

        internal abstract Column PerformTranslateTo(Model model);

        /// <summary>
        /// Gets a value indicates whether this column is readonly for specified <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dataRow">The specified <see cref="DataRow"/>.</param>
        /// <returns><see langword="true" /> if this column is readonly for specified <see cref="DataRow"/>, otherwise
        /// <see langword="false"/>.</returns>
        public abstract bool IsReadOnly(DataRow dataRow);

        /// <summary>
        /// Gets the default value of the column.
        /// </summary>
        /// <returns>The default value.</returns>
        public abstract object GetDefaultValue();

        /// <summary>
        /// Sets default value for this column.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="name">Name of the default.</param>
        /// <param name="description">Description of the default.</param>
        public void SetDefaultValue(object defaultValue, string name, string description)
        {
            PerformSetDefaultValue(defaultValue, name, description);
        }

        internal abstract void PerformSetDefaultValue(object defaultValue, string name, string description);

        /// <summary>
        /// Calculates the hash code for specified <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dataRow">The specified <see cref="DataRow"/>.</param>
        /// <returns>The calculated hash code.</returns>
        /// <remarks>This method is used to compare the equality of two <see cref="DataRow"/>'s data values.</remarks>
        public abstract int GetHashCode(DataRow dataRow);

        /// <summary>
        /// Determines whether two data values are equal.
        /// </summary>
        /// <param name="dataRow">The source DataRow.</param>
        /// <param name="otherColumn">The target column.</param>
        /// <param name="otherDataRow">The target DataRow.</param>
        /// <returns></returns>
        public abstract bool Equals(DataRow dataRow, Column otherColumn, DataRow otherDataRow);

        internal void SetIdentity(Identity identity)
        {
            VerifyDesignMode();

            AddOrUpdate(identity);
            var model = ParentModel;
            if (model.ContainsAddon(((IAddon)identity).Key))
                throw new InvalidOperationException(DiagnosticMessages.Model_MultipleIdentityColumn);
            model.Add(identity);
        }

        /// <summary>
        /// Gets a value indicates whether values of this column must be unique.
        /// </summary>
        public bool IsUnique
        {
            get
            {
                var model = ParentModel;
                if (model == null)
                    return false;

                if (IsIdentity)
                    return true;

                IReadOnlyList<Column> primaryKey = model.PrimaryKey;
                if (primaryKey.Count == 1 && primaryKey[0] == this)
                    return true;

                var dbUniqueConstraints = model.GetAddons<DbUniqueConstraint>();
                foreach (var dbUniqueConstraint in dbUniqueConstraints)
                {
                    if (dbUniqueConstraint.Columns.Count == 1 && dbUniqueConstraint.Columns[0].Column == this)
                        return true;
                }

                return false;
            }
        }

        private LogicalDataType _logicalDataType = LogicalDataType.Custom;
        /// <summary>
        /// Gets or sets the logical data type for this column.
        /// </summary>
        public LogicalDataType LogicalDataType
        {
            get { return _logicalDataType; }
            set
            {
                VerifyDesignMode();
                _logicalDataType = value;
            }
        }

        /// <summary>
        /// Gets the parent model of this column.
        /// </summary>
        public Model ParentModel
        {
            get { return Parent; }
        }

        /// <summary>
        /// Gets the owner model of this column.
        /// </summary>
        public Model OwnerModel
        {
            get { return ParentModel?.OwnerModel; }
        }
    }
}
