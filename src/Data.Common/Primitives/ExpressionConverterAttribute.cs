using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public abstract class ExpressionConverterAttribute : Attribute
    {
        public string Id { get; set; }

        public abstract Type ConverterType { get; }

        internal void Initialize(Type expressionType)
        {
            if (string.IsNullOrEmpty(Id))
                Id = expressionType.GetDefaultTypeId();
        }

        internal abstract ExpressionConverter GetConverter(IReadOnlyList<string> argTypeIds);
    }
}
