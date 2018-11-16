using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class CommonTemplateBuilder<T> : IDisposable
        where T : CommonTemplateBuilder<T>
    {
        internal CommonTemplateBuilder(Template template)
        {
            Debug.Assert(template != null);
            Template = template;
        }

        internal Template Template { get; private set; }

        internal void Seal()
        {
            Template.Seal();
        }

        public void Dispose()
        {
            Template = null;
        }

        public T AddAsyncValidator(IScalars sourceScalars, Func<Task<string>> validator, string displayName)
        {
            if (sourceScalars == null || sourceScalars.Count == 0)
                throw new ArgumentNullException(nameof(sourceScalars));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            Template.AddAsyncValidator(ScalarAsyncValidator.Create(displayName, sourceScalars, validator));
            return (T)this;
        }

        public T AddAsyncValidator(IScalars sourceScalars, Func<Task<IEnumerable<string>>> validator, string displayName)
        {
            if (sourceScalars == null || sourceScalars.Count == 0)
                throw new ArgumentNullException(nameof(sourceScalars));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            Template.AddAsyncValidator(ScalarAsyncValidator.Create(displayName, sourceScalars, validator));
            return (T)this;
        }

        [DefaultValue(ValidationMode.Progressive)]
        public virtual T WithScalarValidationMode(ValidationMode value)
        {
            Template.ScalarValidationMode = value;
            return (T)this;
        }
    }
}
