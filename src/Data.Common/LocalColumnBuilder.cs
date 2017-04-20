using System;
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

        public string DisplayName
        {
            get { return Column.DisplayName; }
            set { Column.DisplayName = value; }
        }

        public void SetDisplayName(Func<string> displayNameGetter)
        {
            Column.SetDisplayName(displayNameGetter);
        }

        public string DisplayDescription
        {
            get { return Column.DisplayDescription; }
            set { Column.DisplayDescription = value; }
        }

        public void SetDisplayDescription(Func<string> displayDescriptionGetter)
        {
            Column.SetDisplayDescription(displayDescriptionGetter);
        }

        public string DisplayShortName
        {
            get { return Column.DisplayShortName; }
            set { Column.DisplayShortName = value; }
        }

        public void SetDisplayShortName(Func<string> displayShortNameGetter)
        {
            Column.SetDisplayShortName(displayShortNameGetter);
        }

        public string DisplayPrompt
        {
            get { return Column.DisplayPrompt; }
            set { Column.DisplayPrompt = value; }
        }

        public void SetDisplayPrompt(Func<string> displayPromptGetter)
        {
            Column.SetDisplayPrompt(displayPromptGetter);
        }

        public void DefaultValue(T defaultValue)
        {
            Column.DefaultValue(defaultValue);
        }

        public bool? IsConcrete
        {
            get { return Column.IsConcrete; }
            set { Column.SetIsConcrete(value); }
        }
    }
}
