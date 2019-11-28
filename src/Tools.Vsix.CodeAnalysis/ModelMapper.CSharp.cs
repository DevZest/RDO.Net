using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DevZest.Data.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        private sealed partial class CSharp : ModelMapper
        {
            public static CSharp GetMapper(ModelMapper existingModelMapper, CodeContext context)
            {
                if (existingModelMapper is CSharp csharpModelMapper)
                    return csharpModelMapper.Refresh(context);

                var modelClass = context.FindClassDeclaration<ClassDeclarationSyntax>(KnownTypes.Model, out var modelType);
                if (modelClass == null)
                    return null;

                var result = new CSharp();
                result.Update(context, modelClass, modelType);
                return result;
            }

            private CSharp Refresh(CodeContext context)
            {
                if (Document == context.Document && ModelClass.Contains(context.CurrentSyntaxNode))
                    return this;

                var modelClass = context.FindClassDeclaration<ClassDeclarationSyntax>(KnownTypes.Model, out var modelType);
                if (modelClass == null)
                    return null;

                Update(context, modelClass, modelType);
                return this;
            }

            public override bool RefreshSelectionChanged(TextSpan selectionSpan)
            {
                var context = CodeContext.Create(Document, selectionSpan);
                context.FindClassDeclaration<ClassDeclarationSyntax>(KnownTypes.Model, out var modelType);
                return modelType == ModelType;
            }

            public new ClassDeclarationSyntax ModelClass { get; private set; }

            protected override SyntaxNode GetModelClass()
            {
                return ModelClass;
            }

            public new TypeSyntax BaseModel { get; private set; }

            protected override SyntaxNode GetBaseModel()
            {
                return BaseModel;
            }

            private void Update(CodeContext context, ClassDeclarationSyntax modelClass, INamedTypeSymbol modelType)
            {
                Update(context);
                ModelClass = modelClass;
                ModelType = modelType;
                BaseModel = ResolveBaseModel();
            }

            private TypeSyntax ResolveBaseModel()
            {
                var baseList = ModelClass.BaseList;
                if (baseList == null)
                    return null;
                var baseTypes = baseList.Types;
                for (int i = 0; i < baseTypes.Count; i++)
                {
                    var baseType = baseTypes[i].Type;
                    if (SemanticModel.GetSymbolInfo(baseType).Symbol == ModelType.BaseType)
                        return baseType;
                }

                return null;
            }

            protected override IEnumerable<SyntaxNode> GenerateColumnProperty(SyntaxGenerator g, ITypeSymbol type, string name)
            {
                var result = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseName(type.Name), name)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                var setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                result = result.AddAccessorListAccessors(getter, setter);

                yield return result;
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
                    var closeBraceToken = ModelClass.CloseBraceToken;
                    return closeBraceToken.IsMissing ? default(TextSpan?) : new TextSpan(closeBraceToken.SpanStart, 0);
                }
            }

            protected override ImmutableArray<IPropertySymbol> GetKeyConstructorArguments(ImmutableArray<IParameterSymbol> constructorParams)
            {
                foreach (var method in ModelClass.Members.OfType<MethodDeclarationSyntax>())
                {
                    var arguments = method.GetKeyConstructorArguments(constructorParams);
                    if (arguments == null)
                        continue;

                    return ImmutableArray.Create(arguments.Select(GetPropertySymbol).ToArray());
                }

                return ImmutableArray<IPropertySymbol>.Empty;
            }

            private IPropertySymbol GetPropertySymbol(ArgumentSyntax argument)
            {
                if (SemanticModel.GetSymbolInfo(argument.Expression).Symbol is IPropertySymbol result)
                    return result.ContainingType == ModelType ? result : null;
                return null;
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

            protected override IEnumerable<SyntaxNode> GenerateFkPropertyGetter(SyntaxGenerator g, string backingFieldName, SyntaxNode assignment)
            {
                var result = SyntaxFactory.BinaryExpression(SyntaxKind.CoalesceExpression,
                     (IdentifierNameSyntax)g.IdentifierName(backingFieldName),
                     SyntaxFactory.Token(SyntaxKind.QuestionQuestionToken),
                     SyntaxFactory.ParenthesizedExpression((ExpressionSyntax)assignment));
                yield return g.ReturnStatement(result);
            }

            protected override IEnumerable<SyntaxNode> GenerateCustomValidatorGetter(SyntaxGenerator g)
            {
                var nameValidate = "Validate";
                var nameGetSourceColumns = "GetSourceColumns";

                {
                    var returnType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
                    var paramNode = g.ParameterDeclaration("dataRow", g.IdentifierName(Compilation.GetKnownType(KnownTypes.DataRow).Name));
                    yield return GenerateNotImplementedLocalFunction(g, returnType, nameValidate, paramNode);
                }

                {
                    var returnType = SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(Compilation.GetKnownType(KnownTypes.IColumns).Name));
                    yield return GenerateNotImplementedLocalFunction(g, returnType, nameGetSourceColumns);
                }

                {
                    var entryTypeNode = g.IdentifierName(Compilation.GetKnownType(KnownTypes.CustomValidatorEntry).Name);
                    yield return g.ReturnStatement(g.ObjectCreationExpression(entryTypeNode, g.IdentifierName(nameValidate), g.IdentifierName(nameGetSourceColumns)));
                }
            }

            private SyntaxNode GenerateNotImplementedLocalFunction(SyntaxGenerator g, TypeSyntax returnType, string name, params SyntaxNode[] paramNodes)
            {
                return SyntaxFactory.LocalFunctionStatement(
                    modifiers: default(SyntaxTokenList),
                    returnType: returnType,
                    identifier: SyntaxFactory.Identifier(name),
                    typeParameterList: null,
                    parameterList: SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(paramNodes)),
                    constraintClauses: default(SyntaxList<TypeParameterConstraintClauseSyntax>),
                    body: SyntaxFactory.Block((StatementSyntax)g.GenerateNotImplemented(Compilation)).WithLeadingTrivia(SyntaxFactory.CarriageReturnLineFeed).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed),
                    expressionBody: null);
            }
        }
    }
}
