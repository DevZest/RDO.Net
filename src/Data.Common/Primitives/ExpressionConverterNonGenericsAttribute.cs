using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public sealed class ExpressionConverterNonGenericsAttribute : ExpressionConverterAttribute
    {
        public ExpressionConverterNonGenericsAttribute(Type converterType)
        {
            _converter = (ExpressionConverter)Activator.CreateInstance(converterType);
        }

        private ExpressionConverter _converter;

        public override Type ConverterType
        {
            get { return _converter.GetType(); }
        }

        internal override ExpressionConverter GetConverter(IReadOnlyList<string> argTypeIds)
        {
            Debug.Assert(argTypeIds.Count == 0);
            return _converter;
        }
    }
}
