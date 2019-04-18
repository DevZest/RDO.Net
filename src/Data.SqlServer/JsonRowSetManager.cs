using DevZest.Data.Primitives;
using System.Runtime.CompilerServices;

namespace DevZest.Data.SqlServer
{
    internal static class JsonRowSetManager
    {
        private static readonly ConditionalWeakTable<Model, _String> s_jsonRowSets = new ConditionalWeakTable<Model, _String>();

        public static _String GetSourceJsonParam(this Model model)
        {
            return s_jsonRowSets.TryGetValue(model, out var result) ? result : null;
        }

        public static DbSet<T> CreateJsonRowSet<T>(this SqlSession sqlSession, string json, string ordinalColumnName)
            where T : class, IEntity, new()
        {
            var _ = new T();
            var model = _.Model;
            if (!string.IsNullOrEmpty(ordinalColumnName))
            {
                var dataSetOrdinalColumn = new _Int32();
                model.AddSystemColumn(dataSetOrdinalColumn, ordinalColumnName);
            }
            var sourceJsonParam = _String.Param(json).AsSqlNVarCharMax();
            s_jsonRowSets.Add(model, sourceJsonParam);
            return _.CreateDbTable(sqlSession, ModelAliasManager.GetDbAlias(model));
        }
    }
}
