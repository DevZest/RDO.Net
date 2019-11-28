using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace DevZest.Data.CodeAnalysis
{
    partial class DbMapper
    {
        private sealed partial class CSharp : DbMapper
        {
            public static CSharp GetMapper(DbMapper existingMapper, CodeContext context)
            {
                if (existingMapper is CSharp csharpMapper)
                    return csharpMapper.Refresh(context);

                var dbClass = context.FindClassDeclaration<ClassDeclarationSyntax>(KnownTypes.DbSession, out var dbType);
                if (dbClass == null)
                    return null;

                var result = new CSharp();
                result.Update(context, dbClass, dbType);
                return result;
            }

            private CSharp Refresh(CodeContext context)
            {
                if (Document == context.Document && DbClass.Contains(context.CurrentSyntaxNode))
                    return this;

                var dbClass = context.FindClassDeclaration<ClassDeclarationSyntax>(KnownTypes.DbSession, out var dbType);
                if (dbClass == null)
                    return null;

                Update(context, dbClass, dbType);
                return this;
            }

            public override bool RefreshSelectionChanged(TextSpan selectionSpan)
            {
                var context = CodeContext.Create(Document, selectionSpan);
                context.FindClassDeclaration<ClassDeclarationSyntax>(KnownTypes.DbSession, out var dbType);
                return dbType == DbType;
            }

            public new ClassDeclarationSyntax DbClass { get; private set; }

            protected override SyntaxNode GetDbClass()
            {
                return DbClass;
            }

            private void Update(CodeContext context, ClassDeclarationSyntax dbClass, INamedTypeSymbol dbType)
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
                    var closeBraceToken = DbClass.CloseBraceToken;
                    return closeBraceToken.IsMissing ? default(TextSpan?) : new TextSpan(closeBraceToken.SpanStart, 0);
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
                return syntaxNode.CsGenerateAttribute(name, leadingWhitespaceTrivia, arguments, argumentListAnnotation);
            }

            protected override string GetBackingFieldName(string propertyName)
            {
                return "_" + propertyName.ToCamelCase();
            }

            protected override string ModelParamName => "_";

            protected override string EntityPropertyName
            {
                get { return nameof(DataSet<DummyModel>._); }
            }
        }
    }
}
