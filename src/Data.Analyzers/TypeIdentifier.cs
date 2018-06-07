using Microsoft.CodeAnalysis;

namespace DevZest.Data.Analyzers
{
    internal struct TypeIdentifier
    {
        private TypeIdentifier(string @namespace, string typeName, string assemblyName)
        {
            _value = GetTypeIdentifierValue(@namespace, typeName, assemblyName);
        }

        private static string GetTypeIdentifierValue(string @namespace, string typeName, string assemblyName)
        {
            return string.Format("{0}.{1}, {2}", @namespace, typeName, assemblyName);
        }

        private readonly string _value;
        public string Value
        {
            get { return _value; }
        }

        public override string ToString()
        {
            return _value;
        }

        public bool IsSameType(INamedTypeSymbol namedType)
        {
            return _value == GetTypeIdentifierValue(namedType);
        }

        private static string GetTypeIdentifierValue(INamedTypeSymbol namedType)
        {
            return GetTypeIdentifierValue(namedType.ContainingNamespace.ToDisplayString(), namedType.Name, namedType.ContainingAssembly.Name);
        }

        private const string DATA_NAMESPACE = "DevZest.Data";
        private const string DATA_ANNOTATIONS_PRIMITIVES_NAMESPACE = "DevZest.Data.Annotations.Primitives";
        private const string DATA_ASSEMBLY_NAME = DATA_NAMESPACE;

        public static readonly TypeIdentifier MounterRegistrationAttribute = new TypeIdentifier(DATA_ANNOTATIONS_PRIMITIVES_NAMESPACE, nameof(MounterRegistrationAttribute), DATA_ASSEMBLY_NAME);
    }
}
