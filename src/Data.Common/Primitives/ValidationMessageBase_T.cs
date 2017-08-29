using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    public abstract class ValidationMessageBase<T> : ValidationMessageBase
    {
        protected ValidationMessageBase(string id, ValidationSeverity severity, string description, T source)
            : base(id, severity, description)
        {
            _source = source;
        }

        private readonly T _source;
        public T Source
        {
            get { return _source; }
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
