using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Linq;

namespace DevZest.Data.Analyzers
{
    internal static class MounterRegistration
    {
        public static bool IsMounterRegistration(this IMethodSymbol symbol)
        {
            var attributes = symbol.GetAttributes();
            if (attributes == null)
                return false;
            return attributes.Any(x => TypeIdentifier.MounterRegistrationAttribute.IsSameTypeOf(x.AttributeClass));
        }

        public static ModelMemberKind? GetModelMemberKind(this IPropertySymbol property)
        {
            if (!(property.Type is INamedTypeSymbol propertyType))
                return null;

            var parent = property.GetModelMemberParent();
            if (!parent.HasValue)
                return null;

            var parentValue = parent.Value;

            if (TypeIdentifier.Column.IsBaseTypeOf(propertyType))
                return parentValue == ModelMemberParent.Model ? ModelMemberKind.ModelColumn : ModelMemberKind.ColumnGroupMember;
            else if (TypeIdentifier.LocalColumn.IsBaseTypeOf(propertyType))
                return ModelMemberKind.LocalColumn;
            else if (TypeIdentifier.ColumnGroup.IsBaseTypeOf(propertyType))
                return ModelMemberKind.ColumnGroup;
            else if (TypeIdentifier.ColumnList.IsBaseTypeOf(propertyType))
                return ModelMemberKind.ColumnList;
            else if (TypeIdentifier.Model.IsBaseTypeOf(propertyType))
                return ModelMemberKind.ChildModel;
            else
                return null;
        }

        private enum ModelMemberParent
        {
            Model,
            ColumnGroup
        }

        private static ModelMemberParent? GetModelMemberParent(this IPropertySymbol property)
        {
            var containingType = property.ContainingType;
            if (TypeIdentifier.Model.IsBaseTypeOf(containingType))
                return ModelMemberParent.Model;
            else if (TypeIdentifier.ColumnGroup.IsBaseTypeOf(containingType))
                return ModelMemberParent.ColumnGroup;
            else
                return null;
        }

        private static string GetMounterName(this IPropertySymbol propertySymbol)
        {
            return "_" + propertySymbol.Name;
        }

        public static SyntaxNode GenerateMounterDeclaration(this SyntaxGenerator g, INamedTypeSymbol typeSymbol, IPropertySymbol propertySymbol)
        {
            var mounterName = propertySymbol.GetMounterName();
            var propertyTypeName = propertySymbol.Type.Name;

            return g.FieldDeclaration(mounterName, g.GenericName("Mounter", propertySymbol.Type), Accessibility.Public, DeclarationModifiers.Static | DeclarationModifiers.ReadOnly);
        }

        public static SyntaxNode GenerateMounterRegistration(this SyntaxGenerator g, string language, INamedTypeSymbol classSymbol, IPropertySymbol propertySymbol,
            string registerMounterMethodName, bool returnsMounter, bool hasStaticConstructor)
        {
            var type = classSymbol.Name;
            var propertyName = propertySymbol.Name;
            var mounterName = returnsMounter ? propertySymbol.GetMounterName() : null;

            if (hasStaticConstructor)
                return g.AssignmentOrInvocationStatement(language, type, propertyName, registerMounterMethodName, mounterName);
            else
                return g.ConstructorDeclaration(containingTypeName: type, modifiers: DeclarationModifiers.Static,
                    statements: new SyntaxNode[] { g.AssignmentOrInvocationStatement(language, type, propertyName, registerMounterMethodName, mounterName) });
        }

        private static SyntaxNode AssignmentOrInvocationStatement(this SyntaxGenerator g, string language, string type, string propertyName, string registerMounterMethodName, string mounterName)
        {
            if (string.IsNullOrEmpty(mounterName))
                return g.InvocationStatement(language, registerMounterMethodName, type, propertyName);
            else
                return g.AssignmentStatement(language, mounterName, registerMounterMethodName, type, propertyName);
        }

        private static SyntaxNode AssignmentStatement(this SyntaxGenerator g, string language, string mounterName, string methodName, string type, string propertyName)
        {
            return g.AssignmentStatement(g.IdentifierName(mounterName), g.InvocationExpression(language, methodName, type, propertyName));
        }

        private static SyntaxNode InvocationStatement(this SyntaxGenerator g, string language, string methodName, string type, string propertyName)
        {
            return g.ExpressionStatement(g.InvocationExpression(language, methodName, type, propertyName));
        }

        private static SyntaxNode InvocationExpression(this SyntaxGenerator g, string language, string methodName, string type, string propertyName)
        {
            return g.InvocationExpression(g.IdentifierName(methodName), g.GetterArgument(language, type, propertyName));
        }

        private static SyntaxNode GetterArgument(this SyntaxGenerator g, string language, string type, string propertyName)
        {
            return g.Argument(g.Getter(language, type, propertyName));
        }

        private static SyntaxNode Getter(this SyntaxGenerator g, string language, string type, string propertyName)
        {
            return g.ValueReturningLambdaExpression(new SyntaxNode[] { g.GetterParameter(language, type) }, g.GetterBody(language, propertyName));
        }

        private static SyntaxNode GetterParameter(this SyntaxGenerator g, string language, string type)
        {
            return g.ParameterDeclaration(g.GetterLambdaParamName(language), g.IdentifierName(type));
        }

        private static SyntaxNode GetterBody(this SyntaxGenerator g, string language, string propertyName)
        {
            return g.MemberAccessExpression(g.IdentifierName(g.GetterLambdaParamName(language)), propertyName);
        }

        private static string GetterLambdaParamName(this SyntaxGenerator g, string language)
        {
            if (language == LanguageNames.CSharp)
                return "_";
            else if (language == LanguageNames.VisualBasic)
                return "x";
            else
                throw new ArgumentException("Invalid language.", nameof(language));
        }
    }
}
