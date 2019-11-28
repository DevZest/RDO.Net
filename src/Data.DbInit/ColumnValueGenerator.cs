using DevZest.Data.Primitives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;

namespace DevZest.Data.DbInit
{
    /// <summary>
    /// Generates source code for column value assignment.
    /// </summary>
    /// <remarks><see cref="ColumnValueGenerator"/> objects are registered by column data type via <see cref="Register{T}"/> method.
    /// Built in column types under <see cref="DevZest.Data"/> namespace are registered already. If no <see cref="ColumnValueGenerator"/>
    /// found for given data type, a fallback generator is used, which use the column's JSON serializer and deserializer.
    /// You can provide your own implementation by deriving your class from <see cref="ColumnValueGenerator{T}"/>, 
    /// override <see cref="ColumnValueGenerator{T}.GenerateValue(T)"/>, and call <see cref="Register{T}"/>.</remarks>
    public abstract class ColumnValueGenerator
    {
        private sealed class FallbackColumnValueGenerator : ColumnValueGenerator
        {
            private Column _column;
            internal override void Initialize(Column column)
            {
                _column = column;
            }

            internal override Column GetColumn()
            {
                return _column;
            }

            protected internal override SyntaxNode Generate(int ordinal)
            {
                var jsonValue = _column.Serialize(ordinal);
                var column = GenerateColumn();
                var g = Generator;
                return g.InvocationExpression(g.MemberAccessExpression(column, "Deserialize"), g.LiteralExpression(ordinal), Generate(jsonValue));
            }

            private SyntaxNode Generate(JsonValue jsonValue)
            {
                var g = Generator;
                if (jsonValue.Type == JsonValueType.Null)
                    return g.DottedName("JsonValue.Null");
                else if (jsonValue.Type == JsonValueType.True)
                    return g.DottedName("JsonValue.True");
                else if (jsonValue.Type == JsonValueType.False)
                    return g.DottedName("JsonValue.False");
                else if (jsonValue.Type == JsonValueType.Number)
                    return g.InvocationExpression(g.MemberAccessExpression(g.IdentifierName("JsonValue"), "Number"), g.LiteralExpression(jsonValue.Text));
                else if (jsonValue.Type == JsonValueType.String)
                    return g.InvocationExpression(g.MemberAccessExpression(g.IdentifierName("JsonValue"), "String"), g.LiteralExpression(jsonValue.Text));
                else
                    return null;
            }
        }

        private sealed class EnumColumnValueGenerator<T> : ColumnValueGenerator<T?>
            where T : struct, IConvertible
        {
            protected override SyntaxNode GenerateValue(T? value)
            {
                var g = Generator;
                if (!value.HasValue)
                    return g.NullLiteralExpression();

                var enumValue = value.Value;
                var enumValueType = enumValue.GetType();
                AddReferencedType(enumValueType);
                return g.DottedName(string.Format("{0}.{1}", enumValueType.Name, enumValue.ToString()));
            }
        }

        private sealed class _BinaryGenerator : ColumnValueGenerator<Binary>
        {
            protected override SyntaxNode GenerateValue(Binary value)
            {
                var g = Generator;
                if (value == null)
                    return g.NullLiteralExpression();

                AddReferencedType(typeof(Convert));
                AddReferencedType(typeof(Binary));
                var base64String = g.LiteralExpression(Convert.ToBase64String(value.ToArray()));
                var convert = g.InvocationExpression(g.MemberAccessExpression(g.IdentifierName(nameof(Convert)), nameof(Convert.FromBase64String)), base64String);
                return g.ObjectCreationExpression(g.IdentifierName(nameof(Binary)), convert);
            }
        }

        private sealed class _GuidGenerator : ColumnValueGenerator<Guid?>
        {
            protected override SyntaxNode GenerateValue(Guid? value)
            {
                var g = Generator;
                if (!value.HasValue)
                    return g.NullLiteralExpression();

                AddReferencedType(typeof(Guid));
                return g.ObjectCreationExpression(g.IdentifierName(nameof(Guid)), g.LiteralExpression(value.Value.ToString()));
            }
        }

        private sealed class _DateTimeGenerator : ColumnValueGenerator<DateTime?>
        {
            protected override SyntaxNode GenerateValue(DateTime? value)
            {
                var g = Generator;
                if (!value.HasValue)
                    return g.NullLiteralExpression();

                AddReferencedType(typeof(Convert));
                AddReferencedType(typeof(DateTime));
                var text = g.LiteralExpression(Column.Serialize(RowOrdinal).Text);
                return g.InvocationExpression(g.MemberAccessExpression(g.IdentifierName(nameof(Convert)), nameof(Convert.ToDateTime)), text);
            }
        }

