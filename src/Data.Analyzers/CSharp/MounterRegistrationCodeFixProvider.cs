using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Analyzers.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MounterRegistrationCodeFixProvider)), Shared]
    public class MounterRegistrationCodeFixProvider : CodeFixProvider
    {
        public MounterRegistrationCodeFixProvider()
        {
            Debug.WriteLine("constructor called!!!");
        }

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIds.MounterRegistration_Missing); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var propertyDeclaration = root.FindToken(diagnosticSpan.Start).Parent.SelfOrFirstAncestor<PropertyDeclarationSyntax>();
            var propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration, context.CancellationToken);
            var kind = propertySymbol.GetModelMemberKind().Value;

            if (kind == ModelMemberKind.ModelColumn)
            {
                context.RegisterCodeFix(CodeAction.Create(
                    title: Resources.Title_AddMounterAndRegisterColumn,
                    createChangedSolution: ct => GenerateMounterRegistration("RegisterColumn", document, propertyDeclaration, propertySymbol, true, ct)),
                    diagnostic);
                context.RegisterCodeFix(CodeAction.Create(
                    title: Resources.Title_AddRegisterColumn,
                    createChangedSolution: ct => GenerateMounterRegistration("RegisterColumn", document, propertyDeclaration, propertySymbol, false, ct)),
                    diagnostic);
            }
            else if (kind == ModelMemberKind.LocalColumn)
                context.RegisterCodeFix(CodeAction.Create(
                    title: Resources.Title_AddRegisterColumn,
                    createChangedSolution: ct => GenerateMounterRegistration("RegisterLocalColumn", document, propertyDeclaration, propertySymbol, false, ct)),
                    diagnostic);
        }

        private static async Task<Solution> GenerateMounterRegistration(string registerMounterMethodName, Document document,
            PropertyDeclarationSyntax propertyDeclaration, IPropertySymbol propertySymbol,
            bool returnsMounter, CancellationToken ct)
        {
            var staticConstructor = await GetStaticConstructor(propertySymbol, ct);

            ClassDeclarationSyntax classDeclaration;
            if (staticConstructor != null)
            {
                document = document.Project.GetDocument(staticConstructor.SyntaxTree);
                classDeclaration = staticConstructor.FirstAncestor<ClassDeclarationSyntax>();
            }
            else
                classDeclaration = propertyDeclaration.FirstAncestor<ClassDeclarationSyntax>();

            int? insertIndex = null;
            var editor = await DocumentEditor.CreateAsync(document, ct);
            if (returnsMounter)
                insertIndex = GenerateMounterFieldDeclaration(editor, classDeclaration, propertySymbol, ct);

            if (staticConstructor == null)
                GenerateStaticConstructor(editor, classDeclaration, insertIndex, registerMounterMethodName, propertySymbol, returnsMounter, ct);
            else
                GenerateMounterStatement(editor, classDeclaration, staticConstructor, registerMounterMethodName, propertySymbol, returnsMounter, ct);

            return editor.GetChangedDocument().Project.Solution;
        }

        private static async Task<ConstructorDeclarationSyntax> GetStaticConstructor(IPropertySymbol propertySymbol, CancellationToken ct)
        {
            var containingType = propertySymbol.ContainingType;
            var staticConstructors = containingType.StaticConstructors;
            if (staticConstructors == null || staticConstructors.Length == 0)
                return null;

            var staticConstructor = staticConstructors[0];
            return (ConstructorDeclarationSyntax)(await staticConstructor.DeclaringSyntaxReferences[0].GetSyntaxAsync(ct));
        }

        private static int GenerateMounterFieldDeclaration(DocumentEditor editor, ClassDeclarationSyntax classDeclaration, IPropertySymbol propertySymbol, CancellationToken ct)
        {
            var semanticModel = editor.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, ct);
            var mounterDeclaration = editor.Generator.GenerateMounterDeclaration(classSymbol, propertySymbol);

            var index = GetMounterDeclarationInsertIndex(classDeclaration, semanticModel);
            editor.InsertMembers(classDeclaration, index, new SyntaxNode[] { mounterDeclaration });
            return index;
        }

        private static void GenerateStaticConstructor(DocumentEditor editor, ClassDeclarationSyntax classDeclaration, int? lastMounterIndex,
            string registerMounterMethodName, IPropertySymbol propertySymbol, bool returnsMounter, CancellationToken ct)
        {
            var semanticModel = editor.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, ct);

            var staticConstructor = editor.Generator.GenerateMounterRegistration(LanguageNames.CSharp, classSymbol, propertySymbol, registerMounterMethodName, returnsMounter, false);
            var index = lastMounterIndex.HasValue ? lastMounterIndex.Value + 1 : GetMounterDeclarationInsertIndex(classDeclaration, semanticModel);
            editor.InsertMembers(classDeclaration, index, new SyntaxNode[] { staticConstructor });
        }

        private static void GenerateMounterStatement(DocumentEditor editor, ClassDeclarationSyntax classDeclaration, ConstructorDeclarationSyntax staticConstructor,
            string registerMounterMethodName, IPropertySymbol propertySymbol, bool returnsMounter, CancellationToken ct)
        {
            var semanticModel = editor.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, ct);

            var body = staticConstructor.Body;
            var statements = body.Statements;
            if (statements.Count > 0)
            {
                var statement = editor.Generator.GenerateMounterRegistration(LanguageNames.CSharp, classSymbol, propertySymbol, registerMounterMethodName, returnsMounter, true);
                editor.InsertAfter(statements.Last(), new SyntaxNode[] { statement });
            }
            else
            {
                var newStaticConstructor = editor.Generator.GenerateMounterRegistration(LanguageNames.CSharp, classSymbol, propertySymbol, registerMounterMethodName, returnsMounter, false);
                editor.ReplaceNode(staticConstructor, newStaticConstructor);
            }
        }

        private static int GetMounterDeclarationInsertIndex(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
        {
            var members = classDeclaration.Members;

            var lastMounterDeclaration = -1;
            for (int i = 0; i < members.Count; i++)
            {
                if (IsMounterDeclaration(members[i], semanticModel))
                    lastMounterDeclaration = i;
            }

            return lastMounterDeclaration + 1;
        }

        private static bool IsMounterDeclaration(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel)
        {
            if (memberDeclaration is FieldDeclarationSyntax fieldDeclaration)
            {
                IFieldSymbol fieldSymbol = (IFieldSymbol)semanticModel.GetDeclaredSymbol(fieldDeclaration);
                return TypeIdentifier.Mounter.IsBaseTypeOf(fieldSymbol.Type);
            }

            return false;
        }
    }
}
