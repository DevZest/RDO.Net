using Microsoft.CodeAnalysis;

namespace DevZest.Data.CodeAnalysis
{
    internal static partial class Extensions
    {
        public static T SelfOrFirstAncestor<T>(this SyntaxNode node)
            where T : SyntaxNode
        {
            return (node is T result) ? result : node.FirstAncestor<T>();
        }

        public static T FirstAncestor<T>(this SyntaxNode node)
            where T : SyntaxNode
        {
            for (var current = node.Parent; current != null; current = current.Parent)
            {
                if (current is T result)
                    return result;
            }

            return null;
        }
    }
}
