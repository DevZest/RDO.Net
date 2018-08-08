using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace DevZest.Data.CodeAnalysis.CSharp
{
    internal static partial class Extensions
    {
        public static ArgumentSyntax[] GetConstructorArguments(this MethodDeclarationSyntax methodDeclaration, ImmutableArray<IParameterSymbol> constructorParams)
        {
            var constructorExpression = GetConstructorExpression(methodDeclaration);
            return constructorExpression == null ? null : GetConstructorArguments(constructorExpression, constructorParams);
        }

        private static ObjectCreationExpressionSyntax GetConstructorExpression(MethodDeclarationSyntax methodDeclaration)
        {
            var body = methodDeclaration.Body;
            if (body == null)
                return null;

            var statements = body.Statements;
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
                var nameColon = argument.NameColon;
                if (argument.NameColon == null)
                {
                    result[i] = argument;
                    continue;
                }

                var name = nameColon.Name.Identifier.Text;
                var index = constructorParams.IndexOf(name);
                if (index < 0 || result[index] != null)
                    return null;
                result[index] = argument;
            }
            return result;
        }
    }
}
