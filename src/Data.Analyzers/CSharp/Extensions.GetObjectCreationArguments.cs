using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace DevZest.Data.CodeAnalysis.CSharp
{
    internal static partial class Extensions
    {
        public static ArgumentSyntax[] GetObjectCreationArguments(this MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel, out ImmutableArray<IParameterSymbol> parameters)
        {
            var objectCreationExpression = GetObjectCreationExpression(methodDeclaration);
            return objectCreationExpression == null ? null : GetArguments(objectCreationExpression, semanticModel, out parameters);
        }

        private static ObjectCreationExpressionSyntax GetObjectCreationExpression(MethodDeclarationSyntax methodDeclaration)
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

        private static ArgumentSyntax[] GetArguments(ObjectCreationExpressionSyntax objectCreationExpression, SemanticModel semanticModel, out ImmutableArray<IParameterSymbol> parameters)
        {
            var constructor = semanticModel.GetSymbolInfo(objectCreationExpression).Symbol as IMethodSymbol;
            if (constructor == null)
                return null;
            parameters = constructor.Parameters;
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
                var nameColon = argument.NameColon;
                if (argument.NameColon == null)
                {
                    result[i] = argument;
                    continue;
                }

                var name = nameColon.Name.Identifier.Text;
                var index = parameters.IndexOf(name);
                if (index < 0 || result[index] != null)
                    return null;
                result[index] = argument;
            }
            return result;
        }
    }
}
