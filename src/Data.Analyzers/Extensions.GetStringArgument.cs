using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static string GetStringArgument(this AttributeData attribute, int index = 0)
        {
            return (string)attribute.ConstructorArguments[index].Value;
        }
    }
}
