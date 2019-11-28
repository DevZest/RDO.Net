using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DevZest.Data.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        private sealed partial class VisualBasic : ModelMapper
        {
            public static VisualBasic GetMapper(ModelMapper existingModelMapper, CodeContext context)
            {
                if (existingModelMapper is VisualBasic visualBasicModelMapper)
                    return visualBasicModelMapper.Refresh(context);

                var modelClass = context.FindClassDeclaration<ClassBlockSyntax>(KnownTypes.Model, out var modelType);
                if (modelClass == null)
                    return null;

                var result = new VisualBasic();
                result.Update(context, modelClass, modelType);
                return result;
            }

            private VisualBasic Refresh(CodeContext context)
            {
                if (Document == context.Document && ModelClass.Contains(context.CurrentSyntaxNode))
                    return this;

                var modelClass = context.FindClassDeclaration<ClassBlockSyntax>(KnownTypes.Model, out var modelType);
                if (modelClass == null)
                    return null;

                Update(context, modelClass, modelType);
                return this;
            }

            public override bool RefreshSelectionChanged(TextSpan selectionSpan)
            {
                var context = CodeContext.Create(Document, selectionSpan);
                context.FindClassDeclaration<ClassBlockSyntax>(KnownTypes.Model, out var modelType);
                return modelType == ModelType;
            }

            public new ClassBlockSyntax ModelClass { get; private set; }

            protected override SyntaxNode GetModelClass()
            {
                return ModelClass;
            }

            public new TypeSyntax BaseModel { get; private set; }

            protected override SyntaxNode GetBaseModel()
            {
                return BaseModel;
            }

            private void Update(CodeContext context, ClassBlockSyntax modelClass, INamedTypeSymbol modelType)
            {
                Update(context);
                ModelClass = modelClass;
                ModelType = modelType;
                BaseModel = ResolveBaseModel();
            }

            private TypeSyntax ResolveBaseModel()
            {
                var inherits = ModelClass.Inherits;
                if (inherits.Count != 1)
                    return null;

                var baseTypes = inherits[0].Types;
                if (baseTypes.Count != 1)
                    return null;
                var baseType = baseTypes[0];
                if (SemanticModel.GetSymbolInfo(baseType).Symbol == ModelType.BaseType)
                    return baseType;

                return null;
            }

            protected override IEnumerable<SyntaxNode> GenerateColumnProperty(SyntaxGenerator g, ITypeSymbol type, string name)
            {
                var backingFieldName = "m_" + name;

                yield return g.FieldDeclaration(backingFieldName, g.IdentifierName(type.Name), accessibility: Accessibility.Private);

                var propertyStatement = SyntaxFactory.PropertyStatement(name)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithAsClause(SyntaxFactory.SimpleAsClause((TypeSyntax)g.IdentifierName(type.Name)));

                var getter = SyntaxFactory.AccessorBlock(SyntaxKind.GetAccessorBlock, 
                    SyntaxFactory.AccessorStatement(SyntaxKind.GetAccessorStatement, SyntaxFactory.Token(SyntaxKind.GetKeyword)),
                        new SyntaxList<StatementSyntax>().Add((StatementSyntax)g.ReturnStatement(g.IdentifierName(backingFieldName))),
                    SyntaxFactory.EndGetStatement());
                var setter = SyntaxFactory.AccessorBlock(SyntaxKind.SetAccessorBlock,
                    SyntaxFactory.AccessorStatement(SyntaxKind.SetAccessorStatement, SyntaxFactory.Token(SyntaxKind.SetKeyword)).AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)),
                        new SyntaxList<StatementSyntax>().Add((StatementSyntax)g.AssignmentStatement(g.IdentifierName(backingFieldName), g.IdentifierName("Value"))),
                    SyntaxFactory.EndSetStatement());

                var accessors = new SyntaxList<AccessorBlockSyntax>().AddRange(new AccessorBlockSyntax[] { getter, setter });
                var result = SyntaxFactory.PropertyBlock(propertyStatement, accessors);
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
                    var endClassStatement = ModelClass.EndClassStatement;
                    return endClassStatement.IsMissing ? default(TextSpan?) : new TextSpan(endClassStatement.SpanStart, 0);
                }
            }

            protected override ImmutableArray<IPropertySymbol> GetKeyConstructorArguments(ImmutableArray<IParameterSymbol> constructorParams)
            {
                foreach (var method in ModelClass.Members.OfType<MethodBlockSyntax>())
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
                if (SemanticModel.GetSymbolInfo(argument.GetExpression()).Symbol is IPropertySymbol result)
                    return result.ContainingType == ModelType ? result : null;
                return null;
            }

            protected override SyntaxNode GetLastNodeToAddKeyOrRef()
            {
                return base.GetLastNodeToAddKeyOrRef()?.Parent;
            }

            protected override SyntaxNode GetLastProjectionNode()
            {
               return base.GetLastProjectionNode()?.Parent;
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

            protected override IEnumerable<SyntaxNode> GenerateFkPropertyGetter(SyntaxGenerator g, string backingFieldName, SyntaxNode assignment)
            {
                var isNothing = SyntaxFactory.IsExpression(SyntaxFactory.IdentifierName(backingFieldName), SyntaxFactory.NothingLiteralExpression(SyntaxFactory.Token(SyntaxKind.NothingKeyword)));
                yield return g.IfStatement(isNothing, new SyntaxNode[] { assignment });
                yield return g.ReturnStatement(g.IdentifierName(backingFieldName));
            }

            protected override bool SupportsClassLevelNameOf
            {
                get { return false; }
            }

            protected override SyntaxNode GenerateModelAttributeName(SyntaxGenerator g, string name)
            {
                return g.LiteralExpression(name);
            }

            protected override IEnumerable<SyntaxNode> GenerateCustomValidatorGetter(SyntaxGenerator g)
            {
                var dimValidate = "validate";
                var dimGetSourceColumns = "getSourceColumns";

                {
                    var returnType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
                    var paramNode = g.ParameterDeclaration("dataRow", g.IdentifierName(Compilation.GetKnownType(KnownTypes.DataRow).Name));
                    yield return GenerateNotImplementedLambdaExpression(g, returnType, dimValidate, paramNode);
                }

                {
                    var returnType = SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(Compilation.GetKnownType(KnownTypes.IColumns).Name));
                    yield return GenerateNotImplementedLambdaExpression(g, returnType, dimGetSourceColumns);
                }

                {
                    var entryTypeNode = g.IdentifierName(Compilation.GetKnownType(KnownTypes.CustomValidatorEntry).Name);
                    yield return g.ReturnStatement(g.ObjectCreationExpression(entryTypeNode, g.IdentifierName(dimValidate), g.IdentifierName(dimGetSourceColumns)));
                }
            }

            private SyntaxNode GenerateNotImplementedLambdaExpression(SyntaxGenerator g, TypeSyntax returnType, string name, params SyntaxNode[] paramNodes)
            {
                var lambdaHeader = SyntaxFactory.LambdaHeader(
                    kind: SyntaxKind.FunctionLambdaHeader,
                    attributeLists: default(SyntaxList<AttributeListSyntax>),
                    modifiers: default(SyntaxTokenList),
                    subOrFunctionKeyword: SyntaxFactory.Token(SyntaxKind.FunctionKeyword),
                    parameterList: SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(paramNodes)),
                    asClause: SyntaxFactory.SimpleAsClause(returnType));

                var lambdaExpression = SyntaxFactory.MultiLineFunctionLambdaExpression(
                    subOrFunctionHeader: lambdaHeader,
                    statements: SyntaxFactory.List(new StatementSyntax[] { (StatementSyntax)g.GenerateNotImplemented(Compilation) }),
                    endSubOrFunctionStatement: SyntaxFactory.EndFunctionStatement());

                var variableDeclarator = SyntaxFactory.VariableDeclarator(
                    names: SyntaxFactory.SingletonSeparatedList(SyntaxFactory.ModifiedIdentifier(name)),
                    asClause: null,
                    initializer: SyntaxFactory.EqualsValue(lambdaExpression));

                return SyntaxFactory.LocalDeclarationStatement(
                    modifiers: SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.DimKeyword)),
                    declarators: SyntaxFactory.SingletonSeparatedList(variableDeclarator));
            }
        }
    }
}
