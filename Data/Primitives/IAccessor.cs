using System;

namespace DevZest.Data.Primitives
{
    internal interface IAccessor<TTarget, TProperty>
    {
        Type OwnerType { get; }
        string Name { get; }
        Type ParentType { get; }
        Type PropertyType { get; }
        TProperty GetProperty(TTarget target);
        TProperty Construct(TTarget target);
    }
}