        private sealed class _DateTimeOffsetGenerator : ColumnValueGenerator<DateTimeOffset?>
        {
            protected override SyntaxNode GenerateValue(DateTimeOffset? value)
            {
                var g = Generator;
                if (!value.HasValue)
                    return g.NullLiteralExpression();

                AddReferencedType(typeof(DateTimeOffset));
                var text = g.LiteralExpression(Column.Serialize(RowOrdinal).Text);
                return g.InvocationExpression(g.MemberAccessExpression(g.IdentifierName(nameof(DateTimeOffset)), nameof(DateTimeOffset.Parse)), text);
            }
        }

        private sealed class _TimeSpanGenerator : ColumnValueGenerator<TimeSpan?>
        {
            protected override SyntaxNode GenerateValue(TimeSpan? value)
            {
                var g = Generator;
                if (!value.HasValue)
                    return g.NullLiteralExpression();

                AddReferencedType(typeof(TimeSpan));
                var text = g.LiteralExpression(Column.Serialize(RowOrdinal).Text);
                return g.InvocationExpression(g.MemberAccessExpression(g.IdentifierName(nameof(TimeSpan)), nameof(TimeSpan.Parse)), text);
            }
        }

        private sealed class _BooleanGenerator : ColumnValueGenerator<bool?>
        {
            protected override SyntaxNode GenerateValue(bool? value)
            {
                var g = Generator;
                return value.HasValue ? (value.Value ? g.TrueLiteralExpression() : g.FalseLiteralExpression()) : g.NullLiteralExpression();
            }
        }

        private sealed class _ByteGenerator : ColumnValueGenerator<byte?>
        {
            protected override SyntaxNode GenerateValue(byte? value)
            {
                var g = Generator;
                return value.HasValue ? g.LiteralExpression(value.Value) : g.NullLiteralExpression();
            }
        }

        private sealed class _CharGenerator : ColumnValueGenerator<char?>
        {
            protected override SyntaxNode GenerateValue(char? value)
            {
                var g = Generator;
                return value.HasValue ? g.LiteralExpression(value.Value) : g.NullLiteralExpression();
            }
        }

        private sealed class _DecimalGenerator : ColumnValueGenerator<decimal?>
        {
            protected override SyntaxNode GenerateValue(decimal? value)
            {
                var g = Generator;
                return value.HasValue ? g.LiteralExpression(value.Value) : g.NullLiteralExpression();
            }
        }

        private sealed class _DoubleGenerator : ColumnValueGenerator<double?>
        {
            protected override SyntaxNode GenerateValue(double? value)
            {
                var g = Generator;
                return value.HasValue ? g.LiteralExpression(value.Value) : g.NullLiteralExpression();
            }
        }

        private sealed class _Int16Generator : ColumnValueGenerator<Int16?>
        {
            protected override SyntaxNode GenerateValue(Int16? value)
            {
                var g = Generator;
                return value.HasValue ? g.LiteralExpression(value.Value) : g.NullLiteralExpression();
            }
        }

        private sealed class _Int32Generator : ColumnValueGenerator<Int32?>
        {
            protected override SyntaxNode GenerateValue(int? value)
            {
                var g = Generator;
                return value.HasValue ? g.LiteralExpression(value.Value) : g.NullLiteralExpression();
            }
        }

        private sealed class _Int64Generator : ColumnValueGenerator<Int64?>
        {
            protected override SyntaxNode GenerateValue(Int64? value)
            {
                var g = Generator;
                return value.HasValue ? g.LiteralExpression(value.Value) : g.NullLiteralExpression();
            }
        }

        private sealed class _SingleGenerator : ColumnValueGenerator<Single?>
        {
            protected override SyntaxNode GenerateValue(Single? value)
            {
                var g = Generator;
                return value.HasValue ? g.LiteralExpression(value.Value) : g.NullLiteralExpression();
            }
        }

        private sealed class _StringGenerator : ColumnValueGenerator<String>
        {
            protected override SyntaxNode GenerateValue(String value)
            {
                var g = Generator;
                return value != null ? g.LiteralExpression(value) : g.NullLiteralExpression();
            }
        }

        static ColumnValueGenerator()
        {
            Register<_BinaryGenerator>();
            Register<_GuidGenerator>();
            Register<_DateTimeGenerator>();
            Register<_DateTimeOffsetGenerator>();
            Register<_TimeSpanGenerator>();
            Register<_BooleanGenerator>();
            Register<_ByteGenerator>();
            Register<_CharGenerator>();
            Register<_DecimalGenerator>();
            Register<_DoubleGenerator>();
            Register<_Int16Generator>();
            Register<_Int32Generator>();
            Register<_Int64Generator>();
            Register<_SingleGenerator>();
            Register<_StringGenerator>();
        }

        private static Dictionary<Type, Func<ColumnValueGenerator>> s_factories = new Dictionary<Type, Func<ColumnValueGenerator>>();

        /// <summary>
        /// Registers the column value generator by column data type.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="ColumnValueGenerator"/>.</typeparam>
        public static void Register<T>()
            where T : ColumnValueGenerator, new()
        {
            var dataType = GetDataType(typeof(T));
            if (dataType != null)
                s_factories[dataType] = () => new T();
        }

