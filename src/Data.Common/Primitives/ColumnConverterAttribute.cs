using DevZest.Data.Utilities;
using System;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public sealed class ColumnConverterAttribute : ColumnConverterProviderAttribute
    {
        public ColumnConverterAttribute(Type converterType)
        {
            Check.NotNull(converterType, nameof(converterType));
            Converter = (ColumnConverter)Activator.CreateInstance(converterType);
        }

        public ColumnConverter Converter { get; private set; }

        internal override void Initialize(Type targetType)
        {
            base.Initialize(targetType);
            Converter.TypeId = TypeId;
        }

        public override Type ColumnType
        {
            get { return Converter.ColumnType; }
        }

        public override Type DataType
        {
            get { return Converter.DataType; }
        }

        internal override ColumnConverter Provide(Column column)
        {
            return Converter;
        }

        internal override ColumnConverter Provide(string typeArg)
        {
            Debug.Assert(typeArg == null);
            return Converter;
        }
    }
}
