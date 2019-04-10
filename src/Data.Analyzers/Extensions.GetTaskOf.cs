using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    static partial class Extensions
    {
        public static INamedTypeSymbol GetTaskOf(this Compilation compilation, ITypeSymbol type)
        {
            var taskOf = compilation.GetKnownType(KnownTypes.TaskOf);
            return taskOf.Construct(type);
        }
    }
}
