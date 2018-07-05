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

            var propertyDeclaration = root.FindToken(diagnosticSpan.Start).Parent.FirstAncestor<PropertyDeclarationSyntax>();
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
                    createChangedSolution: ct => GenerateMounterRegistration("RegisterColumn", document, propertyDeclaration, propertySymbol, false, ct)),
                    diagnostic);
        }

        private static async Task<Solution> GenerateMounterRegistration(string registerMounterMethodName, Document document,
            PropertyDeclarationSyntax propertyDeclaration, IPropertySymbol propertySymbol,
            bool newMounterField, CancellationToken ct)
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
            if (newMounterField)
                GenerateMounterFieldDeclaration(editor, classDeclaration, propertySymbol, ct);

            //if (staticConstructor == null)


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

        private static void GenerateMounterFieldDeclaration(DocumentEditor editor, ClassDeclarationSyntax classDeclaration, IPropertySymbol propertySymbol, CancellationToken ct)
        {
            var semanticModel = editor.SemanticModel;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, ct);
            var mounterDeclaration = editor.Generator.MounterDeclaration(classSymbol, propertySymbol);

            var index = GetMounterDeclarationInsertIndex(classDeclaration, semanticModel);
            editor.InsertMembers(mounterDeclaration, index, new SyntaxNode[] { mounterDeclaration });
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

        #region To be deleted
        //private static FieldDeclarationSyntax MounterDeclaration(INamedTypeSymbol typeSymbol, IPropertySymbol propertySymbol)
        //{
        //    var mounterName = "_" + propertySymbol.Name;
        //    var propertyTypeName = propertySymbol.Type.Name;

        //    return SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(
        //        SyntaxFactory.GenericName(Identifier(mounterName))
        //            .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(IdentifierName(propertyTypeName)))))
        //        .WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(Identifier(mounterName)))))
        //        .WithModifiers(TokenList(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword, SyntaxKind.ReadOnlyKeyword));
        //}

        //private static SyntaxTokenList TokenList(params SyntaxKind[] kinds)
        //{
        //    var tokens = new SyntaxToken[kinds.Length];
        //    for (int i = 0; i < tokens.Length; i++)
        //        tokens[i] = SyntaxFactory.Token(kinds[i]);
        //    return SyntaxFactory.TokenList(tokens);
        //}

        //private static SyntaxToken Token(SyntaxKind kind)
        //{
        //    return SyntaxFactory.Token(kind);
        //}

        //private static ConstructorDeclarationSyntax StaticConstructor(INamedTypeSymbol classSymbol, string propertyName, string registerMounterMethodName, string mounterName)
        //{
        //    var type = classSymbol.Name;

        //    return ConstructorDeclaration(Identifier(type)).WithModifiers(TokenList(SyntaxKind.StaticKeyword))
        //        .WithBody(Block(SingletonList(ConstructorBody(type, propertyName, registerMounterMethodName, mounterName))));
        //}

        //private static SyntaxNode ConstructorBody(string type, string propertyName, string registerMounterMethodName, string mounterName)
        //{
        //    if (string.IsNullOrEmpty(mounterName))
        //        return Register(registerMounterMethodName, type, propertyName);
        //    else
        //        return AssignMounter(mounterName, registerMounterMethodName, type, propertyName);
        //}

        //private static AssignmentExpressionSyntax AssignMounter(string mounterName, string methodName, string type, string propertyName)
        //{
        //    return AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(mounterName), Register(methodName, type, propertyName));
        //}

        //private static InvocationExpressionSyntax Register(string methodName, string type, string propertyName)
        //{
        //    return InvocationExpression(IdentifierName(methodName)).WithArgumentList(SingleArgument(GetterArgument(type, propertyName)));
        //}

        //private static ArgumentListSyntax SingleArgument(ArgumentSyntax argumentSyntax)
        //{
        //    return ArgumentList(SingletonSeparatedList(argumentSyntax));
        //}

        //private static ArgumentSyntax GetterArgument(string type, string propertyName)
        //{
        //    return Argument(Getter(type, propertyName));
        //}

        //private static ParenthesizedLambdaExpressionSyntax Getter(string type, string propertyName)
        //{
        //    return ParenthesizedLambdaExpression(GetterBody(propertyName)).WithParameterList(SingleParameter(GetterParameter(type)));
        //}

        //private static ParameterListSyntax SingleParameter(ParameterSyntax parameterSyntax)
        //{
        //    return ParameterList(SingletonSeparatedList(parameterSyntax));
        //}

        //private static ParameterSyntax GetterParameter(string type)
        //{
        //    return Parameter(Identifier(GETTER_LAMBDA_PARAM)).WithType(IdentifierName(type));
        //}

        //private const string GETTER_LAMBDA_PARAM = "_";

        //private static MemberAccessExpressionSyntax GetterBody(string propertyName)
        //{
        //    return MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(GETTER_LAMBDA_PARAM), IdentifierName(propertyName));
        //}
        #endregion
    }
}
