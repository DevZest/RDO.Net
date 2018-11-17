using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class BaseTemplateBuilder<T> : IDisposable
        where T : BaseTemplateBuilder<T>
    {
        internal BaseTemplateBuilder(Template template)
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

        public T AddAsyncValidator<TElement>(ScalarInput<TElement> input, Func<Task<string>> validator, string displayName)
            where TElement : UIElement, new()
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return AddAsyncValidator(input.Target, validator, displayName);
        }

        public T AddAsyncValidator<TElement>(ScalarInput<TElement> input, Func<Task<IEnumerable<string>>> validator, string displayName)
            where TElement : UIElement, new()
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return AddAsyncValidator(input.Target, validator, displayName);
        }

        [DefaultValue(ValidationMode.Progressive)]
        public virtual T WithScalarValidationMode(ValidationMode value)
        {
            Template.ScalarValidationMode = value;
            return (T)this;
        }

        public T AddBinding<TElement>(TElement element, ScalarBinding<TElement> scalarBinding)
            where TElement : UIElement, new()
        {
            Template.AddBinding(element, scalarBinding);
            return (T)this;
        }

    }
}
