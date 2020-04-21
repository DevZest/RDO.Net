using DevZest.Data.Primitives;
using System.Runtime.CompilerServices;

namespace DevZest.Data.MySql
{
    internal static class JsonTableManager
    {
        private static readonly ConditionalWeakTable<Model, _String> s_jsonRowSets = new ConditionalWeakTable<Model, _String>();

        public static _String GetSourceJsonParam(this Model model)
        {
            return s_jsonRowSets.TryGetValue(model, out var result) ? result : null;
        }

        public static DbSet<T> CreateJsonTable<T>(this MySqlSession mySqlSession, string json, string ordinalColumnName)
            where T : Model, new()
        {
            var _ = new T();
            if (!string.IsNullOrEmpty(ordinalColumnName))
            {
                var dataSetOrdinalColumn = new _Int32().AsJsonOrdinality();
                _.AddSystemColumn(dataSetOrdinalColumn, ordinalColumnName);
            }
            var sourceJsonParam = _String.Param(json).AsMySqlJson();
            s_jsonRowSets.Add(_, sourceJsonParam);
            return _.CreateDbTable(mySqlSession, ModelAliasManager.GetDbAlias(_));
        }
    }
}
