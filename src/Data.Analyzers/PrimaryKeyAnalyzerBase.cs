using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    public abstract class PrimaryKeyAnalyzerBase : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    Rules.PrimaryKeyNotSealed,
                    Rules.PrimaryKeyInvalidConstructors,
                    Rules.PrimaryKeyParameterlessConstructor,
                    Rules.PrimaryKeyInvalidConstructorParam,
                    Rules.PrimaryKeyMissingBaseConstructor,
                    Rules.PrimaryKeySortAttributeConflict,
                    Rules.PrimaryKeyMismatchBaseConstructor,
                    Rules.PrimaryKeyMismatchBaseConstructorArgument,
                    Rules.PrimaryKeyMismatchSortAttribute,
                    Rules.PrimaryKeyInvalidArgument,
                    Rules.PrimaryKeyArgumentNaming,
                    Rules.PkColumnAttributeMissing,
                    Rules.PkColumnAttributeIndexOutOfRange,
                    Rules.PkColumnAttributeDuplicate);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeInvalidPkColumnAttribute, SymbolKind.NamedType);
        }

        private static void AnalyzeInvalidPkColumnAttribute(SymbolAnalysisContext context)
        {
            AnalyzeInvalidPkColumnAttribute(context, (INamedTypeSymbol)context.Symbol);
        }

        private static void AnalyzeInvalidPkColumnAttribute(SymbolAnalysisContext context, INamedTypeSymbol type)
        {
            var compilation = context.Compilation;
            if (type == null || !type.IsDerivedFrom(KnownTypes.Model, compilation))
                return;

            var pkColumnsCount = GetPkColumnsCount(type, compilation);
            if (pkColumnsCount < 0)
                return;

            var pkColumns = pkColumnsCount == 0 ? null : new IPropertySymbol[pkColumnsCount];
            if (pkColumns != null)
                ResolveExistingPkColumns(type.BaseType, pkColumns, compilation);

            var pkColumnAttribute = compilation.GetKnownType(KnownTypes.PkColumnAttribute);
            foreach (var property in type.GetMembers().OfType<IPropertySymbol>())
            {
                var attributes = property.GetAttributes().Where(x => pkColumnAttribute.Equals(x.AttributeClass)).ToArray();
                if (attributes.Length != 1)
                    continue;
                var attribute = attributes[0];
                AnalyzeInvalidPkColumnAttribute(context, property, attribute, pkColumns);
            }
        }

        private static void ResolveExistingPkColumns(INamedTypeSymbol type, IPropertySymbol[] result, Compilation compilation)
        {
            if (compilation.GetKnownType(KnownTypes.GenericModel).Equals(type.OriginalDefinition))
                return;

            ResolveExistingPkColumns(type.BaseType, result, compilation);
            foreach (var property in type.GetMembers().OfType<IPropertySymbol>())
            {
                var index = GetPkColumnAttributeIndex(property, compilation);
                if (!index.HasValue)
                    continue;
                var indexValue = index.Value;
                if (!IsIndexOutOfRange(indexValue, result.Length))
                    result[indexValue] = property;
            }
        }

        private static void AnalyzeInvalidPkColumnAttribute(SymbolAnalysisContext context, IPropertySymbol property, AttributeData attribute, IPropertySymbol[] pkColumns)
        {
            var index = GetPkColumnAttributeIndex(attribute);
            if (!index.HasValue)
                return;

            var indexValue = index.Value;
            var pkColumnsCount = pkColumns == null ? 0 : pkColumns.Length;
            if (IsIndexOutOfRange(indexValue, pkColumnsCount))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.PkColumnAttributeIndexOutOfRange, attribute.GetLocation(), indexValue));
                return;
            }

            if (pkColumns != null)
            {
                if (pkColumns[indexValue] == null)
                {
                    pkColumns[indexValue] = property;
                    return;
                }
                context.ReportDiagnostic(Diagnostic.Create(Rules.PkColumnAttributeDuplicate, attribute.GetLocation(), indexValue, pkColumns[indexValue].Name));
            }
        }

        /// <remakes>Returns -1 to quit analyzer.</remakes>
        private static int GetPkColumnsCount(INamedTypeSymbol modelType, Compilation compilation)
        {
            var pkType = modelType.GetPkType(compilation);
            if (pkType == null)
                return 0;

            var constructors = pkType.Constructors;
            if (constructors.Length != 1)
                return -1;

            var constructor = constructors[0];
            if (constructor.Parameters.Length == 0)
                return -1;
            return constructor.Parameters.Length;
        }

        protected static int? GetPkColumnAttributeIndex(IPropertySymbol property, Compilation compilation)
        {
            var pkColumnAttribute = compilation.GetKnownType(KnownTypes.PkColumnAttribute);
            var attributes = property.GetAttributes().Where(x => pkColumnAttribute.Equals(x.AttributeClass)).ToArray();
            if (attributes.Length != 1)
                return null;
            var attribute = attributes[0];
            return GetPkColumnAttributeIndex(attribute);
        }

        private static int? GetPkColumnAttributeIndex(AttributeData pkColumnAttribute)
        {
            var arguments = pkColumnAttribute.ConstructorArguments;
            if (arguments.IsDefaultOrEmpty)
                return 0;

            var argument = arguments[0];
            if (argument.Value is int result)
                return result;
            else
                return null;
        }

        private static bool IsIndexOutOfRange(int index, int pkColumnsCount)
        {
            return index >= pkColumnsCount || index < 0;
        }

        protected static bool IsPrimaryKey(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSymbol)
        {
            return classSymbol != null && classSymbol.BaseTypeEqualsTo(KnownTypes.PrimaryKey, context.Compilation);
        }

        private static void VerifySealed(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSymbol)
        {
            if (!classSymbol.IsSealed)
                context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeyNotSealed, classSymbol.Locations[0]));
        }

        protected static ImmutableArray<IParameterSymbol> VerifyConstructor(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSymbol, out IMethodSymbol constructorSymbol)
        {
            VerifySealed(context, classSymbol);

            constructorSymbol = classSymbol.GetSingleConstructor();
            if (constructorSymbol == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeyInvalidConstructors, classSymbol.Locations[0]));
                return ImmutableArray<IParameterSymbol>.Empty;
            }

            var parameters = constructorSymbol.Parameters;
            if (parameters.IsEmpty)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeyParameterlessConstructor, constructorSymbol.Locations[0]));
                return parameters;
            }

            bool areParametersValid = true;
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (!parameter.Type.IsDerivedFrom(KnownTypes.Column, context.Compilation))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeyInvalidConstructorParam, parameter.Locations[0], parameter.Name));
                    areParametersValid = false;
                }
                VerifyParameterAttributes(context, parameter);
            }

            return areParametersValid ? parameters : ImmutableArray<IParameterSymbol>.Empty;
        }

        private static void VerifyParameterAttributes(SyntaxNodeAnalysisContext context, IParameterSymbol parameter)
        {
            int ascAttributeIndex = -1;
            int descAttributeIndex = -1;
            var attributes = parameter.GetAttributes();
            for (int i = 0; i < attributes.Length; i++)
            {
                var attributeClass = attributes[i].AttributeClass;
                if (attributeClass.EqualsTo(KnownTypes.AscAttribute, context.Compilation))
                    ascAttributeIndex = i;
                else if (attributeClass.EqualsTo(KnownTypes.DescAttribute, context.Compilation))
                    descAttributeIndex = i;
            }

            if (ascAttributeIndex >= 0 && descAttributeIndex >= 0)
            {
                var attribute = attributes[Math.Max(ascAttributeIndex, descAttributeIndex)];
                context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeySortAttributeConflict, attribute.ApplicationSyntaxReference.GetSyntax().GetLocation()));
            }
        }

        protected static void VerifyMismatchSortAttribute(SyntaxNodeAnalysisContext context, IParameterSymbol parameter, SortDirection sortDirection)
        {
            var paramSortDirection = parameter.GetSortDirection(context.Compilation);
            var isMismatched = paramSortDirection.HasValue ? sortDirection != paramSortDirection.Value : false;
            if (isMismatched)
                context.ReportDiagnostic(Diagnostic.Create(Rules.PrimaryKeyMismatchSortAttribute, parameter.Locations[0], paramSortDirection.Value, sortDirection));
        }

        protected static SortDirection? GetSortDirection(ISymbol expressionSymbol, string methodName, IParameterSymbol constructorParam)
        {
            if (expressionSymbol == null || expressionSymbol != constructorParam)
                return null;

            if (methodName == "Asc")
                return SortDirection.Ascending;
            else if (methodName == "Desc")
                return SortDirection.Descending;
            else
                return null;

        }
    }
}
