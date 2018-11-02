using Microsoft.CodeAnalysis;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static ITypeSymbol GetCrossReferenceAttributeType(this AttributeData attribute, Compilation compilation)
        {
            var attributeClass = attribute.AttributeClass;

            var crossReference = attributeClass.GetAttributes().Where(x => x.AttributeClass.EqualsTo(KnownTypes.CrossReferenceAttribute, compilation)).FirstOrDefault();
            return crossReference == null ? null : (ITypeSymbol)crossReference.ConstructorArguments[0].Value;
        }
    }
}
