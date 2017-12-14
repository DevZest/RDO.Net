using DevZest.Data.Annotations.Primitives;
using System;
using System.Collections.Generic;

namespace DevZest.Data.Annotations
{
    public sealed class CompositeUniqueAttribute : ValidationColumnGroupAttribute
    {
        public CompositeUniqueAttribute(string name, string message)
            : base(name, message)
        {
        }

        public CompositeUniqueAttribute(string name, Type messageResourceType, string message)
            : base(name, messageResourceType, message)
        {
        }

        public string Description { get; set; }

        public bool IsClustered { get; set; }
    }
}
