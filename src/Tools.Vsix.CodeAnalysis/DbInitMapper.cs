using DevZest.Data.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevZest.Data.CodeAnalysis
{
    public static partial class DbInitMapper
    {
        public static IEnumerable<INamedTypeSymbol> ResolveDbSessionProviderTypes(this Project project, INamedTypeSymbol dbInitializerType, bool isEmptyDb)
        {
            return project.TryGetDbInitCompilation(out var compilation)
                ? ResolveDbSessionProviderTypes(compilation, dbInitializerType.GetDbType(compilation), isEmptyDb)
                : Enumerable.Empty<INamedTypeSymbol>();
        }

        private static INamedTypeSymbol GetDbType(this INamedTypeSymbol dbInitializerType, Compilation compilation)
        {
            var baseType = compilation.GetKnownType(KnownTypes.DbInitializerOf);
            return dbInitializerType.GetArgumentType(baseType, compilation);
        }

        private static IEnumerable<INamedTypeSymbol> ResolveDbSessionProviderTypes(Compilation compilation, INamedTypeSymbol dbSessionType, bool isEmptyDb)
        {
            foreach (var result in GenericTypeVisitor.Walk(compilation, KnownTypes.DbSessionProviderOf, dbSessionType))
            {
                if (result.GetIsEmptyDb(compilation) == isEmptyDb)
                    yield return result;
            }
        }

        public static INamedTypeSymbol GetDbGenSessionProviderType(this CodeContext codeContext)
        {
            var result = codeContext.GetTypeDerivedFrom(KnownTypes.DbSessionProviderOf);
            return (result == null || !result.GetIsEmptyDb(codeContext.Compilation)) ? null : result;
        }

        public static INamedTypeSymbol GetDbInitializerType(this CodeContext codeContext)
        {
            return codeContext.GetTypeDerivedFrom(KnownTypes.DbInitializerOf);
        }

        private static INamedTypeSymbol GetTypeDerivedFrom(this CodeContext codeContext, string baseKnownType)
        {
            if (codeContext.IsEmpty)
                return null;

            var result = codeContext.GetCurrentType(baseKnownType);
            return result == null || result.IsAbstract ? null : result;
        }

        public static bool HasDataSetEntry(this CodeContext codeContext)
        {
            var type = codeContext.GetDbInitializerType();
            return type == null ? false : type.GetDataSetMethods(codeContext).Any();
        }

        private static IEnumerable<IMethodSymbol> GetDataSetMethods(this INamedTypeSymbol mockDbOrDbGeneratorType, CodeContext codeContext)
        {
            return mockDbOrDbGeneratorType.GetMembers().OfType<IMethodSymbol>().Where(x => x.IsDataSetMethod(codeContext));
        }

        private static bool IsDataSetMethod(this IMethodSymbol methodSymbol, CodeContext codeContext)
        {
            return methodSymbol.IsStatic && methodSymbol.Parameters.IsDefaultOrEmpty && methodSymbol.IsLocal(codeContext)
                && methodSymbol.ReturnType != null && methodSymbol.ReturnType.OriginalDefinition.Equals(codeContext.Compilation.GetKnownType(KnownTypes.DataSetOf));
        }

        private static bool IsLocal(this IMethodSymbol methodSymbol, CodeContext codeContext)
        {
            var syntaxRefs = methodSymbol.DeclaringSyntaxReferences;
            return syntaxRefs.Length == 1 && syntaxRefs[0].SyntaxTree == codeContext.SyntaxTree;
        }

        public sealed class DataSetEntry : Model
        {
            static DataSetEntry()
            {
                RegisterLocalColumn((DataSetEntry _) => _.DataSetMethod);
                RegisterLocalColumn((DataSetEntry _) => _.DbTableProperty);
                RegisterLocalColumn((DataSetEntry _) => _.DbTablePropertySelection);
                RegisterLocalColumn((DataSetEntry _) => _.ReferencedTypes);
                RegisterLocalColumn((DataSetEntry _) => _.DataSetMethodBody);
            }

            [Display(Name = "DataSet")]
            public LocalColumn<IMethodSymbol> DataSetMethod { get; private set; }

            [Display(Name = "DbTable")]
            [Required]
            public LocalColumn<IPropertySymbol> DbTableProperty { get; private set; }

            public LocalColumn<IEnumerable> DbTablePropertySelection { get; private set; }

            public LocalColumn<string> ReferencedTypes { get; private set; }

            public LocalColumn<string> DataSetMethodBody { get; private set; }
        }

        public static DataSet<DataSetEntry> GetDataSetEntries(this CodeContext codeContext)
        {
            var result = DataSet<DataSetEntry>.Create();

            var dbInitializerType = codeContext.GetDbInitializerType();
            var _ = result._;
            var dbType = dbInitializerType.GetDbType(codeContext.Compilation);
            foreach (var dataSetMethod in dbInitializerType.GetDataSetMethods(codeContext))
            {
                var dataRow = result.AddRow();
                _.DataSetMethod[dataRow] = dataSetMethod;
                var dbTableProperties = GetDbTableProperties(dbType, dataSetMethod, codeContext.Compilation);
                _.DbTableProperty[dataRow] = dbTableProperties.FirstOrDefault();
                _.DbTablePropertySelection[dataRow] = dbTableProperties.Select(x => new { Value = x, Display = x.Name });
            }

            return result;
        }

        private static IEnumerable<IPropertySymbol> GetDbTableProperties(INamedTypeSymbol dbType, IMethodSymbol dataSetMethod, Compilation compilation)
        {
            var returnType = dataSetMethod.ReturnType as INamedTypeSymbol;
            if (returnType == null || !compilation.GetKnownType(KnownTypes.DataSetOf).Equals(returnType.OriginalDefinition))
                return Enumerable.Empty<IPropertySymbol>();

            var modelType = returnType.TypeArguments[0] as INamedTypeSymbol;
            if (modelType == null)
                return Enumerable.Empty<IPropertySymbol>();
            var genericDbTableType = compilation.GetKnownType(KnownTypes.DbTableOf);
            return dbType.GetMembers().OfType<IPropertySymbol>().Where(x => x.Type.IsDbTable(genericDbTableType, modelType));
        }

        private static bool IsDbTable(this ITypeSymbol type, INamedTypeSymbol genericDbTableType, INamedTypeSymbol modelType)
        {
            return (type is INamedTypeSymbol namedType) && genericDbTableType.Equals(namedType.OriginalDefinition) && modelType.Equals(namedType.TypeArguments[0]);
        }

        public static async Task<Document> GenerateDataSetsAsync(this CodeContext codeContext, DataSet<DataSetEntry> entries, CancellationToken ct)
        {
            var dbInitializerType = codeContext.GetDbInitializerType();

            var imports = entries.GetImports(codeContext.Compilation);

            var editor = await DocumentEditor.CreateAsync(codeContext.Document, ct);
            var _ = entries._;
            for (int i = 0; i < entries.Count; i++)
            {
                var methodSymbol = _.DataSetMethod[i];
                var src = _.DataSetMethodBody[i];
                UpdateMethodBody(editor, methodSymbol, src);
            }

            await editor.AddMissingNamespacesAsync(imports, codeContext.Compilation, dbInitializerType.ContainingNamespace, codeContext.SyntaxTree, codeContext.SemanticModel, ct);
            return await editor.FormatAsync(ct);
        }

        private static void UpdateMethodBody(DocumentEditor editor, IMethodSymbol methodSymbol, string src)
        {
            var syntaxNode = methodSymbol.DeclaringSyntaxReferences[0].GetSyntax();
            var language = methodSymbol.Language;
            if (language == LanguageNames.CSharp)
                CSharp.UpdateMethodBody(editor, syntaxNode, src);
            else if (language == LanguageNames.VisualBasic)
                VisualBasic.UpdateMethodBody(editor, syntaxNode, src);
            else
                throw new NotSupportedException(string.Format("Language {0} is not supported.", language));
        }

        private static NamespaceSet GetImports(this DataSet<DataSetEntry> entries, Compilation compilation)
        {
            var result = new NamespaceSet();
            var _ = entries._;
            for (int i = 0; i < entries.Count; i++)
            {
                var referencedTypes = _.ReferencedTypes[i];
                using (StringReader reader = new StringReader(referencedTypes))
                {
                    for (var referencedType = reader.ReadLine(); referencedType != null; referencedType = reader.ReadLine())
                    {
                        var type = compilation.GetTypeByMetadataName(referencedType);
                        if (type != null)
                            result.Add(type);
                    }
                }
            }

            return result;
        }

        public static DataSet<DbInitInput> GetDbInitInput(this INamedTypeSymbol dbSessionProviderType, Compilation compilation)
        {
            var inputAttributes = GetDbInitInputAttributes(dbSessionProviderType, compilation).OrderBy(x => x.Item1.GetNamedArgument(nameof(DbInitInput.Order), 0)).ToArray();
            if (inputAttributes == null || inputAttributes.Length == 0)
                return null;

            var result = DataSet<DbInitInput>.Create();
            for (int i = 0; i < inputAttributes.Length; i++)
            {
                var attribute = inputAttributes[i].Item1;
                var property = inputAttributes[i].Item2;
                result.AddRow();
                result._.Title[i] = attribute.GetNamedArgument(nameof(DbInitInput.Title), property.Name);
                result._.IsPassword[i] = attribute.GetNamedArgument(nameof(DbInitInput.IsPassword), false);
                result._.EnvironmentVariableName[i] = attribute.GetNamedArgument(nameof(DbInitInput.EnvironmentVariableName), property.Name);
                result._.Order[i] = attribute.GetNamedArgument(nameof(DbInitInput.Order), 0);
            }

            return result;
        }

        private static IEnumerable<(AttributeData, IPropertySymbol)> GetDbInitInputAttributes(this INamedTypeSymbol dbSessionProviderType, Compilation compilation)
        {
            foreach (var property in dbSessionProviderType.GetTypeMembers<IPropertySymbol>(IsPublicStringProperty))
            {
                var attribute = property.GetAttributes().Where(x => compilation.GetKnownType(KnownTypes.InputAttribute).Equals(x.AttributeClass)).FirstOrDefault();
                if (attribute != null)
                    yield return (attribute, property);
            }
        }

        private static bool IsPublicStringProperty(IPropertySymbol property)
        {
            var setMethod = property.SetMethod;
            if (setMethod == null)
                return false;

            if (setMethod.IsStatic)
                return false;

            var parameters = setMethod.Parameters;
            if (parameters == null || parameters.Length != 1)
                return false;

            var parameterType = parameters[0].Type;
            if (parameterType.SpecialType != SpecialType.System_String)
                return false;

            return true;
        }
    }
}
