using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        public abstract class Node : TreeNode
        {
            protected Node(ModelMapper mapper)
            {
                Mapper = mapper;
            }

            public ModelMapper Mapper { get; }

            public abstract NodeKind Kind { get; }

            public virtual ISymbol Symbol
            {
                get { return null; }
            }

            protected Compilation Compilation
            {
                get { return Mapper.Compilation; }
            }

            protected SyntaxTree SyntaxTree
            {
                get { return Mapper.SyntaxTree; }
            }

            public bool CanGetAttributes
            {
                get { return Symbol != null && VerifyCanGetAttributes(Kind); }
            }

            private static bool VerifyCanGetAttributes(NodeKind kind)
            {
                return kind == NodeKind.Column
                    || kind == NodeKind.LocalColumn
                    || kind == NodeKind.PrimaryKey;
            }

            private IReadOnlyList<MemberAttribute> _attributes;
            public IReadOnlyList<MemberAttribute> Attributes
            {
                get
                {
                    if (!CanGetAttributes)
                        return null;

                    if (_attributes == null)
                        _attributes = Mapper.GetMemberAttriubtes(this);

                    return _attributes;
                }
            }

            public Document Remove(MemberAttribute attribute)
            {
                Debug.Assert(attribute.IsChecked);
                return Mapper.RemoveMemberAttributeAsync(attribute.AttributeData).Result;
            }

            public Document Add(MemberAttribute attribute, out TextSpan? textSpan)
            {
                Debug.Assert(!attribute.IsChecked && attribute.IsEnabled);
                var result = Mapper.AddMemberAttribute(Symbol, attribute.AttributeClass, KnownTypes.ModelDesignerSpecAttribute).Result;
                textSpan = result.TextSpan;
                return result.Document;
            }
        }
    }
}
