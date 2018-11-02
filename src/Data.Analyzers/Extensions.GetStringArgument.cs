using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static string GetStringArgument(this AttributeData attribute, int index = 0)
        {
            var constructorArguments = attribute.ConstructorArguments;
            return index >= 0 && index < constructorArguments.Length ? attribute.ConstructorArguments[index].Value as string : null;
        }
    }
}
