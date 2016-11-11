using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DevZest.Data
{
    public abstract partial class Model : ModelMember, IModelSet
    {
        #region RegisterColumn

        static AccessorManager<Model, Column> s_columnManager = new AccessorManager<Model, Column>();

        /// <summary>
        /// Registers a new column which has a default constructor.
        /// </summary>
        /// <typeparam name="TModel">The type of model which the column is registered on.</typeparam>
        /// <typeparam name="TColumn">The type of the column.</typeparam>
        /// <param name="getter">The lambda expression of the column getter.</param>
        /// <param name="initializer">The additional initializer.</param>
        /// <returns>Accessor of the column.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not a valid getter.</exception>
        public static Accessor<TModel, TColumn> RegisterColumn<TModel, TColumn>(Expression<Func<TModel, TColumn>> getter, Action<TColumn> initializer = null)
            where TModel : Model
            where TColumn : Column, new()
        {
            var columnAttributes = VerifyPropertyGetter(getter);

            return s_columnManager.Register(getter, a => CreateColumn(a, initializer, columnAttributes));
        }

        /// <summary>
        /// Registers a column from existing column accessor, without inheriting its <see cref="ColumnKey"/> value.
        /// </summary>
        /// <typeparam name="TModel">The type of model which the column is registered on.</typeparam>
        /// <typeparam name="TFromModel">The type of the existing accessor's model.</typeparam>
        /// <typeparam name="TColumn">The type of the column.</typeparam>
        /// <param name="getter">The lambda expression of the column getter.</param>
        /// <param name="fromAccessor">The existing accessor.</param>
        /// <param name="initializer">The additional initializer.</param>
        /// <returns>Accessor of the column.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not an valid getter.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="fromAccessor"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="initializer"/> is null.</exception>
        public static Accessor<TModel, TColumn> RegisterColumn<TModel, TFromModel, TColumn>(Expression<Func<TModel, TColumn>> getter,
            Accessor<TFromModel, TColumn> fromAccessor,
            Action<TColumn> initializer = null)
            where TModel : Model
            where TFromModel : Model
            where TColumn : Column, new()
        {
            var columnAttributes = VerifyPropertyGetter(getter);
            Utilities.Check.NotNull(fromAccessor, nameof(fromAccessor));

            var result = s_columnManager.Register(getter, a => CreateColumn(a, fromAccessor, initializer, columnAttributes));
            result.OriginalOwnerType = fromAccessor.OriginalOwnerType;
            result.OriginalName = fromAccessor.OriginalName;
            return result;
        }

        private static IEnumerable<ColumnAttribute> VerifyPropertyGetter<TModel, TColumn>(Expression<Func<TModel, TColumn>> getter)
        {
            Utilities.Check.NotNull(getter, nameof(getter));
            var memberExpr = getter.Body as MemberExpression;
            if (memberExpr == null)
                throw new ArgumentException(Strings.Accessor_InvalidGetter, nameof(getter));

            var propertyInfo = typeof(TModel).GetProperty(memberExpr.Member.Name, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyInfo == null)
                throw new ArgumentException(Strings.Accessor_InvalidGetter, nameof(getter));

            return propertyInfo.GetCustomAttributes<ColumnAttribute>();
        }

        private static T CreateColumn<TModel, T>(Accessor<TModel, T> accessor, Action<T> initializer, IEnumerable<ColumnAttribute> columnAttributes)
            where TModel : Model
            where T : Column, new()
        {
            var result = Column.Create<T>(accessor.OwnerType, accessor.Name);
            result.Construct(accessor.Parent, accessor.OwnerType, accessor.Name, ColumnKind.User, null, GetColumnInitializer(initializer, columnAttributes));
            return result;
        }

        private static Action<T> GetColumnInitializer<T>(Action<T> initializer, IEnumerable<ColumnAttribute> columnAttributes)
            where T : Column, new()
        {
            return x =>
            {
                if (initializer != null)
                    initializer(x);
                InitializeColumnAttributes(x, columnAttributes);
            };
        }

        private static void InitializeColumnAttributes(Column column, IEnumerable<ColumnAttribute> columnAttributes)
        {
            if (columnAttributes == null)
                return;
            foreach (var columnAttribute in columnAttributes)
            {
                columnAttribute.Initialize(column);

                var columnValidator = columnAttribute as IColumnValidator;
                if (columnValidator != null)
                {
                    var validator = columnValidator.GetValidator(column);
                    var model = column.ParentModel;
                    Debug.Assert(model != null);
                    model._validators.Add(validator);
                }
            }
        }

        private static T CreateColumn<TModel, TModelFrom, T>(Accessor<TModel, T> accessor, Accessor<TModelFrom, T> fromAccessor, Action<T> initializer, IEnumerable<ColumnAttribute> columnAttributes)
            where TModel : Model
            where T : Column, new()
        {
            var result = Column.Create<T>(fromAccessor.OriginalOwnerType, fromAccessor.OriginalName);
            result.Construct(accessor.Parent, accessor.OwnerType, accessor.Name, ColumnKind.User, fromAccessor.Initializer, GetColumnInitializer(initializer, columnAttributes));
            return result;
        }

        #endregion

        #region RegisterColumnList

        static AccessorManager<Model, ColumnList> s_columnListManager = new AccessorManager<Model, ColumnList>();

        /// <summary>
        /// Registers a column list.
        /// </summary>
        /// <typeparam name="TModel">The type of model which the column is registered on.</typeparam>
        /// <typeparam name="T">The type of the column contained by the column list.</typeparam>
        /// <param name="getter">The lambda expression of the column list getter.</param>
        /// <returns>Accessor of the column list.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not a valid getter.</exception>
        public static Accessor<TModel, ColumnList<T>> RegisterColumnList<TModel, T>(Expression<Func<TModel, ColumnList<T>>> getter)
            where TModel : Model
            where T : Column
        {
            Utilities.Check.NotNull(getter, nameof(getter));

            return s_columnListManager.Register(getter, a => CreateColumnList(a), null);
        }

        private static ColumnList<T> CreateColumnList<TModel, T>(Accessor<TModel, ColumnList<T>> accessor)
            where TModel : Model
            where T : Column
        {
            var result = new ColumnList<T>();
            result.ConstructModelMember(accessor.Parent, accessor.OwnerType, accessor.Name);
            return result;
        }

        #endregion

        #region RegisterChildModel

        static AccessorManager<Model, Model> s_childModelManager = new AccessorManager<Model, Model>();

        /// <summary>
        /// Registers a child model.
        /// </summary>
        /// <typeparam name="TModel">The type of model which the child model is registered on.</typeparam>
        /// <typeparam name="TChildModel">The type of the child model.</typeparam>
        /// <param name="getter">The lambda expression of the child model getter.</param>
        /// <param name="childRefGetter">The relationship between child model and parent model.</param>
        /// <returns>Accessor of the child model.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not a valid getter.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="childRefGetter"/> is <see langword="null"/>.</exception>
        public static Accessor<TModel, TChildModel> RegisterChildModel<TModel, TModelKey, TChildModel>(Expression<Func<TModel, TChildModel>> getter,
            Func<TChildModel, TModelKey> childRefGetter, Action<ColumnMappingsBuilder, TChildModel, TModel> childColumnsBuilder = null)
            where TModel : Model<TModelKey>
            where TModelKey : ModelKey
            where TChildModel : Model, new()
        {
            Utilities.Check.NotNull(getter, nameof(getter));
            Utilities.Check.NotNull(childRefGetter, nameof(childRefGetter));

            return s_childModelManager.Register(getter, a => CreateChildModel<TModel, TModelKey, TChildModel>(a, childRefGetter, childColumnsBuilder), null);
        }

        private static TChildModel CreateChildModel<TModel, TModelKey, TChildModel>(Accessor<TModel, TChildModel> accessor,
            Func<TChildModel, TModelKey> childRefGetter, Action<ColumnMappingsBuilder, TChildModel, TModel> parentMappingsBuilder)
            where TModel : Model<TModelKey>
            where TModelKey : ModelKey
            where TChildModel : Model, new()
        {
            TChildModel result = new TChildModel();
            var parentModel = accessor.Parent;
            var parentRelationship = childRefGetter(result).GetRelationship(parentModel.PrimaryKey);
            var parentMappings = GetParentMappings(parentRelationship, parentMappingsBuilder, result, parentModel);
            result.Construct(parentModel, accessor.OwnerType, accessor.Name, parentRelationship, parentMappings);
            return result;
        }

        private static ReadOnlyCollection<ColumnMapping> GetParentMappings<TChildModel, TParentModel>(ReadOnlyCollection<ColumnMapping> parentRelationship,
            Action<ColumnMappingsBuilder, TChildModel, TParentModel> parentMappingsBuilderAction, TChildModel childModel, TParentModel parentModel)
            where TChildModel : Model
            where TParentModel : Model
        {
            if (parentMappingsBuilderAction == null)
                return parentRelationship;

            var parentMappingsBuilder = new ColumnMappingsBuilder(childModel, parentModel);
            var parentMappings = parentMappingsBuilder.Build(x => parentMappingsBuilderAction(x, childModel, parentModel));

            var result = new ColumnMapping[parentRelationship.Count + parentMappings.Count];
            for (int i = 0; i < parentRelationship.Count; i++)
                result[i] = parentRelationship[i];
            for (int i = 0; i < parentMappings.Count; i++)
                result[i + parentRelationship.Count] = parentMappings[i];
            return new ReadOnlyCollection<ColumnMapping>(result);
        }

        private void Construct(Model parentModel, Type ownerType, string name, ReadOnlyCollection<ColumnMapping> parentRelationship,
            ReadOnlyCollection<ColumnMapping> parentMappings)
        {
            this.ConstructModelMember(parentModel, ownerType, name);

            Debug.Assert(parentMappings != null);
            Debug.Assert(parentRelationship != null);
            ParentRelationship = parentRelationship;
            ParentMappings = parentMappings;
        }

        /// <summary>
        /// Gets the column mappings between its parent model and this model.
        /// </summary>
        internal ReadOnlyCollection<ColumnMapping> ParentRelationship { get; private set; }

        internal ReadOnlyCollection<ColumnMapping> ParentMappings { get; private set; }

        #endregion

        protected Model()
        {
            Columns = new ColumnCollection(this);
            ChildModels = new ModelCollection(this);

            Initialize(s_columnManager);
            Initialize(s_columnListManager);
        }

        internal override void ConstructModelMember(Model parentModel, Type ownerType, string name)
        {
            base.ConstructModelMember(parentModel, ownerType, name);
            ParentModel.ChildModels.Add(this);
            Depth = ParentModel.Depth + 1;
        }

        internal int Depth { get; private set; }

        /// <summary>
        /// Gets a value indicates whether child models are initialized.
        /// </summary>
        /// <remarks>Unlike <see cref="Column"/> and <see cref="ColumnList"/>,
        /// child models are not initialized by default. This design decision is to deal with the situation when recursive child models registered.
        /// <see cref="EnsureChildModelsInitialized"/> will be called automatically when creating the first <see cref="DataRow"/> the query builder.
        /// </remarks>
        internal bool AreChildModelsInitialized { get; private set; }

        /// <summary>
        /// Ensures child models are initialized.
        /// </summary>
        /// <remarks>Unlike <see cref="Column"/> and <see cref="ColumnList"/>,
        /// child models are not initialized by default. This design decision is to deal with the situation when recursive child models registered.
        /// <see cref="EnsureChildModelsInitialized"/> will be called automatically when creating the first <see cref="DataRow"/>.
        /// </remarks>
        internal void EnsureChildModelsInitialized()
        {
            if (AreChildModelsInitialized)
                return;

            AreChildModelsInitialized = true;

            Initialize(s_childModelManager);
            if (DataSource != null && DataSource.Kind == DataSourceKind.DataSet)
            {
                foreach (var model in ChildModels)
                {
                    var modelType = model.GetType();
                    var invoker = s_createDataSetInvokers.GetOrAdd(modelType, t => BuildCreateDataSetInvoker(t));
                    invoker(model);
                }
                OnChildModelsInitialized();
                foreach (var column in Columns)
                    column.InitValueManager();
            }
        }

        protected virtual void OnChildModelsInitialized()
        {
        }

        private static ConcurrentDictionary<Type, Action<Model>> s_createDataSetInvokers = new ConcurrentDictionary<Type, Action<Model>>();

        private static DataSet<T> _CreateDataSet<T>(Model model)
            where T : Model, new()
        {
            return DataSet<T>.Create((T)model);
        }

        private static Action<Model> BuildCreateDataSetInvoker(Type modelType)
        {
            var methodInfo = typeof(Model).GetStaticMethodInfo(nameof(_CreateDataSet));
            methodInfo = methodInfo.MakeGenericMethod(modelType);
            var paramModel = Expression.Parameter(typeof(Model), methodInfo.GetParameters()[0].Name);
            var call = Expression.Call(methodInfo, paramModel);
            return Expression.Lambda<Action<Model>>(call, paramModel).Compile();
        }

        private void Initialize<T>(AccessorManager<Model, T> accessorManager)
            where T : class
        {
            var accessors = accessorManager.GetAll(this.GetType());
            foreach (var accessor in accessors)
                accessor.Construct(this);
        }

        protected internal ColumnCollection Columns { get; private set; }

        protected internal ModelCollection ChildModels { get; private set; }

        private List<IValidator> _validators = new List<IValidator>();
        protected internal IList<IValidator> Validators
        {
            get { return _validators; }
        }

        internal ValidationMessageCollection Validate(DataRow dataRow)
        {
            var result = ValidationMessageCollection.Empty;
            foreach (var validator in Validators)
            {
                var isValid = validator.IsValidCondition[dataRow];
                if (isValid == true)
                    continue;

                if (result == ValidationMessageCollection.Empty)
                    result = new ValidationMessageCollection();
                var message = validator.Message[dataRow];
                result.Add(new ValidationMessage(validator.Id, validator.Severity, validator.Columns, message));
            }

            return result;
        }

        public ModelKey PrimaryKey
        {
            get { return GetPrimaryKeyCore(); }
        }

        internal virtual ModelKey GetPrimaryKeyCore()
        {
            return null;
        }

        protected internal DataSource DataSource { get; private set; }

        internal void SetDataSource(DataSource dataSource)
        {
            Debug.Assert(dataSource != null);
            Debug.Assert(DataSource == null);

            DataSource = dataSource;
            DataSet = dataSource as DataSet;
            Columns.Seal();
        }

        protected internal sealed override bool DesignMode
        {
            get
            {
                if (DataSource == null)
                    return true;
                if (DataSource.Kind != DataSourceKind.DataSet)
                    return false;
                return DataSet.Count == 0;
            }
        }

        public DataSet DataSet { get; private set; }

        private int _ordinal = -1;
        internal int Ordinal
        {
            get { return _ordinal; }
            set { _ordinal = value; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        bool IModelSet.Contains(Model model)
        {
            return model == this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<Model>.Count
        {
            get { return 1; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        Model IReadOnlyList<Model>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<Model> IEnumerable<Model>.GetEnumerator()
        {
            yield return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        internal static T Clone<T>(T prototype, bool setDataSource)
            where T : Model, new()
        {
            T result = new T();
            result.InitializeClone(prototype, setDataSource);
            return result;
        }

        private Model _prototype;
        internal Model Prototype
        {
            get { return _prototype == null ? this : _prototype.Prototype; }
        }

        internal Model Clone(bool setDataSource)
        {
            Model result = (Model)Activator.CreateInstance(this.GetType());
            result.InitializeClone(this, setDataSource);
            return result;
        }

        private void InitializeClone(Model prototype, bool setDataSource)
        {
            Debug.Assert(prototype != null && prototype != this);
            _prototype = prototype;
            InitializeColumnLists(prototype);
            if (setDataSource && prototype.DataSource != null)
                SetDataSource(prototype.DataSource);
        }

        private void InitializeColumnLists(Model prototype)
        {
            var accessors = s_columnListManager.GetAll(this.GetType());
            foreach (var accessor in accessors)
            {
                var columnList = accessor.GetProperty(this);
                var sourceColumnList = accessor.GetProperty(prototype);
                columnList.Initialize(sourceColumnList);
            }
        }

        private IIndexConstraint _clusteredIndex;
        private IIndexConstraint ClusteredIndex
        {
            get { return _clusteredIndex; }
            set
            {
                if (_clusteredIndex != null && _clusteredIndex != value)
                    throw new InvalidOperationException(Strings.Model_MultipleClusteredIndex(_clusteredIndex.SystemName));
                else
                    _clusteredIndex = value;
            }
        }

        internal void ChangeClusteredIndexAsNonClustered()
        {
            if (_clusteredIndex == null)
                return;

            _clusteredIndex.AsNonClustered();
            _clusteredIndex = null;
        }

        internal void AddDbTableConstraint(DbTableConstraint constraint, bool overwritable)
        {
            Debug.Assert(constraint != null);

            if (!overwritable && this.ContainsInterceptor(constraint.FullName))
                throw new InvalidOperationException(Strings.Model_DuplicateConstraintName(constraint.SystemName));

            var index = constraint as IIndexConstraint;
            if (index != null && index.IsClustered)
                ClusteredIndex = index;

            this.AddOrUpdateInterceptor(constraint);
        }

        internal KeyOutput CreateSequentialKey()
        {
            return new KeyOutput(this, true);
        }

        protected internal virtual string DbAlias
        {
            get { return this.GetType().Name; }
        }

        private Dictionary<string, ModelMember> _members = new Dictionary<string, ModelMember>();
        internal void AddMember(ModelMember member)
        {
            Debug.Assert(member != null);
            Debug.Assert(!string.IsNullOrEmpty(member.Name));
            _members.Add(member.Name, member);
        }

        private bool ContainsMember(string memberName)
        {
            return string.IsNullOrEmpty(memberName) ? false : _members.ContainsKey(memberName);
        }

        internal ModelMember this[string memberName]
        {
            get { return ContainsMember(memberName) ? _members[memberName] : null; }
        }

        protected internal Identity GetIdentity(bool isTempTable)
        {
            var results = GetInterceptors<Identity>();
            foreach (var result in results)
            {
                if (result.IsTempTable == isTempTable)
                    return result;
            }
            return null;
        }

        internal const string SYS_ROW_ID_COL_NAME = "sys_row_id";
        private const string SYS_PARENT_ROW_ID_COL_NAME = "sys_parent_row_id";

        internal void AddTempTableIdentity()
        {
            _Int32 identityColumn = AddSysRowIdColumn(false);
            var identity = identityColumn.Identity(1, 1, true);

            var primaryKeyConstraint = GetInterceptor<PrimaryKeyConstraint>();
            if (primaryKeyConstraint == null)
                AddDbTableConstraint(new PrimaryKeyConstraint(this, null, true, () => GetIdentityOrderByList(identity)), false);
            else
            {
                ChangeClusteredIndexAsNonClustered();
                AddDbTableConstraint(new UniqueConstraint(null, true, GetIdentityOrderByList(identity)), false);
            }
        }

        private static IList<ColumnSort> GetIdentityOrderByList(Identity identity)
        {
            return new ColumnSort[] { new ColumnSort(identity.Column, identity.Increment > 0 ? SortDirection.Ascending : SortDirection.Descending) };
        }

        internal DbFromClause FromClause
        {
            get
            {
                var dbSet = DataSource as IDbSet;
                return dbSet == null ? null : dbSet.FromClause;
            }
        }

        private _Int32 AddSysRowIdColumn(bool isParent)
        {
            _Int32 result = new _Int32();
            result.Initialize(this, this.GetType(), isParent ? SYS_PARENT_ROW_ID_COL_NAME : SYS_ROW_ID_COL_NAME,
                isParent ? ColumnKind.SystemParentRowId : ColumnKind.SystemRowId, null);
            return result;
        }

        private class SysRowId : IInterceptor
        {
            public SysRowId(_Int32 column)
            {
                Column = column;
            }

            public string FullName
            {
                get { return this.GetType().FullName; }
            }

            public _Int32 Column { get; private set; }
        }

        private sealed class SysParentRowId : IInterceptor
        {
            public SysParentRowId(_Int32 column)
            {
                Column = column;
            }

            public string FullName
            {
                get { return this.GetType().FullName; }
            }

            public _Int32 Column { get; private set; }
        }

        internal _Int32 GetSysRowIdColumn(bool createIfNotExist)
        {
            var sysRowId = GetInterceptor<SysRowId>();
            if (sysRowId == null)
            {
                if (!createIfNotExist)
                    return null;

                sysRowId = new SysRowId(AddSysRowIdColumn(false));
                AddOrUpdateInterceptor(sysRowId);
            }
            return sysRowId.Column;
        }

        internal _Int32 GetSysParentRowIdColumn(bool createIfNotExist)
        {
            var sysParentRowId = GetInterceptor<SysParentRowId>();
            if (sysParentRowId == null)
            {
                if (!createIfNotExist)
                    return null;

                sysParentRowId = new SysParentRowId(AddSysRowIdColumn(true));
                AddOrUpdateInterceptor(sysParentRowId);
            }
            return sysParentRowId.Column;
        }

        protected void Unique(string constraintName, bool isClustered, params ColumnSort[] orderByList)
        {
            Utilities.Check.NotNull(orderByList, nameof(orderByList));
            if (orderByList.Length == 0)
                throw new ArgumentException(Strings.Model_EmptyColumns, nameof(orderByList));

            for (int i = 0; i < orderByList.Length; i++)
            {
                var column = orderByList[i].Column;
                if (column == null || column.ParentModel != this)
                    throw new ArgumentException(Strings.Model_VerifyChildColumn, string.Format(CultureInfo.InvariantCulture, nameof(orderByList) + "[{0}]", i));
            }

            AddDbTableConstraint(new UniqueConstraint(constraintName, isClustered, orderByList), false);
        }

        protected void Check(string constraintName, _Boolean condition)
        {
            Utilities.Check.NotNull(condition, nameof(condition));

            AddDbTableConstraint(new CheckConstraint(constraintName, condition.DbExpression), false);
        }

        internal bool AllowsKeyUpdate(bool value)
        {
            var result = IsKeyUpdateAllowed;
            IsKeyUpdateAllowed = value;
            return result;
        }

        internal bool IsKeyUpdateAllowed { get; private set; }

        private bool _oldIsKeyUpdateAllowed;
        private bool _oldIsIdentityGenerationDisabled;
        internal void EnterDataSetInitialization()
        {
            _oldIsKeyUpdateAllowed = AllowsKeyUpdate(true);
            _oldIsIdentityGenerationDisabled = IsIdentityGenerationDisabled;
            IsIdentityGenerationDisabled = true;
        }

        internal void ExitDataSetInitialization()
        {
            AllowsKeyUpdate(_oldIsKeyUpdateAllowed);
            IsIdentityGenerationDisabled = _oldIsIdentityGenerationDisabled;
        }

        internal bool IsIdentityGenerationDisabled { get; private set; }

        internal DbTable<KeyOutput> SequentialKeyTempTable { get; set; }

        public override string ToString()
        {
            return this.GetType().Name + ": [" + string.Join(", ", Columns.Select(x => x.Name)) + "]";
        }

        internal KeyOutput ParentSequentialKeyModel
        {
            get
            {
                var parentModel = ParentModel;
                if (parentModel == null)
                    return null;

                var parentQuery = parentModel.DataSource as IDbSet;
                if (parentQuery == null || parentQuery.Kind != DataSourceKind.DbQuery)
                    return null;

                var result = parentQuery.QueryStatement.SequentialKeyTempTable._;
                return result;
            }
        }

        internal IList<ColumnMapping> GetParentRelationship(IList<ColumnMapping> columnMappings)
        {
            var parentRelationship = ParentRelationship;
            if (parentRelationship == null)
                return null;

            var result = new ColumnMapping[parentRelationship.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var mapping = parentRelationship[i];
                var source = GetSource(mapping.SourceExpression, columnMappings);
                if (source == null)
                    throw new InvalidOperationException(Strings.ChildColumnNotExistInColumnMappings(mapping.SourceExpression));
                result[i] = new ColumnMapping(source, mapping.Target);
            }

            return result;
        }

        private static DbExpression GetSource(DbExpression target, IList<ColumnMapping> relationship)
        {
            foreach (var mapping in relationship)
            {
                if (mapping.TargetExpression == target)
                    return mapping.SourceExpression;
            }
            return null;
        }

        protected internal virtual DataSet<T> NewDataSetValue<T>(_DataSet<T> dataSetColumn, int rowOrdinal)
            where T : Model, new()
        {
            return DataSet<T>.New();
        }

        protected internal virtual void OnRowAdded(DataRow e)
        {
        }

        protected internal virtual void OnRowUpdated(DataRow e)
        {
        }

        protected internal virtual void OnRowRemoved(DataRowRemovedEventArgs e)
        {
        }

        internal DataRow EditingRow { get; private set; }

        internal void BeginEdit(DataRow dataRow)
        {
            Debug.Assert(EditingRow == null && dataRow != null);
            EditingRow = dataRow;
            foreach (var column in Columns)
                column.BeginEdit(dataRow);
        }

        internal void EndEdit(DataRow dataRow)
        {
            Debug.Assert(EditingRow != null);
            Debug.Assert(EditingRow == DataRow.Placeholder || EditingRow == dataRow);

            dataRow.BeginUpdate();
            foreach (var column in Columns)
                column.EndEdit(dataRow);
            EditingRow = null;
            dataRow.EndUpdate();
        }

        internal void CancelEdit()
        {
            Debug.Assert(EditingRow != null);
            EditingRow = null;
        }

        internal void LockEditingRow(Action action)
        {
            var editingRow = EditingRow;
            EditingRow = DataRow.Placeholder;
            action();
            EditingRow = editingRow;
        }
    }
}
