using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DevZest.Data.Primitives;

namespace DevZest.Data.CodeAnalysis
{
    public static class DataSetExchange
    {
        public sealed class ColumnInfo : Model
        {
            static ColumnInfo()
            {
                RegisterColumn((ColumnInfo _) => _.Type);
                RegisterColumn((ColumnInfo _) => _.Name);
            }

            public _String Type { get; private set; }

            public _String Name { get; private set; }
        }

        private static IReadOnlyList<Column> GetSerializableColumns(DataSet dataSet)
        {
            return dataSet.Model.GetColumns().Where(x => x.IsSerializable).ToArray();
        }

        public static DataSet<ColumnInfo> GetColumns(DataSet dataSet)
        {
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            var columns = GetSerializableColumns(dataSet);
            var result = DataSet<ColumnInfo>.Create();
            var _ = result._;
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var dataRow = result.AddRow();
                _.Type[dataRow] = column.GetType().FullName;
                _.Name[dataRow] = column.Name;
            }

            return result;
        }

        public static DataSet<ColumnInfo> GetColumns(string json)
        {
            return DataSet<ColumnInfo>.ParseJson(json);
        }

        public sealed class DataValue : Model
        {
            static DataValue()
            {
                RegisterColumnList((DataValue _) => _.ColumnList);
            }

            internal ColumnList<Column> ColumnList { get; private set; }
        }

        public static DataSet<DataValue> GetDataValues(DataSet dataSet)
        {
            if (dataSet == null)
                throw new ArgumentNullException(nameof(dataSet));

            var columns = GetSerializableColumns(dataSet);
            var result = DataSet<DataValue>.Create(x => Initialize(x, columns));
            var columnList = result._.ColumnList;
            for (int i = 0; i < dataSet.Count; i++)
            {
                var dataRow = result.AddRow();
                for (int j = 0; j < columns.Count; j++)
                {
                    var column = columnList[j];
                    if (column.IsSupported())
                        column.SetValue(dataRow, columns[j].GetValue(dataSet[i]));
                }
            }

            return result;
        }

