using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace DevZest.Data.AspNetCore
{
    internal static class Extensions
    {
        public static bool IsScalar(this ModelMetadata modelMetadata)
        {
            var validatorMetadata = modelMetadata.ValidatorMetadata;
            return validatorMetadata.OfType<ScalarAttribute>().Any();
        }
    }
}
