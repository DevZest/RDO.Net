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
    /// Represents a column of <see cref="Model"/>.
    /// </summary>
    public abstract class Column : ModelMember, IColumns, IComparer<DataRow>
    {
        protected Column()
        {
        }

        /// <summary>
        /// Gets the original owner type of this <see cref="Column"/>.
        /// </summary>
        /// <remarks>This property forms <see cref="ColumnId"/> of this <see cref="Column"/>.</remarks>
        public Type OriginalDeclaringType { get; internal set; }

        public new string Name
        {
            get { return base.Name; }
        }

        /// <summary>
        /// Gets the original name of this <see cref="Column"/>.
        /// </summary>
        /// <remarks>This property forms <see cref="ColumnId"/> of this <see cref="Column"/>.</remarks>
        public string OriginalName { get; internal set; }

        public ColumnId Id
        {
            get { return new ColumnId(DeclaringType, Name); }
        }

        public ColumnId OriginalId
        {
            get { return new ColumnId(OriginalDeclaringType, OriginalName); }
        }

        /// <summary>
        /// Gets a value indicates whether this column is part of primary key.
        /// </summary>
        public abstract bool IsPrimaryKey { get; }

        public virtual bool IsIdentity
        {
            get { return false; }
        }

        /// <summary>Gets the zero-based position of the column in the <see cref="Model.Columns"/> collection.</summary>
        /// <remarks>If the column is not added to any <see cref="Model"/>, -1 is returned.</remarks>
        public int Ordinal { get; internal set; } = -1;

        private string _dbColumnName;
        /// <summary>Gets or sets the column name in database.</summary>
        /// <inheritdoc cref="ModelMember.VerifyDesignMode" select="exception"/>
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
                OriginalDeclaringType = declaringType;
            if (string.IsNullOrEmpty(OriginalName))
                OriginalName = name;

            _initializer = initializer;
            Ordinal = ParentModel.Add(this);

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

        public abstract ColumnExpression GetExpression();

        public abstract DbExpression DbComputedExpression { get; }

        public abstract IColumns BaseColumns { get; }

        /// <summary>Gets the data type of this <see cref="Column"/>.</summary>
        public abstract Type DataType { get; }

        internal abstract void InsertRow(DataRow dataRow);

        internal abstract void RemoveRow(DataRow dataRow);

        /// <inheritdoc/>
        public override string ToString()
        {
            return IsExpression ? "[Expression]" : Name;
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

        private bool _jsonIgnore = false;
        internal void JsonIgnore()
        {
            _jsonIgnore = true;
        }

        public virtual bool IsSerializable
        {
            get { return _jsonIgnore ? false : IsSerializableOverride; }
        }

        internal abstract bool IsSerializableOverride { get; }

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

        public abstract bool IsDbComputed { get; }

        /// <summary>Determines whether the value of given <see cref="DataRow"/> is null.</summary>
        /// <param name="dataRow">The <see cref="DataRow"/> object.</param>
        /// <returns><see langword="true"/> if the value of given <see cref="DataRow"/> is null, otherwise <see langword="false"/>.</returns>
        public abstract bool IsNull(DataRow dataRow);

        /// <summary>Creates <see cref="ColumnMapping"/> from given <see cref="Column"/>.</summary>
        /// <param name="column">The source <see cref="Column"/>.</param>
        /// <returns>The created <see cref="ColumnMapping"/>.</returns>
        public abstract ColumnMapping MapFrom(Column column);

        public abstract object GetValue(DataRow dataRow, bool beforeEdit = false);

        public abstract void SetValue(DataRow dataRow, object value, bool beforeEdit = false);

        #region IColumnSet

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        bool IColumns.Contains(Column column)
        {
            return column == this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<Column>.Count
        {
            get { return 1; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<Column> IEnumerable<Column>.GetEnumerator()
        {
            yield return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        bool IColumns.IsSealed
        {
            get { return true; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IColumns IColumns.Seal()
        {
            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IColumns IColumns.Add(Column value)
        {
            value.VerifyNotNull(nameof(value));
            if (value == this)
                return this;
            return Columns.New(this, value);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IColumns IColumns.Remove(Column value)
        {
            value.VerifyNotNull(nameof(value));
            if (value == this)
                return Columns.Empty;
            else
                return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IColumns IColumns.Clear()
        {
            return Columns.Empty;
        }

        #endregion

        internal abstract void BeginEdit(DataRow dataRow);

        internal abstract void EndEdit(DataRow dataRow);

        internal abstract void CopyValue(DataRow sourceDataRow, Column targetColumn, DataRow targetDataRow);

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

        public abstract bool HasDefaultComparer { get; }

        public int Compare(DataRow x, DataRow y)
        {
            return Compare(x, y, SortDirection.Ascending);
        }

        public abstract int Compare(DataRow x, DataRow y, SortDirection direction);

        internal abstract void InitAsChild(Column parentColumn);

        public abstract bool IsConcrete { get; }

        internal abstract void TryMakeConcrete();

        internal abstract void RefreshComputation(DataRow dataRow);

        internal abstract Column CreateBackup(Model model);

        internal abstract IColumnComparer ToColumnComparer(SortDirection direction);

        public abstract bool HasValueComparer { get; }

        private string _relativeName;
        public string RelativeName
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return Name;
                if (_relativeName == null)
                    _relativeName = Name.LastPart('.');
                return _relativeName;
            }
        }

        internal abstract Column PerformTranslateTo(Model model);

        public abstract bool IsReadOnly(DataRow dataRow);

        public abstract object GetDefaultValue();

        public abstract void SetDefaultObject(object defaultValue, string name, string description);

        public abstract int GetHashCode(DataRow dataRow);

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
    }
}
