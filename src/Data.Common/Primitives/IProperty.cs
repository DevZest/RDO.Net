using System;

namespace DevZest.Data.Primitives
{
    internal interface IProperty<TTarget, TProperty>
    {
        Type OwnerType { get; }
        string Name { get; }
        Type ParentType { get; }
        Type PropertyType { get; }
        TProperty GetInstance(TTarget target);
        TProperty Construct(TTarget target);
    }
}
