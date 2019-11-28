using Microsoft.CodeAnalysis;
using System.Text;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static string GetFullyQualifiedMetadataName(this ISymbol s)
        {
            if (s == null || IsRootNamespace(s))
                return string.Empty;

            var sb = new StringBuilder(s.MetadataName);
            var last = s;

            s = s.ContainingSymbol;

            while (!IsRootNamespace(s))
            {
                if (s is ITypeSymbol && last is ITypeSymbol)
                    sb.Insert(0, '+');
                else
                    sb.Insert(0, '.');

                sb.Insert(0, s.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                //sb.Insert(0, s.MetadataName);
                s = s.ContainingSymbol;
            }

            return sb.ToString();
        }

        private static bool IsRootNamespace(ISymbol symbol)
        {
            return (symbol is INamespaceSymbol s) && s.IsGlobalNamespace;
        }
    }
}
