using DevZest.Data.Utilities;

namespace DevZest.Data
{
    public abstract class DataValidation
    {
        protected DataValidation(string name)
        {
            Check.NotEmpty(name, nameof(name));
            Name = name;
        }

        public string Name { get; private set; }

        public abstract int ColumnCount { get; }

        public abstract Column this[int index] { get; }

        public abstract string Validate(DataRow dataRow);
    }
}
