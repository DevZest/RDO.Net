using Microsoft.CodeAnalysis;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
        public static bool IsMounterRegistration(this IMethodSymbol symbol, Compilation compilation)
        {
            var attributes = symbol.GetAttributes();
            if (attributes == null)
                return false;
            return attributes.Any(x => x.AttributeClass.EqualsTo(KnownTypes.MounterRegistrationAttribute, compilation));
        }
    }
}
