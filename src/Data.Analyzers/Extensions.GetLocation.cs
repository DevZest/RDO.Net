using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static Location GetLocation(this AttributeData attribute)
        {
            var syntaxReference = attribute.ApplicationSyntaxReference;
            return Location.Create(syntaxReference.SyntaxTree, syntaxReference.Span);
        }
    }
}
