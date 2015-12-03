using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    public static class ScalarDataExtensions
    {
        public static ColumnCollection GetAccessors(this ScalarData scalarData)
        {
            Check.NotNull(scalarData, nameof(scalarData));
            return scalarData.Accessors;
        }
    }
}
