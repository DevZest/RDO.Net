using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevZest.Data.CodeAnalysis
{
    partial struct CodeContext
    {
        private static class CSharp
        {
            private sealed class CSharpDispatcher : LanguageDispatcher
            {
                public static readonly CSharpDispatcher Singleton = new CSharpDispatcher();

                private CSharpDispatcher()
                {
                }

                public override INamedTypeSymbol GetCurrentType(CodeContext codeContext, string knownType)
                {
                    return CSharp.GetCurrentType(codeContext, knownType);
                }

                public override IMethodSymbol GetCurrentMethod(CodeContext codeContext)
                {
                    return CSharp.GetCurrentMethod(codeContext);
                }
            }

            public static LanguageDispatcher GetDispatcher()
            {
                return CSharpDispatcher.Singleton;
            }

            private static INamedTypeSymbol GetCurrentType(CodeContext codeContext, string knownType)
            {
                codeContext.FindClassDeclaration<ClassDeclarationSyntax>(knownType, out var result);
                return result;
            }

            private static IMethodSymbol GetCurrentMethod(CodeContext codeContext)
            {
                var methodDeclaration = codeContext.CurrentSyntaxNode.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                return methodDeclaration == null ? null : codeContext.SemanticModel.GetDeclaredSymbol(methodDeclaration) as IMethodSymbol;
            }
        }
    }
}
