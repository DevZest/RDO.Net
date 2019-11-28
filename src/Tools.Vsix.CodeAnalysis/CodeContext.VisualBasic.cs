using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace DevZest.Data.CodeAnalysis
{
    partial struct CodeContext
    {
        private static class VisualBasic
        {
            private sealed class VisualBasicDispatcher : LanguageDispatcher
            {
                public static readonly VisualBasicDispatcher Singleton = new VisualBasicDispatcher();

                private VisualBasicDispatcher()
                {
                }

                public override INamedTypeSymbol GetCurrentType(CodeContext codeContext, string knownType)
                {
                    return VisualBasic.GetCurrentType(codeContext, knownType);
                }

                public override IMethodSymbol GetCurrentMethod(CodeContext codeContext)
                {
                    return VisualBasic.GetCurrentMethod(codeContext);
                }
            }

            public static LanguageDispatcher GetDispatcher()
            {
                return VisualBasicDispatcher.Singleton;
            }

            private static INamedTypeSymbol GetCurrentType(CodeContext codeContext, string knownType)
            {
                codeContext.FindClassDeclaration<ClassBlockSyntax>(knownType, out var result);
                return result;
            }

            private static IMethodSymbol GetCurrentMethod(CodeContext codeContext)
            {
                var methodBlock = codeContext.CurrentSyntaxNode.FirstAncestorOrSelf<MethodBlockSyntax>();
                if (methodBlock == null)
                    return null;
                var subOrFunctionStatement = methodBlock.SubOrFunctionStatement;
                return subOrFunctionStatement == null ? null : codeContext.SemanticModel.GetDeclaredSymbol(subOrFunctionStatement) as IMethodSymbol;
            }
        }
    }
}
