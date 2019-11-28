using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        private IReadOnlyList<MemberAttribute> GetMemberAttriubtes(Node node)
        {
            var symbol = node.Symbol;
            Debug.Assert(symbol != null);

            var checkedAttributes = GetCheckedMemberAttributes(symbol).ToList();
            var checkedAttributeFlags = new bool[checkedAttributes.Count];
            var attributes = GetMemberAttributes(symbol);
            for (int i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];
                var checkedOrDisabled = GetCheckedOrDisabled(attribute, checkedAttributes);
                var result = checkedOrDisabled.Result;
                if (result != null)
                {
                    attributes[i] = result;
                    var index = checkedOrDisabled.IndexOfChecked;
                    if (index >= 0)
                        checkedAttributeFlags[index] = true;
                }
            }

            for (int i = 0; i < checkedAttributes.Count; i++)
            {
                if (!checkedAttributeFlags[i])
                    attributes.Add(checkedAttributes[i].MarkAsWarning());
            }

            attributes.Sort(MemberAttribute.Comparer);
            return attributes;
        }

        private static (MemberAttribute Result, int IndexOfChecked) GetCheckedOrDisabled(MemberAttribute attribute, IReadOnlyList<MemberAttribute> checkedAttributes)
        {
            for (var i = 0; i < checkedAttributes.Count; i++)
            {
                var result = checkedAttributes[i];
                if (result.AttributeClass.Equals(attribute.AttributeClass))
                    return (result, i);
            }

            return (GetDisabled(attribute, checkedAttributes), -1);
        }

        private static MemberAttribute GetDisabled(MemberAttribute attribute, IReadOnlyList<MemberAttribute> checkedAttributes)
        {
            var conflict = checkedAttributes.FirstOrDefault(x => HasConflict(attribute, x));
            return conflict == null ? null : attribute.CreateDisabled(conflict);
        }

        private static bool HasConflict(MemberAttribute attribute, MemberAttribute checkedAttribute)
        {
            if (attribute.AddonTypes == null || checkedAttribute.AddonTypes == null)
                return false;
            return attribute.AddonTypes.Intersect(checkedAttribute.AddonTypes).Any();
        }

        private IEnumerable<MemberAttribute> GetCheckedMemberAttributes(ISymbol symbol)
        {
            var attributes = symbol.GetAttributes();
            for (int i = 0; i < attributes.Length; i++)
            {
                var attribute = attributes[i];
                var spec = attribute.GetModelDesignerSpec(Compilation);
                if (spec.HasValue)
                    yield return MemberAttribute.CreateChecked(attribute, spec.Value.AddonTypes);
            }
        }

        private List<MemberAttribute> GetMemberAttributes(ISymbol symbol)
        {
            if (symbol is IPropertySymbol propertySymbol)
                return GetMemberAttributes(AttributeTargets.Property, propertySymbol.Type);
            else if (symbol is INamedTypeSymbol classSymbol)
                return GetMemberAttributes(AttributeTargets.Class, classSymbol);
            else
            {
                Debug.Fail("symbol must be either property or class.");
                return null;
            }
        }

        private sealed class MemberAttributeVisitor : SymbolVisitor
        {
            public static List<MemberAttribute> Walk(Compilation compilation, AttributeTargets propertyOrClass, ITypeSymbol propertyOrClassType)
            {
                return new MemberAttributeVisitor(compilation, propertyOrClass, propertyOrClassType)._result;
            }

            private MemberAttributeVisitor(Compilation compilation, AttributeTargets propertyOrClass, ITypeSymbol propertyOrClassType)
            {
                Debug.Assert(propertyOrClass == AttributeTargets.Property || propertyOrClass == AttributeTargets.Class);
                _compilation = compilation;
                _propertyOrClass = propertyOrClass;
                _propertyOrClassType = propertyOrClassType;
                _attributeType = compilation.GetKnownType(KnownTypes.Attribute);
                if (_attributeType != null)
                {
                    Visit(compilation.Assembly.GlobalNamespace);
                    foreach (var reference in compilation.ExternalReferences)
                    {
                        var symbol = compilation.GetAssemblyOrModuleSymbol(reference);
                        if (symbol is IAssemblySymbol assembly)
                            Visit(assembly.GlobalNamespace);
                    }
                }
            }

            private readonly Compilation _compilation;
            private readonly AttributeTargets _propertyOrClass;
            private readonly ITypeSymbol _propertyOrClassType;
            private readonly INamedTypeSymbol _attributeType;
            private List<MemberAttribute> _result = new List<MemberAttribute>();

            public override void VisitNamespace(INamespaceSymbol symbol)
            {
                foreach (var member in symbol.GetMembers())
                    member.Accept(this);
            }

            public override void VisitNamedType(INamedTypeSymbol symbol)
            {
                var addonTypes = GetAddonTypes(symbol);
                if (addonTypes != null)
                    _result.Add(MemberAttribute.Create(symbol, addonTypes));
            }

            private INamedTypeSymbol[] GetAddonTypes(INamedTypeSymbol symbol)
            {
                if (symbol.IsAbstract)
                    return null;

                if (!symbol.IsDerivedFrom(_attributeType))
                    return null;

                var attributeTargets = symbol.GetAttributeTargets(_compilation);
                if ((attributeTargets & _propertyOrClass) != _propertyOrClass)
                    return null;

                var spec = symbol.GetModelDesignerSpec(_compilation);
                if (!spec.HasValue)
                    return null;

                var specValue = spec.Value;
                if (!VerifyValidOnTypes(specValue.ValidOnTypes))
                    return null;

                return specValue.AddonTypes ?? Array.Empty<INamedTypeSymbol>();
            }

            private bool VerifyValidOnTypes(INamedTypeSymbol[] specValidOnTypes)
            {
                if (specValidOnTypes == null || specValidOnTypes.Length == 0)
                    return true;

                for (int i = 0; i < specValidOnTypes.Length; i++)
                {
                    var specValidOnType = specValidOnTypes[i];
                    if (_propertyOrClassType.Equals(specValidOnType) || _propertyOrClassType.IsDerivedFrom(specValidOnType))
                        return true;
                }

                return false;
            }
        }

        private List<MemberAttribute> GetMemberAttributes(AttributeTargets propertyOrClass, ITypeSymbol propertyOrClassType)
        {
            return MemberAttributeVisitor.Walk(Compilation, propertyOrClass, propertyOrClassType);
        }
    }
}
