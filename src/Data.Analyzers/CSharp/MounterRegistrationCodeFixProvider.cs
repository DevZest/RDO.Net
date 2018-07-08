using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
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

            string methodName;
            var returnsMounter = false;
            if (kind == ModelMemberKind.ModelColumn)
            {
                methodName = "RegisterColumn";
                returnsMounter = true;
            }
            else if (kind == ModelMemberKind.LocalColumn)
                methodName = "RegisterLocalColumn";
            else if (kind == ModelMemberKind.ColumnGroup)
                methodName = "RegisterColumnGroup";
            else if (kind == ModelMemberKind.ColumnList)
                methodName = "RegisterColumnList";
            else if (kind == ModelMemberKind.ChildModel)
                throw new NotImplementedException();
            else
                return;

            context.RegisterCodeFix(CodeAction.Create(
                    title: methodName,
                    createChangedSolution: ct => GenerateMounterRegistration(methodName, document, propertyDeclaration, propertySymbol, returnsMounter, ct),
                    equivalenceKey: methodName),
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

            var editor = await DocumentEditor.CreateAsync(document, ct);
            if (returnsMounter)
                GenerateMounterFieldDeclaration(editor, registerMounterMethodName, classDeclaration, propertySymbol, ct);
            else if (staticConstructor == null)
                GenerateStaticConstructor(editor, classDeclaration, registerMounterMethodName, propertySymbol, returnsMounter, ct);
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
            var references = staticConstructor.DeclaringSyntaxReferences;
            if (references == null || references.Length == 0)   // compiler generated static constructor
                return null;
            return (ConstructorDeclarationSyntax)(await references[0].GetSyntaxAsync(ct));
        }

        private static void GenerateMounterFieldDeclaration(DocumentEditor editor, string registerMounterMethodName,
            ClassDeclarationSyntax classDeclaration, IPropertySymbol propertySymbol, CancellationToken ct)
        {
            var semanticModel = editor.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, ct);
            var mounterDeclaration = editor.Generator.GenerateMounterDeclaration(LanguageNames.CSharp, classSymbol, propertySymbol, registerMounterMethodName);

            var index = GetMounterDeclarationInsertIndex(classDeclaration, semanticModel);
            editor.InsertMembers(classDeclaration, index, new SyntaxNode[] { mounterDeclaration });
        }

        private static void GenerateStaticConstructor(DocumentEditor editor, ClassDeclarationSyntax classDeclaration,
            string registerMounterMethodName, IPropertySymbol propertySymbol, bool returnsMounter, CancellationToken ct)
        {
            var semanticModel = editor.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, ct);

            var staticConstructor = editor.Generator.GenerateMounterRegistration(LanguageNames.CSharp, classSymbol, propertySymbol, registerMounterMethodName, returnsMounter, false);
            var index = GetMounterDeclarationInsertIndex(classDeclaration, semanticModel);
            editor.InsertMembers(classDeclaration, index, new SyntaxNode[] { staticConstructor });
        }

        private static void GenerateMounterStatement(DocumentEditor editor, ClassDeclarationSyntax classDeclaration, ConstructorDeclarationSyntax staticConstructor,
            string registerMounterMethodName, IPropertySymbol propertySymbol, bool returnsMounter, CancellationToken ct)
        {
            var semanticModel = editor.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, ct);

            var statements = staticConstructor.Body.Statements;
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
                var variables = fieldDeclaration.Declaration.Variables;
                if (variables.Count == 0)
                    return false;
                IFieldSymbol fieldSymbol = (IFieldSymbol)semanticModel.GetDeclaredSymbol(variables[0]);
                return TypeIdentifier.Mounter.IsSameTypeOf(fieldSymbol.Type);
            }

            return false;
        }
    }
}
