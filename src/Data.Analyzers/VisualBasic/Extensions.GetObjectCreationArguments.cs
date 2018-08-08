using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Collections.Immutable;

namespace DevZest.Data.CodeAnalysis.VisualBasic
{
    internal static partial class Extensions
    {
        public static ArgumentSyntax[] GetObjectCreationArguments(this MethodBlockSyntax methodBlock, ImmutableArray<IParameterSymbol> parameters)
        {
            var objectCreationExpression = GetObjectCreationExpression(methodBlock);
            return objectCreationExpression == null ? null : GetArguments(objectCreationExpression, parameters);
        }

        private static ObjectCreationExpressionSyntax GetObjectCreationExpression(MethodBlockSyntax methodBlock)
        {
            var statements = methodBlock.Statements;
            if (statements.Count != 1)
                return null;

            if (!(statements[0] is ReturnStatementSyntax returnStatement))
                return null;

            return returnStatement.Expression as ObjectCreationExpressionSyntax;
        }

        private static ArgumentSyntax[] GetArguments(ObjectCreationExpressionSyntax objectCreationExpression, ImmutableArray<IParameterSymbol> parameters)
        {
            var argumentList = objectCreationExpression.ArgumentList;
            if (argumentList == null)
                return null;
            var arguments = argumentList.Arguments;
            if (parameters.Length != arguments.Count)
                return null;

            var result = new ArgumentSyntax[arguments.Count];
            for (int i = 0; i < arguments.Count; i++)
            {
                var argument = arguments[i];
                if (!argument.IsNamed)
                {
                    result[i] = argument;
                    continue;
                }

                var name = ((SimpleArgumentSyntax)argument).NameColonEquals.Name.Identifier.Text;
                var index = parameters.IndexOf(name);
                if (index < 0 || result[index] != null)
                    return null;
                result[index] = argument;
            }
            return result;
        }
    }
}
