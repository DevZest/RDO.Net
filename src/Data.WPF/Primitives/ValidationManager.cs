using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace DevZest.Data.Windows.Primitives
{
    internal abstract class ValidationManager : ElementManager
    {
        protected ValidationManager(Template template, DataSet dataSet, _Boolean where, ColumnSort[] orderBy, bool emptyBlockViewList)
            : base(template, dataSet, where, orderBy, emptyBlockViewList)
        {
        }

        private Dictionary<Input, ValidationMessage> _inputErrors = new Dictionary<Input, ValidationMessage>();
        private List<ValidationMessage> _validationMessages = new List<ValidationMessage>();
        private ValidationResult _mergedValidationResult;

        internal bool HasErrors(Input input)
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<ValidationMessage> GetValidationMessages(Input input)
        {
            throw new NotImplementedException();
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
