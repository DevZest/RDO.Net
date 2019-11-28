using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace DevZest.Data.CodeAnalysis
{
    partial class DbMapper
    {
        private sealed partial class VisualBasic : DbMapper
        {
            public static VisualBasic GetMapper(DbMapper existingMapper, CodeContext context)
            {
                if (existingMapper is VisualBasic visualBasicMapper)
                    return visualBasicMapper.Refresh(context);

                var dbClass = context.FindClassDeclaration<ClassBlockSyntax>(KnownTypes.DbSession, out var dbType);
                if (dbClass == null)
                    return null;

                var result = new VisualBasic();
                result.Update(context, dbClass, dbType);
                return result;
            }

            private VisualBasic Refresh(CodeContext context)
            {
                if (Document == context.Document && DbClass.Contains(context.CurrentSyntaxNode))
                    return this;

                var dbClass = context.FindClassDeclaration<ClassBlockSyntax>(KnownTypes.DbSession, out var dbType);
                if (dbClass == null)
                    return null;

                Update(context, dbClass, dbType);
                return this;
            }

            public override bool RefreshSelectionChanged(TextSpan selectionSpan)
            {
                var context = CodeContext.Create(Document, selectionSpan);
                context.FindClassDeclaration<ClassBlockSyntax>(KnownTypes.DbSession, out var dbType);
                return dbType == DbType;
            }

            public new ClassBlockSyntax DbClass { get; private set; }

            protected override SyntaxNode GetDbClass()
            {
                return DbClass;
            }

            private void Update(CodeContext context, ClassBlockSyntax dbClass, INamedTypeSymbol dbType)
            {
                Update(context);
                DbClass = dbClass;
                DbType = dbType;
            }

            public override bool IsKeyword(string name)
            {
                return SyntaxFacts.IsKeywordKind(SyntaxFacts.GetKeywordKind(name));
            }

            public override bool IsValidIdentifier(string name)
            {
                return SyntaxFacts.IsValidIdentifier(name);
            }

            public override TextSpan? CodeSnippetInsertionSpan
            {
                get
                {
                    var endClassStatement = DbClass.EndClassStatement;
                    return endClassStatement.IsMissing ? default(TextSpan?) : new TextSpan(endClassStatement.SpanStart, 0);
                }
            }

            protected override int AttributeListSyntaxKind
            {
                get { return (int)SyntaxKind.AttributeList; }
            }

            protected override int WhitespaceTrivaKind
            {
                get { return (int)SyntaxKind.WhitespaceTrivia; }
            }

            protected override SyntaxNode GenerateAttribute(SyntaxNode syntaxNode, string name, SyntaxTrivia? leadingWhitespaceTrivia, SyntaxNode[] arguments, SyntaxAnnotation argumentListAnnotation)
            {
                return syntaxNode.VbGenerateAttribute(name, leadingWhitespaceTrivia, arguments, argumentListAnnotation);
            }

            protected override string GetBackingFieldName(string propertyName)
            {
                return "m_" + propertyName;
            }

            protected override string ModelParamName => "x";

            protected override string EntityPropertyName
            {
                get { return nameof(DataSet<DummyModel>.Entity); }
            }
        }
    }
}
