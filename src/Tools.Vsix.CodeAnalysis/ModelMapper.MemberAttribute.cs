using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        public abstract class MemberAttribute
        {
            public static IComparer<MemberAttribute> Comparer
            {
                get { return _Comparer.Singleton; }
            }

            private sealed class _Comparer : IComparer<MemberAttribute>
            {
                public static _Comparer Singleton = new _Comparer();
                private _Comparer()
                {
                }

                public int Compare(MemberAttribute x, MemberAttribute y)
                {
                    var kindX = GetKind(x);
                    var kindY = GetKind(y);
                    return kindX == kindY ? string.Compare(x.Name, y.Name) : kindX.CompareTo(kindY);
                }

                private static int GetKind(MemberAttribute x)
                {
                    if (x.IsChecked)
                        return 0;
                    else if (x.IsEnabled)
                        return 1;
                    else
                        return 2;
                }
            }

            internal static MemberAttribute Create(INamedTypeSymbol attributeClass, INamedTypeSymbol[] addonTypes)
            {
                return new NormalMemberAttribute(attributeClass, addonTypes);
            }

            internal static MemberAttribute CreateChecked(AttributeData attributeData, INamedTypeSymbol[] addonTypes)
            {
                return new CheckedMemberAttribute(attributeData, addonTypes);
            }

            private sealed class NormalMemberAttribute : MemberAttribute
            {
                public NormalMemberAttribute(INamedTypeSymbol attributeClass, INamedTypeSymbol[] addonTypes)
                    : base(addonTypes)
                {
                    _attributeClass = attributeClass;
                }

                private readonly INamedTypeSymbol _attributeClass;
                public override INamedTypeSymbol AttributeClass
                {
                    get { return _attributeClass; }
                }

                public override MemberAttribute CreateDisabled(MemberAttribute conflict)
                {
                    return new DisabledMemberAttribute(AttributeClass, conflict);
                }
            } 
             
            private sealed class CheckedMemberAttribute : MemberAttribute
            {
                public CheckedMemberAttribute(AttributeData attributeData, INamedTypeSymbol[] addonTypes)
                    : base(addonTypes)
                {
                    _attributeData = attributeData;
                }

                private readonly AttributeData _attributeData;
                public override AttributeData AttributeData
                {
                    get { return _attributeData; }
                }

                public override INamedTypeSymbol AttributeClass
                {
                    get { return _attributeData.AttributeClass; }
                }

                public override bool IsChecked
                {
                    get { return true; }
                }

                private bool _markedAsWarning;
                public override bool MarkedAsWarning
                {
                    get { return _markedAsWarning; }
                }

                public override MemberAttribute MarkAsWarning()
                {
                    _markedAsWarning = true;
                    return this;
                }
            }

            private sealed class DisabledMemberAttribute : MemberAttribute
            {
                public DisabledMemberAttribute(INamedTypeSymbol attributeClass, MemberAttribute confliction)
                    : base(null)
                {
                    _attributeClass = attributeClass;
                    _confliction = confliction;
                }

                private readonly INamedTypeSymbol _attributeClass;
                public override INamedTypeSymbol AttributeClass
                {
                    get { return _attributeClass; }
                }

                private readonly MemberAttribute _confliction;
                public override MemberAttribute Conflict
                {
                    get { return _confliction; }
                }
            }

            protected MemberAttribute(INamedTypeSymbol[] addonTypes)
            {
                AddonTypes = addonTypes;
            }

            public virtual AttributeData AttributeData
            {
                get { return null; }
            }

            public abstract INamedTypeSymbol AttributeClass { get; }

            public IReadOnlyList<INamedTypeSymbol> AddonTypes { get; }

            public virtual MemberAttribute Conflict
            {
                get { return null; }
            }

            public virtual bool IsChecked
            {
                get { return false; }
            }

            public bool IsEnabled
            {
                get { return Conflict == null; }
            }

            public virtual bool MarkedAsWarning
            {
                get { return false; }
            }

            public virtual MemberAttribute MarkAsWarning()
            {
                throw new NotSupportedException();
            }

            public string Name
            {
                get
                {
                    var name = AttributeClass.Name;
                    var result = name.ToAttributeName();
                    return MarkedAsWarning ? result + "?" : result;
                }
            }

            public string DisplayName
            {
                get { return Conflict == null ? Name : string.Format("{0} < {1}", Name, Conflict.Name); }
            }

            public virtual MemberAttribute CreateDisabled(MemberAttribute confliction)
            {
                return null;
            }
        }
    }
}
