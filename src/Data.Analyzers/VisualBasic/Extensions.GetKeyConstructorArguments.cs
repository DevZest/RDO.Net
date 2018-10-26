using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Collections.Immutable;

namespace DevZest.Data.CodeAnalysis.VisualBasic
{
    internal static partial class Extensions
    {
        public static ArgumentSyntax[] GetKeyConstructorArguments(this MethodBlockSyntax methodBlock, ImmutableArray<IParameterSymbol> constructorParams)
        {
            var constructorExpression = GetConstructorExpression(methodBlock);
            return constructorExpression == null ? null : GetConstructorArguments(constructorExpression, constructorParams);
        }

        private static ObjectCreationExpressionSyntax GetConstructorExpression(MethodBlockSyntax methodBlock)
        {
            var statements = methodBlock.Statements;
            if (statements.Count != 1)
                return null;

            if (!(statements[0] is ReturnStatementSyntax returnStatement))
                return null;

            return returnStatement.Expression as ObjectCreationExpressionSyntax;
        }

        private static ArgumentSyntax[] GetConstructorArguments(ObjectCreationExpressionSyntax constructorExpression, ImmutableArray<IParameterSymbol> constructorParams)
        {
            var argumentList = constructorExpression.ArgumentList;
            if (argumentList == null)
                return null;
            var arguments = argumentList.Arguments;
            if (constructorParams.Length != arguments.Count)
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
                var index = constructorParams.IndexOf(name);
                if (index < 0 || result[index] != null)
                    return null;
                result[index] = argument;
            }
            return result;
        }
    }
}
