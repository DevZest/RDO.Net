using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DevZest.Data.CodeAnalysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ForeignKeyAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeForeignKey, SymbolKind.NamedType);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    Rules.DuplicateDeclarationAttribute,
                    Rules.MissingImplementation,
                    Rules.MissingImplementationAttribute,
                    Rules.InvalidImplementationAttribute,
                    Rules.MissingDeclarationAttribute);
            }
        }

        private static void AnalyzeForeignKey(SymbolAnalysisContext context)
        {
            var dbType = (INamedTypeSymbol)context.Symbol;
            if (!dbType.IsDerivedFrom(KnownTypes.DbSession, context.Compilation))
                return;

            var members = dbType.GetMembers().OfType<IMethodSymbol>().ToImmutableArray();
            for (int i = 0; i < members.Length; i++)
                AnalyzeImplementation(context, dbType, members[i]);

            AnalyzeDeclaration(context, dbType);
        }

        private static void AnalyzeImplementation(SymbolAnalysisContext context, INamedTypeSymbol dbType, IMethodSymbol method)
        {
            var compilation = context.Compilation;

            var attributes = method.GetAttributes().Where(x => compilation.GetKnownType(KnownTypes._ForeignKeyAttribute).Equals(x.AttributeClass)).ToImmutableArray();
            if (attributes.Length != 1)
                return;

            AnalyzeImplementation(context, dbType, method, attributes[0]);
        }

        private static void AnalyzeImplementation(SymbolAnalysisContext context, INamedTypeSymbol dbType, IMethodSymbol implementation, AttributeData attribute)
        {
            var name = implementation.Name;
            var compilation = context.Compilation;
            var declarationModelTypes = GetDeclarationModelTypes(dbType, name, compilation);
            if (declarationModelTypes.IsDefaultOrEmpty)
                context.ReportDiagnostic(Diagnostic.Create(Rules.MissingDeclarationAttribute, attribute.GetLocation(),
                    compilation.GetKnownType(KnownTypes.ForeignKeyAttribute), name));
            else if (declarationModelTypes.Length == 1)
            {
                var modelType = declarationModelTypes[0];
                if (!IsImplementation(implementation, modelType, compilation))
                    context.ReportDiagnostic(Diagnostic.Create(Rules.InvalidImplementationAttribute, attribute.GetLocation(), attribute.AttributeClass,
                        Resources.StringFormatArg_Method, compilation.GetKnownType(KnownTypes.KeyMapping), modelType));
            }
        }

        private static ImmutableArray<INamedTypeSymbol> GetDeclarationModelTypes(INamedTypeSymbol dbType, string name, Compilation compilation)
        {
            HashSet<INamedTypeSymbol> result = null;
            foreach (var dbTable in GetDbTablesWithFkDeclaration(dbType, name, compilation))
            {
                var modelType = dbTable.GetModelType();
                if (modelType != null)
                {
                    if (result == null)
                        result = new HashSet<INamedTypeSymbol>();
                    result.Add(modelType);
                }
            }

            return result == null ? default(ImmutableArray<INamedTypeSymbol>) : result.ToImmutableArray();
        }

        private static bool IsImplementation(IMethodSymbol implementation, INamedTypeSymbol modelType, Compilation compilation)
        {
            var parameters = implementation.Parameters;
            if (parameters.Length != 1)
                return false;

            return compilation.GetKnownType(KnownTypes.KeyMapping).Equals(implementation.ReturnType) && modelType.Equals(parameters[0].Type);
        }

        private static IEnumerable<IPropertySymbol> GetDbTablesWithFkDeclaration(INamedTypeSymbol dbType, string name, Compilation compilation)
        {
            var foreignKeyAttribute = compilation.GetKnownType(KnownTypes.ForeignKeyAttribute);

            var dbTables = dbType.GetDbTables(compilation);
            for (int i = 0; i < dbTables.Length; i++)
            {
                var dbTable = dbTables[i];
                if (HasDeclaration(dbTable, foreignKeyAttribute, name))
                    yield return dbTable;
            }
        }

        private static bool HasDeclaration(IPropertySymbol dbTable, INamedTypeSymbol foreignKeyAttribute, string name)
        {
            var attributes = dbTable.GetAttributes().Where(x => foreignKeyAttribute.Equals(x.AttributeClass) && x.GetStringArgument() == name).ToImmutableArray();
            return attributes.Length > 0;
        }

        private static void AnalyzeDeclaration(SymbolAnalysisContext context, INamedTypeSymbol dbType)
        {
            var dbTables = dbType.GetDbTables(context.Compilation);
            if (dbTables.Length == 0)
                return;

            var names = new HashSet<string>();
            for (int i = 0; i < dbTables.Length; i++)
                AnalyzeDeclaration(context, dbType, dbTables[i], names);
        }

        private static void AnalyzeDeclaration(SymbolAnalysisContext context, INamedTypeSymbol dbType, IPropertySymbol dbTable, HashSet<string> names)
        {
            var compilation = context.Compilation;
            var foreignKeyAttribute = compilation.GetKnownType(KnownTypes.ForeignKeyAttribute);
            var attributes = dbTable.GetAttributes().Where(x => foreignKeyAttribute.Equals(x.AttributeClass)).ToImmutableArray();
            for (int i = 0; i < attributes.Length; i++)
                AnalyzeDeclaration(context, dbType, dbTable, attributes[i], names);
        }

        private static void AnalyzeDeclaration(SymbolAnalysisContext context, INamedTypeSymbol dbType, IPropertySymbol dbTable, AttributeData attribute, HashSet<string> names)
        {
            var name = attribute.GetStringArgument();
            if (name == null)
                return;

            if (names.Contains(name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.DuplicateDeclarationAttribute, attribute.GetLocation(), attribute.AttributeClass, name));
                return;
            }

            names.Add(name);

            var modelType = dbTable.GetModelType();
            if (modelType == null)
                return;

            var compilation = context.Compilation;
            var implementation = GetImplementation(dbType, name, modelType, compilation);
            if (implementation == null)
            {
                var keyMappingType = compilation.GetKnownType(KnownTypes.KeyMapping);
                context.ReportDiagnostic(Diagnostic.Create(Rules.MissingImplementation, attribute.GetLocation(),
                    Resources.StringFormatArg_Method, name, keyMappingType, modelType));
                return;
            }

            var implementationAttribute = compilation.GetKnownType(KnownTypes._ForeignKeyAttribute);
            if (!implementation.HasAttribute(implementationAttribute))
                context.ReportDiagnostic(Diagnostic.Create(Rules.MissingImplementationAttribute, implementation.Locations[0], implementationAttribute));
        }

        private static ISymbol GetImplementation(INamedTypeSymbol dbType, string name, INamedTypeSymbol modelType, Compilation compilation)
        {
            return dbType.GetMembers(name).OfType<IMethodSymbol>().Where(x => IsImplementation(x, modelType, compilation)).FirstOrDefault();
        }
    }
}
