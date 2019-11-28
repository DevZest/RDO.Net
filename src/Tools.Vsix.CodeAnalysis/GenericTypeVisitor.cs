using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace DevZest.Data.CodeAnalysis
{
    internal sealed class GenericTypeVisitor : SymbolVisitor
    {
        public static IReadOnlyList<INamedTypeSymbol> Walk(Compilation compilation, string baseGenericKnownType, string argumentTypeFullyQualifiedMetadataName)
        {
            var baseGenericType = compilation.GetKnownType(baseGenericKnownType);
            var argumentType = compilation.GetTypeByMetadataName(argumentTypeFullyQualifiedMetadataName);
            return new GenericTypeVisitor(compilation, baseGenericType, argumentType)._result;
        }

        public static IReadOnlyList<INamedTypeSymbol> Walk(Compilation compilation, string baseGenericKnownType, INamedTypeSymbol argumentType)
        {
            var baseGenericType = compilation.GetKnownType(baseGenericKnownType);
            return new GenericTypeVisitor(compilation, baseGenericType, argumentType)._result;
        }

        private GenericTypeVisitor(Compilation compilation, INamedTypeSymbol baseGenericType, INamedTypeSymbol argumentType)
        {
            _compilation = compilation;
            _baseGenericType = baseGenericType;
            _argumentType = argumentType;
            Visit(compilation.Assembly.GlobalNamespace);
        }

        private readonly Compilation _compilation;
        private readonly INamedTypeSymbol _baseGenericType;
        private readonly INamedTypeSymbol _argumentType;
        private List<INamedTypeSymbol> _result = new List<INamedTypeSymbol>();

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (var member in symbol.GetMembers())
                member.Accept(this);
        }

        public override void VisitNamedType(INamedTypeSymbol type)
        {
            if (!type.IsAbstract && type.DeclaredAccessibility == Accessibility.Public && _argumentType.Equals(type.GetArgumentType(_baseGenericType, _compilation)))
                _result.Add(type);
        }
    }
}
