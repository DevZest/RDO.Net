using System;
using System.Collections;
using System.Collections.Generic;
using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    public abstract class ValidationMessage<T> : AbstractValidationMessage
    {
        protected ValidationMessage(string id, ValidationSeverity severity, string description, T source)
        {
            Check.NotEmpty(description, nameof(description));

            _id = id;
            _description = description;
            _severity = severity;
            Source = source;
        }

        private string _id;
        public sealed override string Id
        {
            get { return _id; }
        }

        private ValidationSeverity _severity;
        public sealed override ValidationSeverity Severity
        {
            get { return _severity; }
        }

        private string _description;
        public sealed override string Description
        {
            get { return _description; }
        }

        public T Source { get; private set; }
    }
}
