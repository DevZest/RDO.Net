using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    public sealed class LocalColumnBuilder<T> : IDisposable
    {
        internal LocalColumnBuilder(Column<T> column)
        {
            Debug.Assert(column != null);
            _column = column;
        }

        Column<T> _column;
        private Column<T> Column
        {
            get
            {
                if (_column == null)
                    throw new ObjectDisposedException(nameof(LocalColumnBuilder<T>));
                return _column;
            }
        }

        public void Dispose()
        {
            _column = null;
        }

        public string Name
        {
            get { return Column.Name; }
            set { Column.Name = value; }
        }

        public LocalColumnBuilder<T> WithName(string value)
        {
            Name = value;
            return this;
        }

        public string DisplayName
        {
            get { return Column.DisplayName; }
            set { Column.DisplayName = value; }
        }

        public LocalColumnBuilder<T> WithDisplayName(string value)
        {
            DisplayName = value;
            return this;
        }

        public void SetDisplayName(Func<string> displayNameGetter)
        {
            Column.SetDisplayName(displayNameGetter);
        }

        public LocalColumnBuilder<T> WithDisplayName(Func<string> displayNameGetter)
        {
            SetDisplayName(displayNameGetter);
            return this;
        }

        public string DisplayDescription
        {
            get { return Column.DisplayDescription; }
            set { Column.DisplayDescription = value; }
        }

        public LocalColumnBuilder<T> WithDisplayDescription(string value)
        {
            DisplayDescription = value;
            return this;
        }

        public void SetDisplayDescription(Func<string> displayDescriptionGetter)
        {
            Column.SetDisplayDescription(displayDescriptionGetter);
        }

        public LocalColumnBuilder<T> WithDisplayDescription(Func<string> displayDescriptionGetter)
        {
            SetDisplayDescription(displayDescriptionGetter);
            return this;
        }

        public string DisplayShortName
        {
            get { return Column.DisplayShortName; }
            set { Column.DisplayShortName = value; }
        }

        public LocalColumnBuilder<T> WithDisplayShortName(string value)
        {
            DisplayShortName = value;
            return this;
        }

        public void SetDisplayShortName(Func<string> displayShortNameGetter)
        {
            Column.SetDisplayShortName(displayShortNameGetter);
        }

        public LocalColumnBuilder<T> WithDisplayShortName(Func<string> displayShortNameGetter)
        {
            SetDisplayShortName(displayShortNameGetter);
            return this;
        }

        public string DisplayPrompt
        {
            get { return Column.DisplayPrompt; }
            set { Column.DisplayPrompt = value; }
        }

        public LocalColumnBuilder<T> WithDisplayPrompt(string value)
        {
            DisplayPrompt = value;
            return this;
        }

        public void SetDisplayPrompt(Func<string> displayPromptGetter)
        {
            Column.SetDisplayPrompt(displayPromptGetter);
        }

        public LocalColumnBuilder<T> WithDisplayPrompt(Func<string> displayPromptGetter)
        {
            SetDisplayPrompt(displayPromptGetter);
            return this;
        }

        public void SetDefaultValue(T defaultValue)
        {
            Column.SetDefaultValue(defaultValue, null, null);
        }

        public LocalColumnBuilder<T> WithDefaultValue(T defaultValue)
        {
            SetDefaultValue(defaultValue);
            return this;
        }

        public bool? IsConcrete
        {
            get { return Column.IsConcrete; }
            set { Column.SetIsConcrete(value); }
        }

        public LocalColumnBuilder<T> WithIsConcrete(bool? value)
        {
            IsConcrete = value;
            return this;
        }

        public IComparer<T> ValueComparer
        {
            get { return Column.ValueComparer; }
            set { Column.ValueComparer = value; }
        }

        public LocalColumnBuilder<T> WithValueComparer(IComparer<T> value)
        {
            ValueComparer = value;
            return this;
        }
    }
}
