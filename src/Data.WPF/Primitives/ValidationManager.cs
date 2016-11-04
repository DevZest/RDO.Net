using System;
using System.ComponentModel;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class ValidationManager : ElementManager
    {
        protected ValidationManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, bool emptyBlockViewList)
            : base(template, dataSet, where, orderBy, emptyBlockViewList)
        {
        }

        public bool IsValidated { get; private set; }

        private bool _isValid;
        public bool IsValid
        {
            get
            {
                if (IsValidated)
                    return _isValid;
                Validate();
                return _isValid;
            }
        }

        public bool Validate()
        {
            IsValidated = true;
            throw new NotImplementedException();
        }
    }
}
