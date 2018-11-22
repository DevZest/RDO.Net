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
                    Rules.MissingDeclarationAttribute,
                    Rules.DuplicateDeclarationAttribute,
                    Rules.MissingImplementation,
                    Rules.MissingImplementationAttribute);
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
        }

        private static ImmutableArray<INamedTypeSymbol> GetDeclarationModelTypes(INamedTypeSymbol dbType, string name, Compilation compilation)
        {
            HashSet<INamedTypeSymbol> result = null;
            foreach (var declaration in GetDeclarations(dbType, name, compilation))
            {
                var modelType = GetModelType(declaration);
                if (modelType != null)
                {
                    if (result == null)
                        result = new HashSet<INamedTypeSymbol>();
                    result.Add(modelType);
                }
            }

            return result == null ? default(ImmutableArray<INamedTypeSymbol>) : result.ToImmutableArray();
        }

        private static INamedTypeSymbol GetModelType(IPropertySymbol dbTable)
        {
            return (dbTable.Type is INamedTypeSymbol dbTableType) ? dbTableType.TypeArguments[0] as INamedTypeSymbol : null;
        }

        private static bool IsImplementation(IMethodSymbol implementation, INamedTypeSymbol modelType, Compilation compilation)
        {
            var parameters = implementation.Parameters;
            if (parameters.Length != 1)
                return false;

            return compilation.GetKnownType(KnownTypes.KeyMapping).Equals(implementation.ReturnType) && modelType.Equals(parameters[0].Type);
        }

        private static IEnumerable<IPropertySymbol> GetDeclarations(INamedTypeSymbol dbType, string name, Compilation compilation)
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
            return attributes.Length == 1;
        }
    }
}