        public static DataSet<DataValue> GetDataValues(string json, DataSet<ColumnInfo> columns)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException(nameof(json));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return DataSet<DataValue>.ParseJson(x => Initialize(x, columns), json);
        }

        private static void Initialize(DataValue _, IReadOnlyList<Column> columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                _.ColumnList.Add(() => (Column)Activator.CreateInstance(column.GetType()));
            }
        }

        private static void Initialize(DataValue _, DataSet<ColumnInfo> columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                var typeName = columns._.Type[i];
                _.ColumnList.Add(() => CreateColumn(typeName));
            }
        }

        private static Column CreateColumn(Type type)
        {
            Debug.Assert(type != null);
            var result = Activator.CreateInstance(type, true) as Column;
            return result ?? DummyColumn.Create(type.FullName);
        }

        private static Column CreateColumn(string typeFullName)
        {
            var type = GetType(typeFullName);
            return type == null ? DummyColumn.Create(typeFullName) : CreateColumn(type);
        }

        private static Type GetType(string typeFullName)
        {
            Debug.Assert(typeFullName != null);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var result = assembly.GetType(typeFullName, false);
                if (result != null)
                    return result;
            }

            return null;
        }

        private static string FormatFailedToCreateType(string typeFullName)
        {
            return string.Format("Failed to create type '{0}'", typeFullName);
        }

        private sealed class DummyColumn : Column<object>
        {
            public static DummyColumn Create(string unsupportedTypeName)
            {
                var result = new DummyColumn();
                result.SetUnsupportedMessage(FormatFailedToCreateType(unsupportedTypeName));
                return result;
            }

            private DummyColumn()
            {
            }

            public override _String CastToString()
            {
                throw new NotSupportedException();
            }

            protected override Column<object> CreateConst(object value)
            {
                throw new NotSupportedException();
            }

            protected override Column<object> CreateParam(object value)
            {
                throw new NotSupportedException();
            }

            protected override object DeserializeValue(JsonValue value)
            {
                throw new NotSupportedException();
            }

            protected override JsonValue SerializeValue(object value)
            {
                throw new NotSupportedException();
            }
        }

        private static readonly ConditionalWeakTable<Column, string> s_unsupportedMessages = new ConditionalWeakTable<Column, string>();

        internal static string GetUnsupportedMessage(this Column column)
        {
            return s_unsupportedMessages.TryGetValue(column, out var result) ? result : null;
        }

        internal static bool IsSupported(this Column column)
        {
            return !s_unsupportedMessages.TryGetValue(column, out _);
        }

        internal static void SetUnsupportedMessage(this Column column, string message)
        {
            Debug.Assert(!string.IsNullOrEmpty(message));
            Debug.Assert(column.IsSupported());
            s_unsupportedMessages.Add(column, message);
        }

        public sealed class ColumnFlag : Model
        {
            static ColumnFlag()
            {
                RegisterColumn((ColumnFlag _) => _.IsVisible);
            }

            public _Boolean IsVisible { get; private set; }
        }

        public static DataSet<ColumnFlag> GetColumnFlags(this DataSet<DataValue> dataValues)
        {
            var result = DataSet<ColumnFlag>.Create();
            var columnList = dataValues._.ColumnList;
            for (int i = 0; i < columnList.Count; i++)
            {
                var dataRow = result.AddRow();
                var isSupported = columnList[i].IsSupported();
                result._.IsVisible[dataRow] = isSupported;
            }

            return result;
        }

        private sealed class SupportedColumnsJsonFilter : JsonFilter
        {
            public static readonly SupportedColumnsJsonFilter Singleton = new SupportedColumnsJsonFilter();

            private SupportedColumnsJsonFilter()
            {
            }

            protected override bool ShouldSerialize(ModelMember member)
            {
                if (member is Column column)
                    return column.IsSupported();
                return true;
            }
        }

        public static string GetJsonString(this DataSet<DataValue> dataValues)
        {
            return dataValues.Filter(SupportedColumnsJsonFilter.Singleton).ToJsonString(true);
        }

        private sealed class PreviewJsonFilter : JsonFilter
        {
            public PreviewJsonFilter(IReadOnlyList<Column> columns)
            {
                _columns = new HashSet<Column>(columns);
            }

            private readonly HashSet<Column> _columns;

            protected override bool ShouldSerialize(ModelMember member)
            {
                if (member is Column column)
                    return _columns.Contains(column);
                return member is ColumnList || member is Projection;
            }
        }

        public static string PreviewJson(string modelTypeName, string columnFlagsJson, string dataValuesJson)
        {
            var type = GetType(modelTypeName);
            if (type == null)
                throw new InvalidOperationException(FormatFailedToCreateType(modelTypeName));

            var methodInfo = typeof(DataSetExchange).GetMethod(nameof(_PreviewJson), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(type);
            return (string)methodInfo.Invoke(null, new object[] { columnFlagsJson, dataValuesJson });
        }

        internal static string _PreviewJson<T>(string columnFlagsJson, string dataValuesJson)
            where T : class, IEntity, new()
        {
            var result = DataSet<T>.Create().EnsureInitialized();
            var columns = GetSerializableColumns(result);
            var columnFlags = DataSet<ColumnFlag>.ParseJson(columnFlagsJson);
            columns = Filter(columns, columnFlags);

            var dataValues = DataSet<DataValue>.ParseJson(x => Initialize(x, columns), dataValuesJson);
            Debug.Assert(dataValues._.ColumnList.Count == columns.Count);
            for (int i = 0; i < dataValues.Count; i++)
            {
                var dataRow = result.AddRow();
                var dataValueRow = dataValues[i];
                for (int j = 0; j < columns.Count; j++)
                    columns[j].SetValue(dataRow, dataValues._.ColumnList[j].GetValue(dataValueRow));
            }

            return result.Filter(new PreviewJsonFilter(columns)).ToJsonString(true);
        }

        private static IReadOnlyList<Column> Filter(IReadOnlyList<Column> columns, DataSet<ColumnFlag> columnFlags)
        {
            Debug.Assert(columnFlags.Count == columns.Count);
            var result = new List<Column>();
            var isVisible = columnFlags._.IsVisible;
            for (int i = 0; i < columnFlags.Count; i++)
            {
                if (isVisible[i] == true)
                    result.Add(columns[i]);
            }

            return result;
        }
    }
}
