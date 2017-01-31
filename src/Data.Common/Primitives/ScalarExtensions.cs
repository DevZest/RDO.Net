using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    public static class ScalarExtensions
    {
        public static Model GetModel(this Scalar scalar)
        {
            Check.NotNull(scalar, nameof(scalar));
            return scalar.Model;
        }
    }
}