        private static Type GetDataType(Type generatorType)
        {
            if (generatorType == null)
                return null;
            if (generatorType.IsGenericType && generatorType.GetGenericTypeDefinition() == typeof(ColumnValueGenerator<>))
                return generatorType.GenericTypeArguments[0];
            return GetDataType(generatorType.BaseType);
        }

        internal static ColumnValueGenerator Get(Column column)
        {
            var enumResult = GetEnum(column);
            if (enumResult != null)
                return enumResult;

            var dataType = column.DataType;
            return s_factories.TryGetValue(dataType, out var result) ? result() : new FallbackColumnValueGenerator();
        }

        private static ColumnValueGenerator GetEnum(Column column)
        {
            var enumType = GetEnumType(column.GetType());
            return enumType == null ? null : (ColumnValueGenerator)Activator.CreateInstance(typeof(EnumColumnValueGenerator<>).MakeGenericType(enumType));
        }

        private static Type GetEnumType(Type columnType)
        {
            if (columnType == null)
                return null;

            if (columnType.IsGenericType && columnType.GetGenericTypeDefinition() == typeof(EnumColumn<>))
                return columnType.GenericTypeArguments[0];
            return GetEnumType(columnType.BaseType);
        }

        internal void Initialize(DataSetGenerator dataSetGenerator, Column column)
        {
            _dataSetGenerator = dataSetGenerator;
            Initialize(column);
        }

        internal abstract void Initialize(Column column);

        internal abstract Column GetColumn();

        private DataSetGenerator _dataSetGenerator;

        /// <summary>
        /// Gets the language of source code.
        /// </summary>
        protected string Language
        {
            get { return _dataSetGenerator.Language; }
        }

        /// <summary>
        /// Gets the syntax generator to generate roslyn <see cref="SyntaxNode"/>.
        /// </summary>
        protected SyntaxGenerator Generator
        {
            get { return _dataSetGenerator.Generator; }
        }

        /// <summary>
        /// Adds referenced type.
        /// </summary>
        /// <param name="type"></param>
        protected void AddReferencedType(Type type)
        {
            _dataSetGenerator.AddReferencedType(type);
        }

        /// <summary>
        /// Generates for specified <see cref="DataRow"/>.
        /// </summary>
        /// <param name="ordinal">The ordinal of the <see cref="DataRow"/>.</param>
        /// <returns></returns>
        protected internal abstract SyntaxNode Generate(int ordinal);

        private string ModelVariableName
        {
            get { return _dataSetGenerator.ModelVariableName; }
        }

        /// <summary>
        /// Generates the column expression.
        /// </summary>
        /// <returns>The generated <see cref="SyntaxNode"/>.</returns>
        protected SyntaxNode GenerateColumn()
        {
            var g = Generator;
            return g.MemberAccessExpression(g.IdentifierName(ModelVariableName), GetColumn().Name);
        }

        /// <summary>
        /// Generates the column indexer expression.
        /// </summary>
        /// <param name="ordinal">The ordinal as column indexer.</param>
        /// <returns>The generated <see cref="SyntaxNode"/>.</returns>
        protected SyntaxNode GenerateColumn(int ordinal)
        {
            var g = Generator;
            return g.ElementAccessExpression(GenerateColumn(), g.LiteralExpression(ordinal));
        }
    }

    /// <summary>
    /// Generates source code for column value assignment by column data type.
    /// </summary>
    /// <typeparam name="T">The column data type.</typeparam>
    public abstract class ColumnValueGenerator<T> : ColumnValueGenerator
    {
        /// <summary>
        /// Gets the current <see cref="Column"/> to generate.
        /// </summary>
        protected Column<T> Column { get; private set; }

        internal sealed override Column GetColumn()
        {
            return Column;
        }

        internal sealed override void Initialize(Column column)
        {
            Column = (Column<T>)column;
        }

        /// <summary>
        /// Gets the current data row ordinal.
        /// </summary>
        protected int RowOrdinal { get; private set; } = -1;

        /// <inheritdoc />
        protected internal override SyntaxNode Generate(int ordinal)
        {
            var g = Generator;
            var left = GenerateColumn(ordinal);
            RowOrdinal = ordinal;
            var right = GenerateValue(Column[ordinal]);
            RowOrdinal = -1;
            if (Language == LanguageNames.CSharp)
                return left.CsSimpleAssignment(right);
            else if (Language == LanguageNames.VisualBasic)
                return left.VbSimpleAssignment(right);
            else
                throw new NotSupportedException(string.Format("Language {0} is not supported.", Language));
        }

        /// <summary>
        /// Generates the value constant or expression.
        /// </summary>
        /// <param name="value">The value to generate.</param>
        /// <returns>The generated <see cref="SyntaxNode"/>.</returns>
        protected abstract SyntaxNode GenerateValue(T value);
    }
}
