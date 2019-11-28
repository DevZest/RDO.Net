using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace DevZest.Data.CodeAnalysis
{
    public sealed class NamespaceSet : HashSet<INamespaceSymbol>
    {
        private sealed class EqualityComparer : IEqualityComparer<INamespaceSymbol>
        {
            public static readonly EqualityComparer Singleton = new EqualityComparer();

            private EqualityComparer()
            {
            }

            public bool Equals(INamespaceSymbol x, INamespaceSymbol y)
            {
                return x.GetFullyQualifiedMetadataName() == y.GetFullyQualifiedMetadataName();
            }

            public int GetHashCode(INamespaceSymbol obj)
            {
                return obj.GetFullyQualifiedMetadataName().GetHashCode();
            }
        }

        public NamespaceSet()
            : base(EqualityComparer.Singleton)
        {
        }

        public NamespaceSet(IEnumerable<INamespaceSymbol> items)
            : base(items, EqualityComparer.Singleton)
        {
        }
    }
}
