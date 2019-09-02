using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies the implementation of the relationship.
    /// </summary>
    [CrossReference(typeof(RelationshipAttribute))]
    public sealed class _RelationshipAttribute : Attribute
    {
    }
}
