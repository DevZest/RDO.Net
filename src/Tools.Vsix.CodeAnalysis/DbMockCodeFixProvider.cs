using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace DevZest.Data.CodeAnalysis
{
    public abstract class DbMockCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIds.MissingDbMockFactoryMethod); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var title = "Add factory method";
            context.RegisterCodeFix(CodeAction.Create(
                title: title,
                createChangedSolution: ct => GenerateMissingFactoryMethodAsync(context, ct),
                equivalenceKey: title),
                diagnostics: context.Diagnostics);

            return Task.CompletedTask;
        }

        protected abstract INamedTypeSymbol GetClassSymbol(SyntaxNode root, SemanticModel semanticModel, TextSpan diagnosticSpan, out SyntaxNode classNode, CancellationToken ct);

        private async Task<Solution> GenerateMissingFactoryMethodAsync(CodeFixContext context, CancellationToken ct)
        {
            var document = context.Document;
            var syntaxTree = await document.GetSyntaxTreeAsync(ct).ConfigureAwait(false);
            var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var compilation = semanticModel.Compilation;

            var diagnostic = context.Diagnostics.First<Diagnostic>();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var dbMockType = GetClassSymbol(root, semanticModel, diagnosticSpan, out var classNode, ct);
            var dbSessionType = dbMockType.GetArgumentType(compilation.GetKnownType(KnownTypes.DbMockOf), compilation);

            var imports = new NamespaceSet
            {
                dbMockType,
                dbSessionType,
                compilation.GetKnownType(KnownTypes.TaskOf),
                compilation.GetKnownType(KnownTypes.IProgressOf),
                compilation.GetKnownType(KnownTypes.CancellationToken)
            };

            var editor = await DocumentEditor.CreateAsync(document, ct);
            var g = editor.Generator;
            var methodDeclaration = GenerateMethodDeclaration(g, compilation, dbMockType, dbSessionType).WithAdditionalAnnotations(Formatter.Annotation);
            editor.InsertMembers(classNode, 0, new SyntaxNode[] { methodDeclaration });

            await editor.AddMissingNamespacesAsync(imports, compilation, dbMockType.ContainingNamespace, syntaxTree, semanticModel, ct);
            var result = await editor.FormatAsync(ct);
            return result.Project.Solution;
        }

        private static SyntaxNode GenerateMethodDeclaration(SyntaxGenerator g, Compilation compilation, ITypeSymbol dbMockType, ITypeSymbol dbSessionType)
        {
            const string paramDbName = "db";
            const string paramProgressName = "progress";
            const string paramCancellationTokenName = "ct";

            var paramDb = GenerateParameterDeclaration(g, paramDbName, dbSessionType, false);
            var paramProgress = GenerateParameterDeclaration(g, paramProgressName, compilation.GetIProgressOf(compilation.GetKnownType(KnownTypes.DbInitProgress)), true);
            var paramCancellationToken = GenerateParameterDeclaration(g, paramCancellationTokenName, compilation.GetKnownType(KnownTypes.CancellationToken), true);

            return g.MethodDeclaration("CreateAsync", parameters: new SyntaxNode[] { paramDb, paramProgress, paramCancellationToken },
                returnType: g.TypeExpression(compilation.GetTaskOf(dbSessionType)),
                accessibility: Accessibility.Public,
                modifiers: DeclarationModifiers.Static,
                statements: new SyntaxNode[] { GenerateMethodBody(g, dbMockType, paramDbName, paramProgressName, paramCancellationTokenName)});
        }

        private static SyntaxNode GenerateParameterDeclaration(SyntaxGenerator g, string name, ITypeSymbol type, bool defaultInitializer)
        {
            var initializer = defaultInitializer ? g.DefaultExpression(type) : null;
            return g.ParameterDeclaration(name, g.TypeExpression(type), initializer);
        }

        private static SyntaxNode GenerateMethodBody(SyntaxGenerator g, ITypeSymbol type, string paramDb, string paramProgress, string paramCancellationToken)
        {
            var memberAccess = g.MemberAccessExpression(g.ObjectCreationExpression(type), "MockAsync");
            var invocation = g.InvocationExpression(memberAccess, g.IdentifierName(paramDb), g.IdentifierName(paramProgress), g.IdentifierName(paramCancellationToken));
            return g.ReturnStatement(invocation);
        }
    }
}
