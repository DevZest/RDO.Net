using DevZest.Data.Primitives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Reflection;

namespace DevZest.Data.DbInit
{
    internal static partial class Extensions
    {
        private const string RESULT = "result";

        private static SyntaxNode DataSetType(this SyntaxGenerator g, Type modelType)
        {
            return g.GenericName(nameof(DataSet), g.IdentifierName(modelType.Name));
        }

        private static SyntaxNode Result(this SyntaxGenerator g)
        {
            return g.IdentifierName(RESULT);
        }

        public static SyntaxNode DataSetCreation(this SyntaxGenerator g, Type modelType, int count)
        {
            var initializer = g.InvocationExpression(g.MemberAccessExpression(g.DataSetType(modelType), nameof(DataSet<DummyModel>.Create)));
            if (count > 0)
                initializer = g.DataSetAddRows(initializer, count);
            return g.LocalDeclarationStatement(g.DataSetType(modelType), RESULT, initializer);
        }

        private static SyntaxNode DataSetAddRows(this SyntaxGenerator g, SyntaxNode dataSetNew, int count)
        {
            return g.InvocationExpression(g.MemberAccessExpression(dataSetNew, nameof(DbInitExtensions.AddRows)), g.LiteralExpression(count));
        }

        public static SyntaxNode ReturnResult(this SyntaxGenerator g)
        {
            return g.ReturnStatement(g.Result());
        }

        public static SyntaxNode ModelVariableDeclaration(this SyntaxGenerator g, string name, Type modelType, string language)
        {
            var entityPropertyName = language == LanguageNames.VisualBasic ? nameof(DataSet<DummyModel>.Entity) : nameof(DataSet<DummyModel>._);
            var initializer = g.MemberAccessExpression(g.IdentifierName(RESULT), entityPropertyName);
            return g.LocalDeclarationStatement(g.IdentifierName(modelType.Name), name, initializer);
        }

        public static SyntaxNode SuspendIdentity(this SyntaxGenerator g, string modelName)
        {
            return g.InvocationExpression(g.MemberAccessExpression(g.IdentifierName(modelName), nameof(Data.Primitives.ModelExtensions.SuspendIdentity)));
        }

        public static SyntaxNode ResumeIdentity(this SyntaxGenerator g, string modelName)
        {
            return g.InvocationExpression(g.MemberAccessExpression(g.IdentifierName(modelName), nameof(Data.Primitives.ModelExtensions.ResumeIdentity)));
        }

        public static Type SafeGetGenericDbInitializerType(this Type dbInitializerType)
        {
            return dbInitializerType.SafeGetGenericBaseType(typeof(DbInitializer<>));
        }

        public static Type SafeGetDbSessionType(this Type dbSessionProviderType)
        {
            return dbSessionProviderType.SafeGetGenericDbSessionProviderType().GenericTypeArguments[0];
        }

        public static Type SafeGetGenericDbSessionProviderType(this Type dbSessionProviderType)
        {
            return dbSessionProviderType.SafeGetGenericBaseType(typeof(DbSessionProvider<>));
        }

        private static Type SafeGetGenericBaseType(this Type dbSessionProviderType, Type genericTypeDefition)
        {
            return dbSessionProviderType.GetGenericBaseType(genericTypeDefition)
                ?? throw new InvalidOperationException(string.Format("Type {0} must derive from {1}.", dbSessionProviderType.FullName, typeof(DbSessionProvider<>).FullName));
        }

        private static Type GetGenericBaseType(this Type dbSessionProviderType, Type genericTypeDefition)
        {
            if (dbSessionProviderType.IsGenericType)
            {
                if (dbSessionProviderType.GetGenericTypeDefinition() == genericTypeDefition)
                    return dbSessionProviderType;
            }

            return dbSessionProviderType.BaseType == null ? null : GetGenericBaseType(dbSessionProviderType.BaseType, genericTypeDefition);
        }

        private static Assembly EntryAssembly
        {
            get { return Assembly.GetEntryAssembly(); }
        }

        public static Type ResoveType(this string typeFullName)
        {
            var result = EntryAssembly.GetType(typeFullName);
            if (result != null)
                return result;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                result = assembly.GetType(typeFullName);
                if (result != null)
                    return result;
            }

            return EntryAssembly.GetType(typeFullName, throwOnError: true);
        }
    }
}
