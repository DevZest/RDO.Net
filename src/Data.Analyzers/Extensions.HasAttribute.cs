using Microsoft.CodeAnalysis;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static bool HasAttribute(this ISymbol symbol, ITypeSymbol attributeClass)
        {
            return symbol.GetAttributes().Any(x => x.AttributeClass.Equals(attributeClass));
        }
    }
}
