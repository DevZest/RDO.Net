using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ModelMemberAttributeAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeModelMemberAttribute, SymbolKind.NamedType);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Rules.InvalidModelMemberAttribute, Rules.ModelMemberAttributeRequiresArgument); }
        }

        private static void AnalyzeModelMemberAttribute(SymbolAnalysisContext context)
        {
            var modelType = (INamedTypeSymbol)context.Symbol;
            if (!modelType.IsDerivedFrom(KnownTypes.Model, context.Compilation))
                return;

            var members = modelType.GetMembers();
            for (int i = 0; i < members.Length; i++)
            {
                var member = members[i];
                if (member is IPropertySymbol property)
                    AnalyzeModelMemberAttribute(context, property, property.Type);
                else if (member is INamedTypeSymbol innerClass)
                    AnalyzeModelMemberAttribute(context, innerClass, innerClass);
            }
        }

        private static void AnalyzeModelMemberAttribute(SymbolAnalysisContext context, ISymbol symbol, ITypeSymbol type)
        {
            var attributes = symbol.GetAttributes();
            for (int i = 0; i < attributes.Length; i++)
                AnalyzeModelMemberAttribute(context, symbol, type, attributes[i]);

        }

        private static void AnalyzeModelMemberAttribute(SymbolAnalysisContext context, ISymbol symbol, ITypeSymbol type, AttributeData attribute)
        {
            var spec = attribute.GetModelMemberAttributeSpec(context.Compilation);
            if (!spec.HasValue)
                return;

            var validOnTypes = spec.Value.ValidOnTypes;
            if (validOnTypes == null)
                return;
            
            validOnTypes = validOnTypes.Where(x => x != null).ToArray();
            if (!IsValid(type, validOnTypes))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.InvalidModelMemberAttribute, attribute.GetLocation(), attribute.AttributeClass, FormatString(validOnTypes), type));
                return;
            }

            if (ArgumentMissing(attribute, spec.Value.RequiresArgument))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.ModelMemberAttributeRequiresArgument, attribute.GetLocation(), attribute.AttributeClass));
                return;
            }
        }

        private static bool IsValid(ITypeSymbol type, INamedTypeSymbol[] validOnTypes)
        {
            Debug.Assert(validOnTypes != null);
                
            for (int i = 0; i < validOnTypes.Length; i++)
            {
                var validOnType = validOnTypes[i];

                if (type.Equals(validOnType) || type.IsDerivedFrom(validOnType))
                    return true;
            }

            return validOnTypes.Length == 0;
        }

        private static bool ArgumentMissing(AttributeData attribute, bool requiresArgument)
        {
            if (!requiresArgument)
                return false;

            return attribute.ConstructorArguments.Length == 0 && attribute.NamedArguments.Length == 0 && HasParameterlessConstructor(attribute);
        }

        private static bool HasParameterlessConstructor(AttributeData attribute)
        {
            var attributeClass = attribute.AttributeClass;
            if (attributeClass == null)
                return false;

            var constructors = attributeClass.Constructors;
            for (int i = 0; i < constructors.Length; i++)
            {
                if (constructors[i].Parameters.Length == 0)
                    return true;
            }
            return false;
        }

        private static string FormatString(INamedTypeSymbol[] validOnTypes)
        {
            return validOnTypes.Length == 1 ? validOnTypes[0].ToString() : string.Format("[{0}]", string.Join(", ", (object[])validOnTypes));
        }
    }
}
