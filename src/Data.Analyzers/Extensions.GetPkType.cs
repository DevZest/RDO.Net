using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static INamedTypeSymbol GetPkType(this INamedTypeSymbol type, Compilation compilation)
        {
            var genericModelType = compilation.GetKnownType(KnownTypes.GenericModel);

            INamedTypeSymbol baseModelType = null;
            for (var currentType = type.BaseType; currentType != null; currentType = currentType.BaseType)
            {
                if (currentType.OriginalDefinition.Equals(genericModelType))
                {
                    baseModelType = currentType;
                    break;
                }
            }

            if (baseModelType == null)
                return null;
            return baseModelType.TypeArguments[0] as INamedTypeSymbol;
        }
    }
}
