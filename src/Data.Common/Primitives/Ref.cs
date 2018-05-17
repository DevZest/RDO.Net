using System;
using System.Linq.Expressions;

namespace DevZest.Data.Primitives
{
    public abstract class Ref : Projection
    {
        internal abstract Type PrimaryKeyType { get; }
    }
}
