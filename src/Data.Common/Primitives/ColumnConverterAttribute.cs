using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Primitives
{
    public sealed class ColumnConverterAttribute : Attribute
    {
        public ColumnConverterAttribute(Type converterType)
        {
            Check.NotNull(converterType, nameof(converterType));
            Converter = (ColumnConverter)Activator.CreateInstance(converterType);
        }

        public string TypeId
        {
            get { return Converter.TypeId; }
            set { Converter.TypeId = value; }
        }

        public ColumnConverter Converter { get; private set; }

        public Type ColumnType
        {
            get { return Converter.ColumnType; }
        }

        public Type DataType
        {
            get { return Converter.DataType; }
        }
    }
}
