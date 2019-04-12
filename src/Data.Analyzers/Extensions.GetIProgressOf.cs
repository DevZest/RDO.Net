using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static INamedTypeSymbol GetIProgressOf(this Compilation compilation, ITypeSymbol type)
        {
            var iprogressType = compilation.GetKnownType(KnownTypes.IProgressOf);
            return iprogressType.Construct(type);
        }
    }
}
