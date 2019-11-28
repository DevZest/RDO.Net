using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.CodeAnalysis.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CSharpPropertyRegistrationCodeFixProvider)), Shared]
    public class CSharpPropertyRegistrationCodeFixProvider : PropertyRegistrationCodeFixProvider
    {
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First<Diagnostic>();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var propertyDeclaration = root.FindToken(diagnosticSpan.Start).Parent.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
            var propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration, context.CancellationToken);
            var type = propertySymbol.GetPropertyRegistrationType(semanticModel.Compilation).Value;
            if (type == PropertyRegistrationType.ChildModel)
                AddRegisterChildModelCodeFix(context, semanticModel, propertySymbol, diagnostic,
                    (methodName, returnsMounter, ct) => GenerateRegisterProperty(methodName, document, propertyDeclaration, propertySymbol, returnsMounter, ct),
                    (foreignKey, ct) => GenerateRegisterChildModel(document, semanticModel, propertyDeclaration, propertySymbol, foreignKey, ct));

            else
                AddRegisterPropertyCodeFix(context, propertySymbol.Name, type, diagnostic,
                    (methodName, returnsMounter, ct) => GenerateRegisterProperty(methodName, document, propertyDeclaration, propertySymbol, returnsMounter, ct));
        }

        private static async Task<Solution> GenerateRegisterChildModel(Document document, SemanticModel semanticModel,
            PropertyDeclarationSyntax propertyDeclaration, IPropertySymbol childProperty, IPropertySymbol foreignKey, CancellationToken ct)
        {
            var staticConstructor = await GetStaticConstructor(childProperty, ct);

            ClassDeclarationSyntax classDeclaration;
            if (staticConstructor != null)
            {
                document = document.Project.GetDocument(staticConstructor.SyntaxTree);
                classDeclaration = staticConstructor.FirstAncestor<ClassDeclarationSyntax>();
            }
            else
                classDeclaration = propertyDeclaration.FirstAncestor<ClassDeclarationSyntax>();

            var editor = await DocumentEditor.CreateAsync(document, ct);
            GenerateRegisterChildModel(editor, staticConstructor, classDeclaration, semanticModel, childProperty, foreignKey);

            return editor.GetChangedDocument().Project.Solution;
        }

        private static async Task<Solution> GenerateRegisterProperty(string registerPropertyMethodName, Document document,
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
                GenerateMounterFieldDeclaration(editor, registerPropertyMethodName, classDeclaration, propertySymbol, ct);
            else if (staticConstructor == null)
                GenerateStaticConstructor(editor, classDeclaration, registerPropertyMethodName, propertySymbol, returnsMounter, ct);
            else
                GenerateRegistrationStatement(editor, classDeclaration, staticConstructor, registerPropertyMethodName, propertySymbol, returnsMounter, ct);

            return editor.GetChangedDocument().Project.Solution;
        }

        private static async Task<ConstructorDeclarationSyntax> GetStaticConstructor(IPropertySymbol propertySymbol, CancellationToken ct)
        {
            var containingType = propertySymbol.ContainingType;
            var staticConstructors = containingType.StaticConstructors;
            if (staticConstructors.IsDefaultOrEmpty)
                return null;

            var staticConstructor = staticConstructors[0];
            var references = staticConstructor.DeclaringSyntaxReferences;
            if (references.IsDefaultOrEmpty)   // compiler generated static constructor
                return null;
            return (ConstructorDeclarationSyntax)(await references[0].GetSyntaxAsync(ct));
        }

        private static void GenerateMounterFieldDeclaration(DocumentEditor editor, string registerPropertyMethodName,
            ClassDeclarationSyntax classDeclaration, IPropertySymbol propertySymbol, CancellationToken ct)
        {
            var semanticModel = editor.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, ct);
            var mounterDeclaration = editor.Generator.GenerateMounterDeclaration(LanguageNames.CSharp, classSymbol, propertySymbol, registerPropertyMethodName);

            var index = GetMounterDeclarationInsertIndex(classDeclaration, semanticModel);
            editor.InsertMembers(classDeclaration, index, new SyntaxNode[] { mounterDeclaration });
        }

        private static void GenerateStaticConstructor(DocumentEditor editor, ClassDeclarationSyntax classDeclaration,
            string registerPropertyMethodName, IPropertySymbol propertySymbol, bool returnsMounter, CancellationToken ct)
        {
            var semanticModel = editor.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, ct);

            var staticConstructor = editor.Generator.GeneratePropertyRegistration(LanguageNames.CSharp, classSymbol, propertySymbol, registerPropertyMethodName, returnsMounter, false);
            var index = GetMounterDeclarationInsertIndex(classDeclaration, semanticModel);
            editor.InsertMembers(classDeclaration, index, new SyntaxNode[] { staticConstructor });
        }

        private static void GenerateRegistrationStatement(DocumentEditor editor, ClassDeclarationSyntax classDeclaration, ConstructorDeclarationSyntax staticConstructor,
            string registerPropertyMethodName, IPropertySymbol propertySymbol, bool returnsMounter, CancellationToken ct)
        {
            var semanticModel = editor.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, ct);

            var statements = staticConstructor.Body.Statements;
            if (statements.Count > 0)
            {
                var statement = editor.Generator.GeneratePropertyRegistration(LanguageNames.CSharp, classSymbol, propertySymbol, registerPropertyMethodName, returnsMounter, true);
                editor.InsertAfter(statements.Last(), new SyntaxNode[] { statement });
            }
            else
            {
                var newStaticConstructor = editor.Generator.GeneratePropertyRegistration(LanguageNames.CSharp, classSymbol, propertySymbol, registerPropertyMethodName, returnsMounter, false);
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
                return fieldSymbol.Type.EqualsTo(KnownTypes.MounterOf, semanticModel.Compilation);
            }

            return false;
        }

        private static void GenerateRegisterChildModel(DocumentEditor editor, ConstructorDeclarationSyntax staticConstructor, ClassDeclarationSyntax classDeclaration,
            SemanticModel semanticModel, IPropertySymbol childProperty, IPropertySymbol foreignKey)
        {
            if (staticConstructor == null)
            {
                var newStaticConstructor = editor.Generator.GenerateChildModelRegistrationStaticConstructor(LanguageNames.CSharp, childProperty, foreignKey);
                var index = GetMounterDeclarationInsertIndex(classDeclaration, semanticModel);
                editor.InsertMembers(classDeclaration, index, new SyntaxNode[] { newStaticConstructor });
            }
            else
            {
                var statements = staticConstructor.Body.Statements;
                if (statements.Count > 0)
                {
                    var statement = editor.Generator.GenerateChildModelRegistration(LanguageNames.CSharp, childProperty, foreignKey);
                    editor.InsertAfter(statements.Last(), new SyntaxNode[] { statement });
                }
                else
                {
                    var newStaticConstructor = editor.Generator.GenerateChildModelRegistrationStaticConstructor(LanguageNames.CSharp, childProperty, foreignKey);
                    editor.ReplaceNode(staticConstructor, newStaticConstructor);
                }
            }
        }
    }
}
