using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DevZest.Data.CodeAnalysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class ModelDeclarationAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeModelDeclartion, SymbolKind.NamedType);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(
                    Rules.MissingDeclarationAttribute,
                    Rules.DuplicateDeclarationAttribute,
                    Rules.MissingImplementation,
                    Rules.MissingImplementationAttribute); }
        }

        private static void AnalyzeModelDeclartion(SymbolAnalysisContext context)
        {
            var modelType = (INamedTypeSymbol)context.Symbol;
            if (!modelType.IsDerivedFrom(KnownTypes.Model, context.Compilation))
                return;

            var members = modelType.GetMembers();
            for (int i = 0; i < members.Length; i++)
                AnalyzeImplementation(context, modelType, members[i]);


            var compilation = context.Compilation;

            var attributes = ImmutableArray.CreateRange(modelType.GetAttributes().Where(x => x.AttributeClass.IsDerivedFrom(KnownTypes.ModelDeclarationAttribute, compilation)));
            for (int i = 0; i < attributes.Length; i++)
            {
                if (AnalyzeDuplicateDeclarationAttribute(context, attributes, i))
                    continue;
                AnalyzeModelDeclarationAttribute(context, modelType, attributes[i]);
            }
        }

        private static void AnalyzeImplementation(SymbolAnalysisContext context, INamedTypeSymbol modelType, ISymbol symbol)
        {
            var compilation = context.Compilation;

            var attributes = GetImplementationAttributes(symbol, compilation);
            for (int i = 0; i < attributes.Length; i++)
                AnalyzeImplementation(context, modelType, symbol, attributes[i], compilation);
        }

        private static void AnalyzeImplementation(SymbolAnalysisContext context, INamedTypeSymbol modelType, ISymbol symbol, AttributeData attribute, Compilation compilation)
        {
            var name = symbol.Name;

            var modelAttributeType = attribute.GetCrossReferenceAttributeType(compilation);
            if (modelAttributeType == null)
                return;

            var modelAttribute = modelType.GetAttributes().Where(x => x.AttributeClass.Equals(modelAttributeType) && x.GetStringArgument() == name).FirstOrDefault();
            if (modelAttribute == null)
                context.ReportDiagnostic(Diagnostic.Create(Rules.MissingDeclarationAttribute, attribute.GetLocation(), modelAttributeType, name));
        }

        private static ImmutableArray<AttributeData> GetImplementationAttributes(ISymbol symbol, Compilation compilation)
        {
            return ImmutableArray.CreateRange(symbol.GetAttributes().Where(x => x.AttributeClass.IsDerivedFrom(KnownTypes.ModelImplementationAttribute, compilation)));
        }

        private static bool AnalyzeDuplicateDeclarationAttribute(SymbolAnalysisContext context, ImmutableArray<AttributeData> attributes, int index)
        {
            if (index == 0)
                return false;

            var current = attributes[index];
            var name = current.GetStringArgument();
            for (int i = 0; i < index; i++)
            {
                var prevAttribute = attributes[i];
                if (prevAttribute.AttributeClass.Equals(current.AttributeClass) && prevAttribute.GetStringArgument() == name)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rules.DuplicateDeclarationAttribute, current.GetLocation(), current.AttributeClass, name));
                    return true;
                }
            }

            return false;
        }

        private static void AnalyzeModelDeclarationAttribute(SymbolAnalysisContext context, INamedTypeSymbol modelType, AttributeData attribute)
        {
            var compilation = context.Compilation;

            var spec = attribute.AttributeClass.GetModelDeclarationSpec(compilation);
            if (!spec.HasValue)
                return;

            var specValue = spec.Value;
            var name = attribute.GetStringArgument();
            if (name == null)
                return;

            var implementation = GetImplementation(modelType, name, specValue);
            if (implementation == null)
            {
                var isProperty = specValue.IsProperty;
                var parameterTypes = specValue.ParameterTypes;
                var returnType = specValue.ReturnType;
                context.ReportDiagnostic(Diagnostic.Create(Rules.MissingImplementation, attribute.GetLocation(),
                    (isProperty ? Resources.StringFormatArg_Property : Resources.StringFormatArg_Method), name, returnType, parameterTypes.FormatString()));
                return;
            }

            var crossRefAttributeType = attribute.GetCrossReferenceAttributeType(compilation);
            if (crossRefAttributeType == null)
                return;
            if (!implementation.HasAttribute(crossRefAttributeType))
                context.ReportDiagnostic(Diagnostic.Create(Rules.MissingImplementationAttribute, implementation.Locations[0], crossRefAttributeType));
        }

        private static ISymbol GetImplementation(INamedTypeSymbol modelType, string name, (bool IsProperty, ITypeSymbol ReturnType, ITypeSymbol[] ParameterTypes) spec)
        {
            var isProperty = spec.IsProperty;
            var returnType = spec.ReturnType;
            var parameterTypes = spec.ParameterTypes;
            return modelType.GetMembers(name).Where(x => IsImplementation(x, isProperty, returnType, parameterTypes)).FirstOrDefault();
        }

        private static bool IsImplementation(ISymbol symbol, bool isProperty, ITypeSymbol returnType, ITypeSymbol[] parameterTypes)
        {
            if (symbol.IsStatic)
                return false;
            return isProperty ? IsPropertyImplementation(symbol as IPropertySymbol, returnType, parameterTypes)
                : IsMethodImplementation(symbol as IMethodSymbol, returnType, parameterTypes);
        }

        private static bool IsPropertyImplementation(IPropertySymbol property, ITypeSymbol returnType, ITypeSymbol[] parameterTypes)
        {
            Debug.Assert(!property.IsStatic);
            return property == null ? false : property.Type.Equals(returnType) && AreEqual(property.Parameters, parameterTypes);
        }

        private static bool IsMethodImplementation(IMethodSymbol method, ITypeSymbol returnType, ITypeSymbol[] parameterTypes)
        {
            Debug.Assert(!method.IsStatic);
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
    }
}
