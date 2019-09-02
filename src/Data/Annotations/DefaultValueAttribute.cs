using DevZest.Data.Addons;
using DevZest.Data.Annotations.Primitives;
using System;
using System.ComponentModel;

namespace DevZest.Data.Annotations
{
    /// <summary>
    /// Specifies the default value for the column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(ColumnDefault) }, validOnTypes: new Type[] { typeof(Column) })]
    public sealed class DefaultValueAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DefaultValueAttribute"/>, converting the specified value to
        /// the specified type, and using an invariant culture as the translation context.
        /// </summary>
        /// <param name="type">A <see cref="Type"/> that represents the type to convert the value to.</param>
        /// <param name="value">A <see cref="string"/> that can be converted to the type using the <see cref="TypeConverter"/>
        /// for the type and the invariant culture.</param>
        public DefaultValueAttribute(Type type, string value)
        {
            _defaultValue = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(value);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultValueAttribute"/> using a <see cref="bool"/> value.
        /// </summary>
        /// <param name="value">A <see cref="bool"/> that is the default value.</param>
        public DefaultValueAttribute(bool value)
        {
            _defaultValue = value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultValueAttribute"/> using a <see cref="byte"/> value.
        /// </summary>
        /// <param name="value">A <see cref="byte"/> that is the default value.</param>
        public DefaultValueAttribute(byte value)
        {
            _defaultValue = value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultValueAttribute"/> using a <see cref="short"/> value.
        /// </summary>
        /// <param name="value">A <see cref="short"/> that is the default value.</param>
        public DefaultValueAttribute(short value)
        {
            _defaultValue = value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultValueAttribute"/> using a <see cref="int"/> value.
        /// </summary>
        /// <param name="value">A <see cref="int"/> that is the default value.</param>
        public DefaultValueAttribute(int value)
        {
            _defaultValue = value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultValueAttribute"/> using a <see cref="long"/> value.
        /// </summary>
        /// <param name="value">A <see cref="long"/> that is the default value.</param>
        public DefaultValueAttribute(long value)
        {
            _defaultValue = value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultValueAttribute"/> using a <see cref="char"/> value.
        /// </summary>
        /// <param name="value">A <see cref="char"/> that is the default value.</param>
        public DefaultValueAttribute(char value)
        {
            _defaultValue = value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultValueAttribute"/> using a <see cref="double"/> value.
        /// </summary>
        /// <param name="value">A <see cref="double"/> that is the default value.</param>
        public DefaultValueAttribute(double value)
        {
            _defaultValue = value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultValueAttribute"/> using a <see cref="float"/> value.
        /// </summary>
        /// <param name="value">A <see cref="float"/> that is the default value.</param>
        public DefaultValueAttribute(float value)
        {
            _defaultValue = value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultValueAttribute"/> using a <see cref="string"/> value.
        /// </summary>
        /// <param name="value">A <see cref="string"/> that is the default value.</param>
        public DefaultValueAttribute(string value)
        {
            _defaultValue = value;
        }

        private readonly object _defaultValue;

        /// <summary>
        /// Gets or sets the name of default value declaration.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the default value declaration.
        /// </summary>
        public string Description { get; set; }

        /// <inheritdoc />
        protected override void Wireup(Column column)
        {
            column.SetDefaultObject(TypeDescriptor.GetConverter(column.DataType).ConvertFrom(_defaultValue), Name, Description);
        }
    }
}
