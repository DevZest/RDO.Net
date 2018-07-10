using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.Analyzers.VisualBasic
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic, Name = nameof(MounterRegistrationCodeFixProvider)), Shared]
    public class MounterRegistrationCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DiagnosticIds.MissingMounterRegistration); }
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

            var propertyDeclaration = root.FindToken(diagnosticSpan.Start).Parent.SelfOrFirstAncestor<PropertyBlockSyntax>();
            var propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration, context.CancellationToken);
            var kind = propertySymbol.GetModelMemberKind(semanticModel.Compilation).Value;
            if (kind == ModelMemberKind.ChildModel)
            {
                GenerateRegisterChildModel(context, document, semanticModel, propertyDeclaration, propertySymbol, diagnostic);
                return;
            }

            string methodName;
            var returnsMounter = false;
            if (kind == ModelMemberKind.ModelColumn)
            {
                methodName = "RegisterColumn";
                returnsMounter = true;
            }
            else if (kind == ModelMemberKind.LocalColumn)
                methodName = "RegisterLocalColumn";
            else if (kind == ModelMemberKind.Projection)
                methodName = "RegisterProjection";
            else if (kind == ModelMemberKind.ColumnList)
                methodName = "RegisterColumnList";
            else
                return;

            context.RegisterCodeFix(CodeAction.Create(
                    title: methodName,
                    createChangedSolution: ct => GenerateMounterRegistration(methodName, document, propertyDeclaration, propertySymbol, returnsMounter, ct),
                    equivalenceKey: methodName),
                    diagnostic);
        }

        private static void GenerateRegisterChildModel(CodeFixContext context, Document document, SemanticModel semanticModel,
            PropertyBlockSyntax propertyDeclaration, IPropertySymbol propertySymbol, Diagnostic diagnostic)
        {
            var compilation = semanticModel.Compilation;
            var methodName = "RegisterChildModel";
            var pkType = propertySymbol.ContainingType.GetPrimaryKeyType(compilation);
            var childModelType = propertySymbol.Type;
            IPropertySymbol[] foreignKeys = pkType == null ? null : GetForeignKeys(childModelType, pkType).ToArray();

            if (foreignKeys == null)
                context.RegisterCodeFix(CodeAction.Create(
                                    title: methodName,
                                    createChangedSolution: ct => GenerateMounterRegistration(methodName, document, propertyDeclaration, propertySymbol, false, ct),
                                    equivalenceKey: methodName),
                                    diagnostic);
            else
            {
                for (int i = 0; i < foreignKeys.Length; i++)
                {
                    var foreignKey = foreignKeys[i];
                    var title = string.Format("{0} ({1}.{2})", methodName, childModelType.Name, foreignKey.Name);
                    context.RegisterCodeFix(CodeAction.Create(
                                        title: title,
                                        createChangedSolution: ct => GenerateRegisterChildModel(document, semanticModel, propertyDeclaration, propertySymbol, foreignKey, ct),
                                        equivalenceKey: title),
                                        diagnostic);
                }
            }
        }

        private static IEnumerable<IPropertySymbol> GetForeignKeys(ITypeSymbol childModelType, INamedTypeSymbol pkType)
        {
            var members = childModelType.GetMembers();
            for (int i = 0; i < members.Length; i++)
            {
                if (members[i] is IPropertySymbol result)
                {
                    if (result.Type.Equals(pkType))
                        yield return result;
                }
            }
        }

        private static async Task<Solution> GenerateRegisterChildModel(Document document, SemanticModel semanticModel,
            PropertyBlockSyntax propertyDeclaration, IPropertySymbol childProperty, IPropertySymbol foreignKey, CancellationToken ct)
        {
            var staticConstructor = await GetStaticConstructor(childProperty, ct);

            ClassBlockSyntax classDeclaration;
            if (staticConstructor != null)
            {
                document = document.Project.GetDocument(staticConstructor.SyntaxTree);
                classDeclaration = staticConstructor.FirstAncestor<ClassBlockSyntax>();
            }
            else
                classDeclaration = propertyDeclaration.FirstAncestor<ClassBlockSyntax>();

            var editor = await DocumentEditor.CreateAsync(document, ct);
            GenerateRegisterChildModel(editor, staticConstructor, classDeclaration, semanticModel, childProperty, foreignKey);

            return editor.GetChangedDocument().Project.Solution;
        }

        private static async Task<Solution> GenerateMounterRegistration(string registerMounterMethodName, Document document,
            PropertyBlockSyntax propertyDeclaration, IPropertySymbol propertySymbol,
            bool returnsMounter, CancellationToken ct)
        {
            var staticConstructor = await GetStaticConstructor(propertySymbol, ct);

            ClassBlockSyntax classDeclaration;
            if (staticConstructor != null)
            {
                document = document.Project.GetDocument(staticConstructor.SyntaxTree);
                classDeclaration = staticConstructor.FirstAncestor<ClassBlockSyntax>();
            }
            else
                classDeclaration = propertyDeclaration.FirstAncestor<ClassBlockSyntax>();

            var editor = await DocumentEditor.CreateAsync(document, ct);
            if (returnsMounter)
                GenerateMounterFieldDeclaration(editor, registerMounterMethodName, classDeclaration, propertySymbol, ct);
            else if (staticConstructor == null)
                GenerateStaticConstructor(editor, classDeclaration, registerMounterMethodName, propertySymbol, returnsMounter, ct);
            else
                GenerateMounterStatement(editor, classDeclaration, staticConstructor, registerMounterMethodName, propertySymbol, returnsMounter, ct);

            return editor.GetChangedDocument().Project.Solution;
        }

        private static async Task<ConstructorBlockSyntax> GetStaticConstructor(IPropertySymbol propertySymbol, CancellationToken ct)
        {
            var containingType = propertySymbol.ContainingType;
            var staticConstructors = containingType.StaticConstructors;
            if (staticConstructors == null || staticConstructors.Length == 0)
                return null;

            var staticConstructor = staticConstructors[0];
            var references = staticConstructor.DeclaringSyntaxReferences;
            if (references == null || references.Length == 0)   // compiler generated static constructor
                return null;
            return (await references[0].GetSyntaxAsync(ct)).FirstAncestor<ConstructorBlockSyntax>();
        }

        private static void GenerateMounterFieldDeclaration(DocumentEditor editor, string registerMounterMethodName,
            ClassBlockSyntax classDeclaration, IPropertySymbol propertySymbol, CancellationToken ct)
        {
            var semanticModel = editor.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, ct);
            var mounterDeclaration = editor.Generator.GenerateMounterDeclaration(LanguageNames.VisualBasic, classSymbol, propertySymbol, registerMounterMethodName);

            var index = GetMounterDeclarationInsertIndex(classDeclaration, semanticModel);
            editor.InsertMembers(classDeclaration, index, new SyntaxNode[] { mounterDeclaration });
        }

        private static void GenerateStaticConstructor(DocumentEditor editor, ClassBlockSyntax classDeclaration,
            string registerMounterMethodName, IPropertySymbol propertySymbol, bool returnsMounter, CancellationToken ct)
        {
            var semanticModel = editor.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, ct);

            var staticConstructor = editor.Generator.GenerateMounterRegistration(LanguageNames.VisualBasic, classSymbol, propertySymbol, registerMounterMethodName, returnsMounter, false);
            var index = GetMounterDeclarationInsertIndex(classDeclaration, semanticModel);
            editor.InsertMembers(classDeclaration, index, new SyntaxNode[] { staticConstructor });
        }

        private static void GenerateMounterStatement(DocumentEditor editor, ClassBlockSyntax classDeclaration, ConstructorBlockSyntax staticConstructor,
            string registerMounterMethodName, IPropertySymbol propertySymbol, bool returnsMounter, CancellationToken ct)
        {
            var semanticModel = editor.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, ct);

            var statements = staticConstructor.Statements;
            if (statements.Count > 0)
            {
                var statement = editor.Generator.GenerateMounterRegistration(LanguageNames.VisualBasic, classSymbol, propertySymbol, registerMounterMethodName, returnsMounter, true);
                editor.InsertAfter(statements.Last(), new SyntaxNode[] { statement });
            }
            else
            {
                var newStaticConstructor = editor.Generator.GenerateMounterRegistration(LanguageNames.VisualBasic, classSymbol, propertySymbol, registerMounterMethodName, returnsMounter, false);
                editor.ReplaceNode(staticConstructor, newStaticConstructor);
            }
        }

        private static int GetMounterDeclarationInsertIndex(ClassBlockSyntax classDeclaration, SemanticModel semanticModel)
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

        private static bool IsMounterDeclaration(StatementSyntax memberDeclaration, SemanticModel semanticModel)
        {
            if (memberDeclaration is FieldDeclarationSyntax fieldDeclaration)
            {
                var declarators = fieldDeclaration.Declarators;
                if (declarators.Count != 1)
                    return false;
                var names = declarators[0].Names;
                if (names.Count == 0)
                    return false;
                IFieldSymbol fieldSymbol = (IFieldSymbol)semanticModel.GetDeclaredSymbol(names[0]);
                return fieldSymbol.Type.IsTypeOfMounter(semanticModel.Compilation);
            }

            return false;
        }

        private static void GenerateRegisterChildModel(DocumentEditor editor, ConstructorBlockSyntax staticConstructor, ClassBlockSyntax classDeclaration,
            SemanticModel semanticModel, IPropertySymbol childProperty, IPropertySymbol foreignKey)
        {
            if (staticConstructor == null)
            {
                var newStaticConstructor = editor.Generator.GenerateChildModelRegistrationStaticConstructor(LanguageNames.VisualBasic, childProperty, foreignKey);
                var index = GetMounterDeclarationInsertIndex(classDeclaration, semanticModel);
                editor.InsertMembers(classDeclaration, index, new SyntaxNode[] { newStaticConstructor });
            }
            else
            {
                var statements = staticConstructor.Statements;
                if (statements.Count > 0)
                {
                    var statement = editor.Generator.GenerateChildModelRegistration(LanguageNames.VisualBasic, childProperty, foreignKey);
                    editor.InsertAfter(statements.Last(), new SyntaxNode[] { statement });
                }
                else
                {
                    var newStaticConstructor = editor.Generator.GenerateChildModelRegistrationStaticConstructor(LanguageNames.VisualBasic, childProperty, foreignKey);
                    editor.ReplaceNode(staticConstructor, newStaticConstructor);
                }
            }
        }
    }
}
