using Microsoft.CodeAnalysis;

namespace DevZest.Data.Analyzers
{
    internal struct TypeIdentifier
    {
        private TypeIdentifier(string @namespace, string typeName, string assemblyName)
        {
            _namespace = @namespace;
            _typeName = typeName;
            _assemblyName = assemblyName;
        }

        private static string GetTypeIdentifierValue(string @namespace, string typeName, string assemblyName)
        {
            return string.Format("{0}.{1}, {2}", @namespace, typeName, assemblyName);
        }

        private readonly string _namespace;
        private readonly string _typeName;
        private readonly string _assemblyName;

        public override string ToString()
        {
            return string.Format("{0}.{1}, {2}", _namespace, _typeName, _assemblyName);
        }

        private bool IsGeneric
        {
            get { return _typeName.IndexOf('`') >= 0; }
        }

        public bool IsBaseTypeOf(ITypeSymbol namedType)
        {
            for (var baseType = namedType.BaseType; baseType != null; baseType = baseType.BaseType)
            {
                if (IsSameTypeOf(baseType))
                    return true;
            }
            return false;
        }

        public bool IsSameTypeOf(INamedTypeSymbol namedType)
        {
            return IsSameNamespace(namedType) && _typeName == namedType.Name && _assemblyName == namedType.ContainingAssembly.Name;
        }

        private bool IsSameNamespace(INamedTypeSymbol namedTypeSymbol)
        {
            return AreSame(namedTypeSymbol.ContainingNamespace, _namespace, string.IsNullOrEmpty(_namespace) ? -1 : _namespace.Length - 1);
        }

        private static bool AreSame(INamespaceSymbol @namespace, string displayString, int lastIndex)
        {
            if (@namespace.IsGlobalNamespace)
                return lastIndex == -1;

            var name = @namespace.Name;
            for (int i = name.Length - 1; i>= 0; i--)
            {
                if (lastIndex < 0 || displayString[lastIndex--] != name[i])
                    return false;
            }

            var containingNamespace = @namespace.ContainingNamespace;
            if (!containingNamespace.IsGlobalNamespace)
            {
                if (lastIndex < 0 || displayString[lastIndex--] != '.')
                    return false;
            }

            return AreSame(containingNamespace, displayString, lastIndex);
        }

        private const string DATA_NAMESPACE = "DevZest.Data";
        private const string DATA_PRIMITIVES_NAMESPACE = "DevZest.Data.Primitives";
        private const string DATA_ANNOTATIONS_PRIMITIVES_NAMESPACE = "DevZest.Data.Annotations.Primitives";
        private const string DATA_ASSEMBLY_NAME = DATA_NAMESPACE;

        public static readonly TypeIdentifier MounterRegistrationAttribute = new TypeIdentifier(DATA_ANNOTATIONS_PRIMITIVES_NAMESPACE, nameof(MounterRegistrationAttribute), DATA_ASSEMBLY_NAME);
        public static readonly TypeIdentifier Column = new TypeIdentifier(DATA_NAMESPACE, nameof(Column), DATA_ASSEMBLY_NAME);
        public static readonly TypeIdentifier LocalColumn = new TypeIdentifier(DATA_NAMESPACE, nameof(LocalColumn) + "`1", DATA_ASSEMBLY_NAME);
        public static readonly TypeIdentifier ColumnGroup = new TypeIdentifier(DATA_NAMESPACE, nameof(ColumnGroup), DATA_ASSEMBLY_NAME);
        public static readonly TypeIdentifier ColumnList = new TypeIdentifier(DATA_NAMESPACE, nameof(ColumnList), DATA_ASSEMBLY_NAME);
        public static readonly TypeIdentifier Model = new TypeIdentifier(DATA_NAMESPACE, nameof(Model), DATA_ASSEMBLY_NAME);
        public static readonly TypeIdentifier Mounter = new TypeIdentifier(DATA_NAMESPACE, nameof(Mounter) + "`1", DATA_ASSEMBLY_NAME);
    }
}
