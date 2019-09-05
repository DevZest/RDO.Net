using System;
using System.Collections.Generic;
using System.Text;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Wrapper of StringBuilder to provide text indention.
    /// </summary>
    public sealed class IndentedStringBuilder
    {
        const string DefaultTabString = "    ";

        private readonly StringBuilder _innerBuilder = new StringBuilder();
        private int _indentLevel;
        private bool _tabsPending;
        private readonly string _tabString;

        private readonly List<string> _cachedIndents = new List<string>();

        /// <summary>
        /// Initializes a new instance of <see cref="IndentedStringBuilder"/> class.
        /// </summary>
        public IndentedStringBuilder()
            : this(DefaultTabString)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="IndentedStringBuilder"/> class.
        /// </summary>
        /// <param name="tabString">The tab string value.</param>
        public IndentedStringBuilder(string tabString)
        {
            _tabString = tabString;
            _indentLevel = 0;
            _tabsPending = false;
        }

        /// <summary>
        /// Gets the inner string builder.
        /// </summary>
        public StringBuilder InnerBuilder
        {
            get { return _innerBuilder; }
        }

        /// <summary>
        /// Gets or sets the indent level.
        /// </summary>
        public int IndentLevel
        {
            get { return _indentLevel; }
            set
            {
                if (value < 0)
                    value = 0;
                _indentLevel = value;
            }
        }

        private string GetCurrentIndentation()
        {
            if (_indentLevel <= 0 || String.IsNullOrEmpty(_tabString))
                return String.Empty;

            if (_indentLevel == 1)
                return _tabString;

            // Since _indentLevel is known > 2, we can safely subtract two to index the list
            var cacheIndex = _indentLevel - 2;
            var cached = cacheIndex < _cachedIndents.Count ? _cachedIndents[cacheIndex] : null;

            if (cached == null)
            {
                cached = _cachedIndents.Count == 0 ? _tabString : _cachedIndents[_cachedIndents.Count - 1];
                for (var i = _cachedIndents.Count; i <= cacheIndex; i++)
                {
                    cached = cached + _tabString;
                    _cachedIndents.Add(cached);
                }
            }

            return cached;
        }

        private void OutputTabs()
        {
            if (!_tabsPending)
                return;

            _innerBuilder.Append(GetCurrentIndentation());
            _tabsPending = false;
        }

        /// <summary>
        /// Appends Boolean value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(bool value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends Byte value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(byte value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends Char value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(char value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends array of Char value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(char[] value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends Decimal value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(decimal value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends Double value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(double value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends Int32 value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(int value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends Int16 value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(Int16 value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends Int64 value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(Int64 value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends Object value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(object value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends SByte value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(SByte value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends Single value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(Single value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends String value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(String value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends UInt16 value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(UInt16 value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends Unit32 value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(UInt32 value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends UInt64 value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(UInt64 value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends repeated Char value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="repeatCount">Count of chars to prepeat.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(char value, int repeatCount)
        {
            OutputTabs();
            _innerBuilder.Append(value, repeatCount);
            return this;
        }

        /// <summary>
        /// Appends part of Char array value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="startIndex">The start index in char array.</param>
        /// <param name="charCount">Count of chars.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(char[] value, int startIndex, int charCount)
        {
            OutputTabs();
            _innerBuilder.Append(value, startIndex, charCount);
            return this;
        }

        /// <summary>
        /// Appends part of string value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="startIndex">The start index in the string.</param>
        /// <param name="count">Count of chars.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder Append(string value, int startIndex, int count)
        {
            OutputTabs();
            _innerBuilder.Append(value, startIndex, count);
            return this;
        }

        /// <summary>
        /// Appends an empty line.
        /// </summary>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder AppendLine()
        {
            _innerBuilder.AppendLine();
            _tabsPending = true;
            return this;
        }

        /// <summary>
        /// Appends String value followed by an empty line.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder AppendLine(string value)
        {
            OutputTabs();
            _innerBuilder.AppendLine(value);
            _tabsPending = true;
            return this;
        }

        /// <summary>
        /// Appends formatted string.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="arg0">The object to format.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder AppendFormat(string format, object arg0)
        {
            OutputTabs();
            _innerBuilder.AppendFormat(format, arg0);
            return this;
        }

        /// <summary>
        /// Appends formatted string.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The objects to format.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder AppendFormat(string format, params object[] args)
        {
            OutputTabs();
            _innerBuilder.AppendFormat(format, args);
            return this;
        }

        /// <summary>
        /// Appends formatted string.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="format">The format string.</param>
        /// <param name="args">The objects to format.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder AppendFormat(IFormatProvider provider, string format, params object[] args)
        {
            OutputTabs();
            _innerBuilder.AppendFormat(provider, format, args);
            return this;
        }

        /// <summary>
        /// Appends formatted string.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder AppendFormat(string format, object arg0, object arg1)
        {
            OutputTabs();
            _innerBuilder.AppendFormat(format, arg0, arg1);
            return this;
        }

        /// <summary>
        /// Appends formatted string.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <param name="arg2">The third object to format.</param>
        /// <returns>This <see cref="IndentedStringBuilder"/> for fluent coding.</returns>
        public IndentedStringBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
        {
            OutputTabs();
            _innerBuilder.AppendFormat(format, arg0, arg1, arg2);
            return this;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _innerBuilder.ToString();
        }

        /// <summary>
        /// Converts the value of a substring of this instance to a <see cref="String"/>.
        /// </summary>
        /// <param name="startIndex">The starting position of the substring in this instance.</param>
        /// <param name="length">The length of the substring.</param>
        /// <returns>A string whose value is the same as the specified substring of this instance.</returns>
        public string ToString(int startIndex, int length)
        {
            return _innerBuilder.ToString(startIndex, length);
        }
    }
}
