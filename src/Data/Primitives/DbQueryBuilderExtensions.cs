
namespace DevZest.Data.Primitives
{
    public static class DbQueryBuilderExtensions
    {
        public static T SelectColumn<T>(this T queryBuilder, Column source, Column target)
            where T : DbQueryBuilder
        {
            queryBuilder.SelectCore(source, target);
            return queryBuilder;
        }
    }
}
