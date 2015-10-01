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

        public IndentedStringBuilder()
            : this(DefaultTabString)
        {
        }

        public IndentedStringBuilder(string tabString)
        {
            _tabString = tabString;
            _indentLevel = 0;
            _tabsPending = false;
        }

        public StringBuilder InnerBuilder
        {
            get { return _innerBuilder; }
        }

        public int Indent
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

        public IndentedStringBuilder Append(bool value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        public IndentedStringBuilder Append(byte value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        public IndentedStringBuilder Append(char value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        public IndentedStringBuilder Append(char[] value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        public IndentedStringBuilder Append(decimal value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        public IndentedStringBuilder Append(double value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        public IndentedStringBuilder Append(int value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        public IndentedStringBuilder Append(Int16 value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        public IndentedStringBuilder Append(Int64 value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        public IndentedStringBuilder Append(object value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        public IndentedStringBuilder Append(SByte value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        public IndentedStringBuilder Append(Single value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        public IndentedStringBuilder Append(String value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        public IndentedStringBuilder Append(UInt16 value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        public IndentedStringBuilder Append(UInt32 value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        public IndentedStringBuilder Append(UInt64 value)
        {
            OutputTabs();
            _innerBuilder.Append(value);
            return this;
        }

        public IndentedStringBuilder Append(char value, int repeatCount)
        {
            OutputTabs();
            _innerBuilder.Append(value, repeatCount);
            return this;
        }

        public IndentedStringBuilder Append(char[] value, int startIndex, int charCount)
        {
            OutputTabs();
            _innerBuilder.Append(value, startIndex, charCount);
            return this;
        }

        public IndentedStringBuilder Append(string value, int startIndex, int count)
        {
            OutputTabs();
            _innerBuilder.Append(value, startIndex, count);
            return this;
        }

        public IndentedStringBuilder AppendLine()
        {
            _innerBuilder.AppendLine();
            _tabsPending = true;
            return this;
        }

        public IndentedStringBuilder AppendLine(string value)
        {
            OutputTabs();
            _innerBuilder.AppendLine(value);
            _tabsPending = true;
            return this;
        }

        public IndentedStringBuilder AppendFormat(string format, object arg0)
        {
            OutputTabs();
            _innerBuilder.AppendFormat(format, arg0);
            return this;
        }

        public IndentedStringBuilder AppendFormat(string format, params object[] args)
        {
            OutputTabs();
            _innerBuilder.AppendFormat(format, args);
            return this;
        }

        public IndentedStringBuilder AppendFormat(IFormatProvider provider, string format, params object[] args)
        {
            OutputTabs();
            _innerBuilder.AppendFormat(provider, format, args);
            return this;
        }

        public IndentedStringBuilder AppendFormat(string format, object arg0, object arg1)
        {
            OutputTabs();
            _innerBuilder.AppendFormat(format, arg0, arg1);
            return this;
        }

        public IndentedStringBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
        {
            OutputTabs();
            _innerBuilder.AppendFormat(format, arg0, arg1, arg2);
            return this;
        }

        public override string ToString()
        {
            return _innerBuilder.ToString();
        }

        public string ToString(int startIndex, int length)
        {
            return _innerBuilder.ToString(startIndex, length);
        }
    }
}
