using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DevZest.Data.CodeAnalysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ModelAttributeAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeModelAttribute, SymbolKind.NamedType);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(
                      Rules.DuplicateModelAttribute,
                      Rules.MissingImplementation,
                      Rules.MissingImplementationAttribute,
                      Rules.MultipleImplementationAttributes,
                      Rules.InvalidImplementationAttribute,
                      Rules.MissingModelAttribute); }
        }

        private static void AnalyzeModelAttribute(SymbolAnalysisContext context)
        {
            var modelType = (INamedTypeSymbol)context.Symbol;
            if (!modelType.IsDerivedFrom(KnownTypes.Model, context.Compilation))
                return;

            var compilation = context.Compilation;

            var attributes = ImmutableArray.CreateRange(modelType.GetAttributes().Where(x => x.AttributeClass.IsDerivedFrom(KnownTypes.NamedModelAttribute, compilation)));
            for (int i = 0; i < attributes.Length; i++)
            {
                if (AnalyzeDuplicateModelAttribute(context, attributes, i))
                    continue;
                AnalyzeModelAttribute(context, modelType, attributes[i]);
            }

            var members = modelType.GetMembers();
            for (int i = 0; i < members.Length; i++)
                AnalyzeImplementation(context, modelType, members[i]);
        }

        private static bool AnalyzeDuplicateModelAttribute(SymbolAnalysisContext context, ImmutableArray<AttributeData> attributes, int index)
        {
            if (index == 0)
                return false;

            var current = attributes[index];
            var name = GetName(current);
            for (int i = 0; i < index; i++)
            {
                var prevAttribute = attributes[i];
                if (prevAttribute.AttributeClass.Equals(current.AttributeClass) && GetName(prevAttribute) == name)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rules.DuplicateModelAttribute, current.GetLocation(), current.AttributeClass, name));
                    return true;
                }
            }

            return false;
        }

        private static void AnalyzeModelAttribute(SymbolAnalysisContext context, INamedTypeSymbol modelType, AttributeData attribute)
        {
            var compilation = context.Compilation;

            var implementation = attribute.AttributeClass.GetImplementation(compilation);
            if (!implementation.HasValue)
                return;

            var implementationValue = implementation.Value;
            var name = GetName(attribute);
            var implementationSymbol = GetImplementationSymbol(modelType, name, implementationValue);
            if (implementationSymbol == null)
            {
                var isProperty = implementationValue.IsProperty;
                var parameterTypes = implementationValue.ParameterTypes;
                var returnType = implementationValue.ReturnType;
                context.ReportDiagnostic(Diagnostic.Create(Rules.MissingImplementation, attribute.GetLocation(),
                    (isProperty ? Resources.Property : Resources.Method), GetImplementationSignature(name, isProperty, parameterTypes), returnType));
                return;
            }

            var crossRefAttributeType = attribute.GetCrossReferenceAttributeType(compilation);
            if (crossRefAttributeType == null)
                return;
            if (!implementationSymbol.HasAttribute(crossRefAttributeType))
                context.ReportDiagnostic(Diagnostic.Create(Rules.MissingImplementationAttribute, implementationSymbol.Locations[0], crossRefAttributeType));
        }

        private static string GetImplementationSignature(string name, bool isProperty, INamedTypeSymbol[] parameterTypes)
        {
            var paramList = string.Join(", ", (object[])parameterTypes);
            if (isProperty)
                return parameterTypes.Length == 0 ? name : string.Format("{0}[{1}]", name, paramList);
            else
                return string.Format("{0}({1})", name, paramList);
        }

        private static string GetName(AttributeData namedModelAttribute)
        {
            return namedModelAttribute.GetStringArgument();
        }

        private static ISymbol GetImplementationSymbol(INamedTypeSymbol modelType, string name, (bool IsProperty, INamedTypeSymbol ReturnType, INamedTypeSymbol[] ParameterTypes) implementation)
        {
            var isProperty = implementation.IsProperty;
            var returnType = implementation.ReturnType;
            var parameterTypes = implementation.ParameterTypes;
            return modelType.GetMembers(name).Where(x => IsImplementation(x, isProperty, returnType, parameterTypes)).FirstOrDefault();
        }

        private static bool IsImplementation(ISymbol symbol, bool isProperty, INamedTypeSymbol returnType, ITypeSymbol[] parameterTypes)
        {
            return isProperty ? IsImplementation(symbol as IPropertySymbol, returnType, parameterTypes)
                : IsImplementation(symbol as IMethodSymbol, returnType, parameterTypes);
        }

        private static bool IsImplementation(IPropertySymbol property, INamedTypeSymbol returnType, ITypeSymbol[] parameterTypes)
        {
            return property == null ? false : property.Type.Equals(returnType) && AreEqual(property.Parameters, parameterTypes);
        }

        private static bool IsImplementation(IMethodSymbol method, INamedTypeSymbol returnType, ITypeSymbol[] parameterTypes)
        {
            return method == null ? false : method.ReturnType.Equals(returnType) && AreEqual(method.Parameters, parameterTypes);
        }

        private static bool AreEqual(ImmutableArray<IParameterSymbol> parameters, ITypeSymbol[] parameterTypes)
        {
            if (parameters.Length != parameterTypes.Length)
                return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameterType = parameters[i].Type;
                if (!parameterTypes[i].Equals(parameterType))
                    return false;
            }

            return true;
        }

        private static void AnalyzeImplementation(SymbolAnalysisContext context, INamedTypeSymbol modelType, ISymbol symbol)
        {
            var compilation = context.Compilation;

            var attributes = GetImplementationAttributes(symbol, compilation);
            if (attributes.Length == 0)
                return;

            var name = symbol.Name;

            if (attributes.Length > 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.MultipleImplementationAttributes, symbol.Locations[0],
                    symbol.Kind == SymbolKind.Property ? Resources.Property : Resources.Method, name));
                return;
            }

            var attribute = attributes[0];
            var modelAttributeType = attribute.GetCrossReferenceAttributeType(compilation);
            if (modelAttributeType == null)
                return;

            var implementation = modelAttributeType.GetImplementation(compilation);
            if (!implementation.HasValue)
                return;
            var implementationValue = implementation.Value;
            var isProperty = implementationValue.IsProperty;
            var returnType = implementationValue.ReturnType;
            var parameterTypes = implementationValue.ParameterTypes;
            if (!IsImplementation(symbol, isProperty, implementationValue.ReturnType, implementationValue.ParameterTypes))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.InvalidImplementationAttribute, symbol.Locations[0], attribute.AttributeClass,
                    isProperty ? Resources.Property : Resources.Method, GetImplementationSignature(name, isProperty, parameterTypes), returnType));
                return;
            }

            var modelAttribute = modelType.GetAttributes().Where(x => x.AttributeClass.Equals(modelAttributeType) && GetName(x) == name).FirstOrDefault();
            if (modelAttribute == null)
                context.ReportDiagnostic(Diagnostic.Create(Rules.MissingModelAttribute, symbol.Locations[0], modelAttributeType, name));
        }

        private static ImmutableArray<AttributeData> GetImplementationAttributes(ISymbol symbol, Compilation compilation)
        {
            return ImmutableArray.CreateRange(symbol.GetAttributes().Where(x => x.AttributeClass.IsDerivedFrom(KnownTypes._NamedModelAttribute, compilation)));
        }
    }
}
