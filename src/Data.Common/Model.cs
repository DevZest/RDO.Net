using DevZest.Data.Primitives;
using DevZest.Data.Resources;
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
    public abstract class Model : ModelMember, IModelSet
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

            var propertyInfo = typeof(TModel).GetProperty(memberExpr.Member.Name, BindingFlags.Public | BindingFlags.Instance);
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
                columnAttribute.Initialize(column);
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
        /// <param name="relationship">The relationship between child model and parent model.</param>
        /// <returns>Accessor of the child model.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not a valid getter.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relationship"/> is <see langword="null"/>.</exception>
        public static Accessor<TModel, TChildModel> RegisterChildModel<TModel, TModelKey, TChildModel>(Expression<Func<TModel, TChildModel>> getter,
            Func<TChildModel, TModelKey> relationship)
            where TModel : Model<TModelKey>
            where TModelKey : ModelKey
            where TChildModel : Model, new()
        {
            Utilities.Check.NotNull(getter, nameof(getter));
            Utilities.Check.NotNull(relationship, nameof(relationship));

            return s_childModelManager.Register(getter, a => CreateChildModel<TModel, TModelKey, TChildModel>(a, relationship), null);
        }

        private static TChildModel CreateChildModel<TModel, TModelKey, TChildModel>(Accessor<TModel, TChildModel> accessor, Func<TChildModel, TModelKey> relationship)
            where TModel : Model<TModelKey>
            where TModelKey : ModelKey
            where TChildModel : Model, new()
        {
            TChildModel result = new TChildModel();
            var parentModel = accessor.Parent;
            var parentModelColumnMappings = relationship(result).GetColumnMappings(parentModel.PrimaryKey);
            result.Construct(parentModel, accessor.OwnerType, accessor.Name, parentModelColumnMappings);
            return result;
        }

        private void Construct(Model parentModel, Type ownerType, string name, ReadOnlyCollection<ColumnMapping> parentModelColumnMappings)
        {
            this.ConstructModelMember(parentModel, ownerType, name);
            ParentModelColumnMappings = parentModelColumnMappings;
        }

        /// <summary>
        /// Gets the column mappings between its parent model and this model.
        /// </summary>
        internal ReadOnlyCollection<ColumnMapping> ParentModelColumnMappings { get; private set; }

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
        }

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

            Initialize(s_childModelManager);
            if (DataSource != null && DataSource.Kind == DataSourceKind.DataSet)
            {
                foreach (var model in ChildModels)
                {
                    var modelType = model.GetType();
                    var invoker = s_createDataSetInvokers.GetOrAdd(modelType, t => BuildCreateDataSetInvoker(t));
                    invoker(model);
                }
            }

            AreChildModelsInitialized = true;
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
            DataSet = dataSource as IDataSet;
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

        internal IDataSet DataSet { get; private set; }

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
        int IModelSet.Count
        {
            get { return 1; }
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

        internal static T Clone<T>(T sourceModel, bool setDataSource)
            where T : Model, new()
        {
            T result = new T();
            result.InitializeClone(sourceModel, setDataSource);
            return result;
        }

        internal Model Clone(bool setDataSource)
        {
            Model result = (Model)Activator.CreateInstance(this.GetType());
            result.InitializeClone(this, setDataSource);
            return result;
        }

        private void InitializeClone(Model sourceModel, bool setDataSource)
        {
            InitializeColumnLists(sourceModel);
            if (setDataSource && sourceModel.DataSource != null)
                SetDataSource(sourceModel.DataSource);
        }

        private void InitializeColumnLists(Model sourceModel)
        {
            var accessors = s_columnListManager.GetAll(this.GetType());
            foreach (var accessor in accessors)
            {
                var columnList = accessor.GetProperty(this);
                var sourceColumnList = accessor.GetProperty(sourceModel);
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
                    throw Error.Model_MultipleClusteredIndex(_clusteredIndex.SystemName);
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
                throw Error.Model_DuplicateConstraintName(constraint.SystemName);

            var index = constraint as IIndexConstraint;
            if (index != null && index.IsClustered)
                ClusteredIndex = index;

            this.AddOrUpdateInterceptor(constraint);
        }

        internal SequentialKeyModel CreateSequentialKey()
        {
            var result = new SequentialKeyModel(this);
            return result;
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

        internal bool ContainsMember(string memberName)
        {
            Utilities.Check.NotEmpty(memberName, nameof(memberName));
            return _members.ContainsKey(memberName);
        }

        internal ModelMember this[string memberName]
        {
            get
            {
                if (!ContainsMember(memberName))
                    throw new ArgumentOutOfRangeException(nameof(memberName));

                return _members[memberName];
            }
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
                AddDbTableConstraint(new PrimaryKeyConstraint(this, null, true, () => GetIdentityColumnSort(identity)), false);
            else
            {
                ChangeClusteredIndexAsNonClustered();
                AddDbTableConstraint(new UniqueConstraint(null, true, GetIdentityColumnSort(identity)), false);
            }
        }

        private static IList<ColumnSort> GetIdentityColumnSort(Identity identity)
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

        protected void Unique(string constraintName, bool isClustered, params ColumnSort[] columns)
        {
            Utilities.Check.NotNull(columns, nameof(columns));
            if (columns.Length == 0)
                throw Error.Argument(Strings.Model_EmptyColumns, nameof(columns));

            for (int i = 0; i < columns.Length; i++)
            {
                var column = columns[i].Column;
                if (column == null || column.ParentModel != this)
                    throw Error.Argument(Strings.Model_VerifyChildColumn, string.Format(CultureInfo.InvariantCulture, nameof(columns) + "[{0}]", i));
            }

            AddDbTableConstraint(new UniqueConstraint(constraintName, isClustered, columns), false);
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

        internal DbTable<SequentialKeyModel> SequentialKeyTempTable { get; set; }

        public override string ToString()
        {
            return this.GetType().Name + ": [" + string.Join(", ", Columns.Select(x => x.ColumnName)) + "]";
        }
    }
}
