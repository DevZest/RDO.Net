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
    public abstract partial class Model : ModelMember, IModels
    {
        #region RegisterColumn

        static MounterManager<Model, Column> s_columnManager = new MounterManager<Model, Column>();

        /// <summary>
        /// Registers a new column which has a default constructor.
        /// </summary>
        /// <typeparam name="TModel">The type of model which the column is registered on.</typeparam>
        /// <typeparam name="TColumn">The type of the column.</typeparam>
        /// <param name="getter">The lambda expression of the column getter.</param>
        /// <param name="initializer">The additional initializer.</param>
        /// <returns>Mounter of the column.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not a valid getter.</exception>
        public static Mounter<TColumn> RegisterColumn<TModel, TColumn>(Expression<Func<TModel, TColumn>> getter, Action<TColumn> initializer = null)
            where TModel : Model
            where TColumn : Column, new()
        {
            var columnAttributes = VerifyPropertyGetter(getter);

            return s_columnManager.Register(getter, a => CreateColumn(a, initializer, columnAttributes));
        }

        /// <summary>
        /// Registers a column from existing column property, without inheriting its <see cref="ColumnKey"/> value.
        /// </summary>
        /// <typeparam name="TModel">The type of model which the column is registered on.</typeparam>
        /// <typeparam name="TColumn">The type of the column.</typeparam>
        /// <param name="getter">The lambda expression of the column getter.</param>
        /// <param name="fromProperty">The existing column property.</param>
        /// <param name="initializer">The additional initializer.</param>
        /// <returns>Mounter of the column.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not an valid getter.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="fromProperty"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="initializer"/> is null.</exception>
        public static Mounter<TColumn> RegisterColumn<TModel, TColumn>(Expression<Func<TModel, TColumn>> getter,
            Mounter<TColumn> fromProperty,
            Action<TColumn> initializer = null)
            where TModel : Model
            where TColumn : Column, new()
        {
            var columnAttributes = VerifyPropertyGetter(getter);
            Utilities.Check.NotNull(fromProperty, nameof(fromProperty));

            var result = s_columnManager.Register(getter, a => CreateColumn(a, fromProperty, initializer, columnAttributes));
            result.OriginalOwnerType = fromProperty.OriginalOwnerType;
            result.OriginalName = fromProperty.OriginalName;
            return result;
        }

        private static IEnumerable<ColumnAttribute> VerifyPropertyGetter<TModel, TColumn>(Expression<Func<TModel, TColumn>> getter)
        {
            Utilities.Check.NotNull(getter, nameof(getter));
            var memberExpr = getter.Body as MemberExpression;
            if (memberExpr == null)
                throw new ArgumentException(Strings.Property_InvalidGetter, nameof(getter));

            var propertyInfo = typeof(TModel).GetProperty(memberExpr.Member.Name, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyInfo == null)
                throw new ArgumentException(Strings.Property_InvalidGetter, nameof(getter));

            return propertyInfo.GetCustomAttributes<ColumnAttribute>();
        }

        private static T CreateColumn<TModel, T>(Mounter<TModel, T> mounter, Action<T> initializer, IEnumerable<ColumnAttribute> columnAttributes)
            where TModel : Model
            where T : Column, new()
        {
            var result = Column.Create<T>(mounter.OwnerType, mounter.Name);
            result.Construct(mounter.Parent, mounter.OwnerType, mounter.Name, ColumnKind.User, null, GetColumnInitializer(initializer, columnAttributes));
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

        private static T CreateColumn<TModel, T>(Mounter<TModel, T> mounter, Mounter<T> fromMounter, Action<T> initializer, IEnumerable<ColumnAttribute> columnAttributes)
            where TModel : Model
            where T : Column, new()
        {
            var result = Column.Create<T>(fromMounter.OriginalOwnerType, fromMounter.OriginalName);
            result.Construct(mounter.Parent, mounter.OwnerType, mounter.Name, ColumnKind.User, fromMounter.Initializer, GetColumnInitializer(initializer, columnAttributes));
            return result;
        }

        #endregion

        #region RegisterColumnList

        static MounterManager<Model, ColumnList> s_columnListManager = new MounterManager<Model, ColumnList>();

        /// <summary>
        /// Registers a column list.
        /// </summary>
        /// <typeparam name="TModel">The type of model which the column is registered on.</typeparam>
        /// <typeparam name="T">The type of the column contained by the column list.</typeparam>
        /// <param name="getter">The lambda expression of the column list getter.</param>
        /// <returns>Mounter of the column list.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not a valid getter.</exception>
        public static Mounter<ColumnList<T>> RegisterColumnList<TModel, T>(Expression<Func<TModel, ColumnList<T>>> getter)
            where TModel : Model
            where T : Column
        {
            Utilities.Check.NotNull(getter, nameof(getter));

            return s_columnListManager.Register(getter, a => CreateColumnList(a), null);
        }

        private static ColumnList<T> CreateColumnList<TModel, T>(Mounter<TModel, ColumnList<T>> mounter)
            where TModel : Model
            where T : Column
        {
            var result = new ColumnList<T>();
            result.ConstructModelMember(mounter.Parent, mounter.OwnerType, mounter.Name);
            return result;
        }

        #endregion

        #region RegisterChildModel

        static MounterManager<Model, Model> s_childModelManager = new MounterManager<Model, Model>();

        public static Mounter<TChildModel> RegisterChildModel<TModel, TChildModel>(Expression<Func<TModel, TChildModel>> getter)
            where TModel : Model, new()
            where TChildModel : Model, new()
        {
            Utilities.Check.NotNull(getter, nameof(getter));
            return s_childModelManager.Register(getter, CreateChildModel, null);
        }

        private static TChildModel CreateChildModel<TModel, TChildModel>(Mounter<TModel, TChildModel> mounter)
            where TModel : Model, new()
            where TChildModel : Model, new()
        {
            TChildModel result = new TChildModel();
            var parentModel = mounter.Parent;
            result.Construct(parentModel, mounter.OwnerType, mounter.Name);
            return result;
        }

        internal void Construct(Model parentModel, Type ownerType, string name)
        {
            Construct(parentModel, ownerType, name, Array<ColumnMapping>.Empty, Array<ColumnMapping>.Empty);
        }

        /// <summary>
        /// Registers a child model.
        /// </summary>
        /// <typeparam name="TModel">The type of model which the child model is registered on.</typeparam>
        /// <typeparam name="TChildModel">The type of the child model.</typeparam>
        /// <param name="getter">The lambda expression of the child model getter.</param>
        /// <param name="relationshipGetter">Gets relationship between child model and parent model.</param>
        /// <returns>Mounter of the child model.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="getter"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="getter"/> expression is not a valid getter.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="relationshipGetter"/> is <see langword="null"/>.</exception>
        public static Mounter<TChildModel> RegisterChildModel<TModel, TModelKey, TChildModel>(Expression<Func<TModel, TChildModel>> getter,
            Func<TChildModel, TModelKey> relationshipGetter, Action<ColumnMappingsBuilder, TChildModel, TModel> childColumnsBuilder = null)
            where TModel : Model<TModelKey>
            where TModelKey : KeyBase
            where TChildModel : Model, new()
        {
            Utilities.Check.NotNull(getter, nameof(getter));
            Utilities.Check.NotNull(relationshipGetter, nameof(relationshipGetter));

            return s_childModelManager.Register(getter, a => CreateChildModel<TModel, TModelKey, TChildModel>(a, relationshipGetter, childColumnsBuilder), null);
        }

        private static TChildModel CreateChildModel<TModel, TModelKey, TChildModel>(Mounter<TModel, TChildModel> mounter,
            Func<TChildModel, TModelKey> relationshipGetter, Action<ColumnMappingsBuilder, TChildModel, TModel> parentMappingsBuilder)
            where TModel : Model<TModelKey>
            where TModelKey : KeyBase
            where TChildModel : Model, new()
        {
            TChildModel result = new TChildModel();
            var parentModel = mounter.Parent;
            var parentRelationship = new Relationship(relationshipGetter(result), parentModel.PrimaryKey).Mappings;
            var parentMappings = AppendColumnMappings(parentRelationship, parentMappingsBuilder, result, parentModel);
            result.Construct(parentModel, mounter.OwnerType, mounter.Name, parentRelationship, parentMappings);
            return result;
        }

        private static IReadOnlyList<ColumnMapping> AppendColumnMappings<TChildModel, TParentModel>(IReadOnlyList<ColumnMapping> parentRelationship,
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
            return result;
        }

        private void Construct(Model parentModel, Type ownerType, string name, IReadOnlyList<ColumnMapping> parentRelationship, IReadOnlyList<ColumnMapping> parentMappings)
        {
            this.ConstructModelMember(parentModel, ownerType, name);

            Debug.Assert(parentMappings != null);
            Debug.Assert(parentRelationship != null);
            ParentRelationship = parentRelationship;
            ParentMappings = parentMappings;
            InitChildColumns();
        }

        private void InitChildColumns()
        {
            var parentMappings = ParentMappings;
            for (int i = 0; i < parentMappings.Count; i++)
            {
                var mapping = parentMappings[i];
                mapping.Source.InitAsChild(mapping.Target);
            }
        }

        /// <summary>
        /// Gets the column mappings between its parent model and this model.
        /// </summary>
        internal IReadOnlyList<ColumnMapping> ParentRelationship { get; private set; }

        internal IReadOnlyList<ColumnMapping> ParentMappings { get; private set; }

        #endregion

        protected Model()
        {
            Columns = new ColumnCollection(this);
            ChildModels = new ModelCollection(this);

            Mount(s_columnManager);
            Mount(s_columnListManager);
        }

        internal override void ConstructModelMember(Model parentModel, Type ownerType, string name)
        {
            base.ConstructModelMember(parentModel, ownerType, name);
            ParentModel.ChildModels.Add(this);
            Depth = ParentModel.Depth + 1;
            _rootModel = ParentModel.RootModel;
        }

        internal int Depth { get; private set; }

        private Model _rootModel;
        internal Model RootModel
        {
            get { return _rootModel ?? this; }
        }

        /// <summary>
        /// Gets a value indicates whether child models are initialized.
        /// </summary>
        /// <remarks>Unlike <see cref="Column"/> and <see cref="ColumnList"/>,
        /// child models are not initialized by default. This design decision is to deal with the situation when recursive child models registered.
        /// <see cref="EnsureInitialized"/> will be called automatically when creating the first <see cref="DataRow"/> the query builder.
        /// </remarks>
        internal bool IsInitialized { get; private set; }

        /// <summary>
        /// Ensures child models are initialized.
        /// </summary>
        /// <remarks>Unlike <see cref="Column"/> and <see cref="ColumnList"/>,
        /// child models are not initialized by default. This design decision is to deal with the situation when recursive child models registered.
        /// <see cref="EnsureInitialized"/> will be called automatically when creating the first <see cref="DataRow"/>.
        /// </remarks>
        public void EnsureInitialized()
        {
            if (IsInitialized)
                return;

            Mount(s_childModelManager);
            if (DataSource != null && DataSource.Kind == DataSourceKind.DataSet)
            {
                foreach (var model in ChildModels)
                {
                    var modelType = model.GetType();
                    var invoker = s_createDataSetInvokers.GetOrAdd(modelType, t => BuildCreateDataSetInvoker(t));
                    invoker(model);
                }
                OnInitializing();
                DataSetContainer.MergeComputations(this);
                foreach (var column in Columns)
                    column.InitValueManager();
            }
            IsInitialized = true;

            OnInitialized();
        }

        protected virtual void OnInitializing()
        {
            Initializing(this, EventArgs.Empty);
        }

        protected virtual void OnInitialized()
        {
            Initialized(this, EventArgs.Empty);
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

        private void Mount<T>(MounterManager<Model, T> mounterManager)
            where T : class
        {
            var mounters = mounterManager.GetAll(this.GetType());
            foreach (var mounter in mounters)
                mounter.Mount(this);
        }

        protected internal ColumnCollection Columns { get; private set; }

        protected internal IReadOnlyList<Column> LocalColumns
        {
            get
            {
                if (LocalColumnList == null)
                    return Array<Column>.Empty;
                else
                    return LocalColumnList;
            }
        }

        internal List<Column> LocalColumnList { get; set; }

        protected internal ModelCollection ChildModels { get; private set; }

        private List<IValidator> _validators = new List<IValidator>();
        protected internal IList<IValidator> Validators
        {
            get { return _validators; }
        }

        protected internal virtual IColumnValidationMessages Validate(DataRow dataRow, ValidationSeverity? severity)
        {
            var result = ColumnValidationMessages.Empty;
            foreach (var validator in Validators)
            {
                if (severity.HasValue && validator.Severity != severity.GetValueOrDefault())
                    continue;
                var validationMessage = Validate(validator, dataRow);
                if (validationMessage == null)
                    continue;

                result = result.Add(validationMessage);
            }

            return result;
        }

        private static ColumnValidationMessage Validate(IValidator validator, DataRow dataRow)
        {
            if (validator.ValidCondition[dataRow] == true)
                return null;
            else
                return new ColumnValidationMessage(validator.MessageId, validator.Severity, validator.Message[dataRow], validator.Columns);
        }

        public KeyBase PrimaryKey
        {
            get { return GetPrimaryKeyCore(); }
        }

        internal virtual KeyBase GetPrimaryKeyCore()
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
            Columns.InitDbColumnNames();
            if (DataSet != null && RootModel == this)
                _dataSetContainer = new DataSetContainer();
        }

        protected internal sealed override bool DesignMode
        {
            get
            {
                if (DataSource == null)
                    return true;
                if (DataSource.Kind != DataSourceKind.DataSet)
                    return false;
                return !IsInitialized;
            }
        }

        public DataSet DataSet { get; private set; }

        private int _ordinal = -1;
        internal int Ordinal
        {
            get { return _ordinal; }
            set { _ordinal = value; }
        }

        #region IModelSet
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        bool IModels.Contains(Model model)
        {
            return model == this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<Model>.Count
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

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        bool IModels.IsSealed
        {
            get { return true; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IModels IModels.Seal()
        {
            return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IModels IModels.Add(Model value)
        {
            Utilities.Check.NotNull(value, nameof(value));
            if (value == this)
                return this;
            return Models.New(this, value);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IModels IModels.Remove(Model value)
        {
            Utilities.Check.NotNull(value, nameof(value));
            if (value == this)
                return Models.Empty;
            else
                return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IModels IModels.Clear()
        {
            return Models.Empty;
        }
        #endregion

        internal static T Clone<T>(T prototype, bool setDataSource)
            where T : Model, new()
        {
            T result = new T();
            result.InitializeClone(prototype, setDataSource);
            return result;
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
            InitializeColumnLists(prototype);
            if (setDataSource && prototype.DataSource != null)
                SetDataSource(prototype.DataSource);
        }

        private void InitializeColumnLists(Model prototype)
        {
            var properties = s_columnListManager.GetAll(this.GetType());
            foreach (var property in properties)
            {
                var columnList = property.GetInstance(this);
                var sourceColumnList = property.GetInstance(prototype);
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

        private int _suspendIdentityCount;
        internal bool IsIdentitySuspended
        {
            get { return _suspendIdentityCount > 0; }
        }

        internal void SuspendIdentity()
        {
            _suspendIdentityCount++;
        }

        internal void ResumeIdentity()
        {
            _suspendIdentityCount--;
        }

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

        internal IReadOnlyList<ColumnMapping> GetParentRelationship(IReadOnlyList<ColumnMapping> columnMappings)
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

        private static DbExpression GetSource(DbExpression target, IReadOnlyList<ColumnMapping> relationship)
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

        internal DataRow EditingRow { get; private set; }

        internal void BeginEdit(DataRow dataRow)
        {
            Debug.Assert(EditingRow == null && dataRow != null);
            EnsureInitialized();
            EditingRow = dataRow;
            for (int i = 0; i < Columns.Count; i++)
                Columns[i].BeginEdit(dataRow);
            for (int i = 0; i < LocalColumns.Count; i++)
                LocalColumns[i].BeginEdit(dataRow);
        }

        internal void EndEdit(DataRow dataRow)
        {
            Debug.Assert(EditingRow != null);
            Debug.Assert(EditingRow == DataRow.Placeholder || EditingRow == dataRow);
            Debug.Assert(dataRow.ValueChangedSuspended);

            for (int i = 0; i < Columns.Count; i++)
                Columns[i].EndEdit(dataRow);
            for (int i = 0; i < LocalColumns.Count; i++)
                LocalColumns[i].EndEdit(dataRow);
            EditingRow = null;
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

        internal Column DeserializeColumn(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                return null;

            var result = Columns[columnName];
            if (result == null)
                throw new FormatException(Strings.Model_InvalidColumnName(columnName));
            return result;
        }

        public event EventHandler<EventArgs> Initializing = delegate { };
        public event EventHandler<EventArgs> Initialized = delegate { };
        public event EventHandler<DataRowEventArgs> DataRowInserting = delegate { };
        public event EventHandler<DataRowEventArgs> BeforeDataRowInserted = delegate { };
        public event EventHandler<DataRowEventArgs> AfterDataRowInserted = delegate { };
        public event EventHandler<DataRowEventArgs> DataRowRemoving = delegate { };
        public event EventHandler<DataRowRemovedEventArgs> DataRowRemoved = delegate { };
        public event EventHandler<ValueChangedEventArgs> ValueChanged = delegate { };

        internal void HandlesDataRowInserted(DataRow dataRow, Action<DataRow> updateAction)
        {
            dataRow.ValueChangedSuspended = true;
            var e = new DataRowEventArgs(dataRow);
            OnDataRowInserting(e);
            DataSetContainer.OnDataRowInserting(e);
            if (updateAction != null)
                updateAction(dataRow);
            OnBeforeDataRowInserted(e);
            DataSetContainer.OnBeforeDataRowInserted(e);
            dataRow.ValueChangedSuspended = false;
            DataSetContainer.SuspendComputation();
            OnDataRowInserted(e);
            DataSetContainer.OnAfterDataRowInserted(e);
            DataSetContainer.ResumeComputation();
        }

        protected virtual void OnDataRowInserting(DataRowEventArgs e)
        {
            DataRowInserting(this, e);
        }

        protected virtual void OnBeforeDataRowInserted(DataRowEventArgs e)
        {
            BeforeDataRowInserted(this, e);
        }

        protected virtual void OnDataRowInserted(DataRowEventArgs e)
        {
            AfterDataRowInserted(this, e);
        }

        internal void HandlesDataRowRemoving(DataRow dataRow)
        {
            dataRow.ValueChangedSuspended = true;
            var e = new DataRowEventArgs(dataRow);
            OnDataRowRemoving(e);
            DataSetContainer.OnDataRowRemoving(e);
        }

        protected virtual void OnDataRowRemoving(DataRowEventArgs e)
        {
            DataRowRemoving(this, e);
        }

        internal void HandlesDataRowRemoved(DataRow dataRow, DataSet baseDataSet, int ordinal, DataSet dataSet, int index)
        {
            if (EditingRow == dataRow)
                CancelEdit();

            dataRow.ValueChangedSuspended = false;
            DataSetContainer.SuspendComputation();
            var e = new DataRowRemovedEventArgs(dataRow, baseDataSet, ordinal, dataSet, index);
            OnDataRowRemoved(e);
            DataSetContainer.OnDataRowRemoved(e);
            DataSetContainer.ResumeComputation();
        }

        protected virtual void OnDataRowRemoved(DataRowRemovedEventArgs e)
        {
            DataRowRemoved(this, e);
        }

        internal void HandlesValueChanged(DataRow dataRow, Column column)
        {
            DataSetContainer.SuspendComputation();
            var e = new ValueChangedEventArgs(dataRow, column);
            OnValueChanged(e);
            DataSetContainer.OnValueChanged(e);
            DataSetContainer.ResumeComputation();
        }

        protected virtual void OnValueChanged(ValueChangedEventArgs e)
        {
            ValueChanged(this, e);
        }

        private DataSetContainer _dataSetContainer;
        public DataSetContainer DataSetContainer
        {
            get { return RootModel._dataSetContainer; }
        }

        private void VerifyCreateLocalColumn()
        {
            if (DataSetContainer == null)
                throw new InvalidOperationException(Strings.Model_CreateLocalColumnNullDataSetContainer);
        }

        protected Column<T> CreateLocalColumn<T>(Action<LocalColumnBuilder<T>> builder = null)
        {
            VerifyCreateLocalColumn();
            return DataSetContainer.CreateLocalColumn(this, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T>(T1 column, Func<DataRow, T1, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
        {
            VerifyCreateLocalColumn();
            return DataSetContainer.CreateLocalColumn(this, column, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T>(T1 column1, T2 column2, Func<DataRow, T1, T2, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
        {
            VerifyCreateLocalColumn();
            return DataSetContainer.CreateLocalColumn(this, column1, column2, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T>(T1 column1, T2 column2, T3 column3, Func<DataRow, T1, T2, T3, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
        {
            VerifyCreateLocalColumn();
            return DataSetContainer.CreateLocalColumn(this, column1, column2, column3, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T>(T1 column1, T2 column2, T3 column3, T4 column4,
            Func<DataRow, T1, T2, T3, T4, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
        {
            VerifyCreateLocalColumn();
            return DataSetContainer.CreateLocalColumn(this, column1, column2, column3, column4, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T>(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5,
            Func<DataRow, T1, T2, T3, T4, T5, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
        {
            VerifyCreateLocalColumn();
            return DataSetContainer.CreateLocalColumn(this, column1, column2, column3, column4, column5, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T>(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            Func<DataRow, T1, T2, T3, T4, T5, T6, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
        {
            VerifyCreateLocalColumn();
            return DataSetContainer.CreateLocalColumn(this, column1, column2, column3, column4, column5, column6, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T>(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6, T7 column7,
            Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
        {
            VerifyCreateLocalColumn();
            return DataSetContainer.CreateLocalColumn(this, column1, column2, column3, column4, column5, column6, column7, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T8, T>(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6, T7 column7, T8 column8,
            Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
        {
            VerifyCreateLocalColumn();
            return DataSetContainer.CreateLocalColumn(this, column1, column2, column3, column4, column5, column6, column7, column8, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T>(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6, T7 column7, T8 column8,
            T9 column9, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
            where T9 : Column
        {
            VerifyCreateLocalColumn();
            return DataSetContainer.CreateLocalColumn(this, column1, column2, column3, column4, column5, column6, column7, column8, column9, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T>(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6, T7 column7,
            T8 column8, T9 column9, T10 column10, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
            where T9 : Column
            where T10 : Column
        {
            VerifyCreateLocalColumn();
            return DataSetContainer.CreateLocalColumn(this, column1, column2, column3, column4, column5, column6, column7, column8, column9, column10, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T>(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6, T7 column7,
            T8 column8, T9 column9, T10 column10, T11 column11, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
            where T9 : Column
            where T10 : Column
            where T11 : Column
        {
            VerifyCreateLocalColumn();
            return DataSetContainer.CreateLocalColumn(this, column1, column2, column3, column4, column5, column6, column7, column8, column9, column10,
                column11, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T>(T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            T7 column7, T8 column8, T9 column9, T10 column10, T11 column11, T12 column12,
            Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
            where T9 : Column
            where T10 : Column
            where T11 : Column
            where T12 : Column
        {
            VerifyCreateLocalColumn();
            return DataSetContainer.CreateLocalColumn(this, column1, column2, column3, column4, column5, column6, column7, column8, column9, column10,
                column11, column12, expression, builder);
        }

        private void VerifyCreateLocalColumn(Model childModel, string paramName)
        {
            Utilities.Check.NotNull(childModel, paramName);
            if (childModel.ParentModel != this)
                throw new ArgumentException(Strings.Model_InvalidChildModel, paramName);
            VerifyCreateLocalColumn();
        }

        protected Column<T> CreateLocalColumn<T>(Model childModel, Action<LocalColumnBuilder<T>> builder = null)
        {
            VerifyCreateLocalColumn(childModel, nameof(childModel));
            return DataSetContainer.CreateLocalColumn(childModel, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T>(Model childModel, T1 column, Func<DataRow, T1, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
        {
            VerifyCreateLocalColumn(childModel, nameof(childModel));
            return DataSetContainer.CreateLocalColumn(childModel, column, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T>(Model childModel, T1 column1, T2 column2, Func<DataRow, T1, T2, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
        {
            VerifyCreateLocalColumn(childModel, nameof(childModel));
            return DataSetContainer.CreateLocalColumn(childModel, column1, column2, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T>(Model childModel, T1 column1, T2 column2, T3 column3,
            Func<DataRow, T1, T2, T3, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
        {
            VerifyCreateLocalColumn(childModel, nameof(childModel));
            return DataSetContainer.CreateLocalColumn(childModel, column1, column2, column3, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T>(Model childModel, T1 column1, T2 column2, T3 column3, T4 column4,
            Func<DataRow, T1, T2, T3, T4, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
        {
            VerifyCreateLocalColumn(childModel, nameof(childModel));
            return DataSetContainer.CreateLocalColumn(childModel, column1, column2, column3, column4, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T>(Model childModel, T1 column1, T2 column2, T3 column3, T4 column4, T5 column5,
            Func<DataRow, T1, T2, T3, T4, T5, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
        {
            VerifyCreateLocalColumn(childModel, nameof(childModel));
            return DataSetContainer.CreateLocalColumn(childModel, column1, column2, column3, column4, column5, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T>(Model childModel, T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            Func<DataRow, T1, T2, T3, T4, T5, T6, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
        {
            VerifyCreateLocalColumn(childModel, nameof(childModel));
            return DataSetContainer.CreateLocalColumn(childModel, column1, column2, column3, column4, column5, column6, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T>(Model childModel, T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6, T7 column7,
            Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
        {
            VerifyCreateLocalColumn(childModel, nameof(childModel));
            return DataSetContainer.CreateLocalColumn(childModel, column1, column2, column3, column4, column5, column6, column7, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T8, T>(Model childModel, T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6, T7 column7,
            T8 column8, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
        {
            VerifyCreateLocalColumn(childModel, nameof(childModel));
            return DataSetContainer.CreateLocalColumn(childModel, column1, column2, column3, column4, column5, column6, column7, column8, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T>(Model childModel, T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6, T7 column7,
            T8 column8, T9 column9, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
            where T9 : Column
        {
            VerifyCreateLocalColumn(childModel, nameof(childModel));
            return DataSetContainer.CreateLocalColumn(childModel, column1, column2, column3, column4, column5, column6, column7, column8, column9, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T>(Model childModel, T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            T7 column7, T8 column8, T9 column9, T10 column10, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
            where T9 : Column
            where T10 : Column
        {
            VerifyCreateLocalColumn(childModel, nameof(childModel));
            return DataSetContainer.CreateLocalColumn(childModel, column1, column2, column3, column4, column5, column6, column7, column8, column9, column10, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T>(Model childModel, T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            T7 column7, T8 column8, T9 column9, T10 column10, T11 column11, Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T> expression,
            Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
            where T9 : Column
            where T10 : Column
            where T11 : Column
        {
            VerifyCreateLocalColumn(childModel, nameof(childModel));
            return DataSetContainer.CreateLocalColumn(childModel, column1, column2, column3, column4, column5, column6, column7, column8, column9, column10,
                column11, expression, builder);
        }

        protected Column<T> CreateLocalColumn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T>(Model childModel, T1 column1, T2 column2, T3 column3, T4 column4, T5 column5, T6 column6,
            T7 column7, T8 column8, T9 column9, T10 column10, T11 column11, T12 column12,
            Func<DataRow, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T> expression, Action<LocalColumnBuilder<T>> builder = null)
            where T1 : Column
            where T2 : Column
            where T3 : Column
            where T4 : Column
            where T5 : Column
            where T6 : Column
            where T7 : Column
            where T8 : Column
            where T9 : Column
            where T10 : Column
            where T11 : Column
            where T12 : Column
        {
            VerifyCreateLocalColumn(childModel, nameof(childModel));
            return DataSetContainer.CreateLocalColumn(childModel, column1, column2, column3, column4, column5, column6, column7, column8, column9, column10,
                column11, column12, expression, builder);
        }
    }
}
