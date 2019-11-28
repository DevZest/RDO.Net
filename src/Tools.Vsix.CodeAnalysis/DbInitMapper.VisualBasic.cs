using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;
using System.Collections.Generic;
using System.IO;

namespace DevZest.Data.CodeAnalysis
{
    static partial class DbInitMapper
    {
        private static class VisualBasic
        {
            public static void UpdateMethodBody(DocumentEditor editor, SyntaxNode syntaxNode, string src)
            {
                var methodBlock = syntaxNode.FirstAncestorOrSelf<MethodBlockSyntax>();
                var statements = new SyntaxList<StatementSyntax>().AddRange(GetStatements(src));
                editor.ReplaceNode(methodBlock, methodBlock.WithStatements(statements));
            }

            private static IEnumerable<StatementSyntax> GetStatements(string src)
            {
                using (StringReader reader = new StringReader(src))
                {
                    for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                        yield return SyntaxFactory.ParseExecutableStatement(line + Environment.NewLine).WithAdditionalAnnotations(Formatter.Annotation);
                }
            }
        }
    }
}
