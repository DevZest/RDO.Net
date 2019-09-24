using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Base class to support building template.
    /// </summary>
    /// <typeparam name="T">Type of the template builder.</typeparam>
    public abstract class TemplateBuilder<T> : IDisposable
        where T : TemplateBuilder<T>
    {
        internal TemplateBuilder(Template template)
        {
            Debug.Assert(template != null);
            Template = template;
        }

        internal Template Template { get; private set; }

        internal void Seal()
        {
            Template.Seal();
        }

        /// <summary>
        /// Disposes this object.
        /// </summary>
        public void Dispose()
        {
            Template = null;
        }

        /// <summary>
        /// Adds async scalar validator.
        /// </summary>
        /// <param name="sourceScalars">The source scalar data.</param>
        /// <param name="validator">Delegate to perform validation.</param>
        /// <param name="displayName">The display name used for fault error message.</param>
        /// <returns>This template builder for fluent coding.</returns>
        public T AddAsyncValidator(IScalars sourceScalars, Func<Task<string>> validator, string displayName)
        {
            if (sourceScalars == null || sourceScalars.Count == 0)
                throw new ArgumentNullException(nameof(sourceScalars));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            Template.AddAsyncValidator(ScalarAsyncValidator.Create(displayName, sourceScalars, validator));
            return (T)this;
        }

        /// <summary>
        /// Adds async scalar validator.
        /// </summary>
        /// <param name="sourceScalars">The source scalar data.</param>
        /// <param name="validator">Delegate to perform validation.</param>
        /// <param name="displayName">The display name used for fault error message.</param>
        /// <returns>This template builder for fluent coding.</returns>
        public T AddAsyncValidator(IScalars sourceScalars, Func<Task<IEnumerable<string>>> validator, string displayName)
        {
            if (sourceScalars == null || sourceScalars.Count == 0)
                throw new ArgumentNullException(nameof(sourceScalars));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            Template.AddAsyncValidator(ScalarAsyncValidator.Create(displayName, sourceScalars, validator));
            return (T)this;
        }

        /// <summary>
        /// Adds async scalar validator.
        /// </summary>
        /// <param name="input">The scalar input.</param>
        /// <param name="validator">Delegate to perform validation.</param>
        /// <param name="displayName">The display name used for fault error message.</param>
        /// <returns>This template builder for fluent coding.</returns>
        public T AddAsyncValidator<TElement>(ScalarInput<TElement> input, Func<Task<string>> validator, string displayName)
            where TElement : UIElement, new()
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return AddAsyncValidator(input.Target, validator, displayName);
        }

        /// <summary>
        /// Adds async scalar validator.
        /// </summary>
        /// <param name="input">The scalar input.</param>
        /// <param name="validator">Delegate to perform validation.</param>
        /// <param name="displayName">The display name used for fault error message.</param>
        /// <returns>This template builder for fluent coding.</returns>
        public T AddAsyncValidator<TElement>(ScalarInput<TElement> input, Func<Task<IEnumerable<string>>> validator, string displayName)
            where TElement : UIElement, new()
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return AddAsyncValidator(input.Target, validator, displayName);
        }

        /// <summary>
        /// Sets scalar validation mode.
        /// </summary>
        /// <param name="value">The scalar validtion mode value.</param>
        /// <returns>This template builder for fluent coding.</returns>
        [DefaultValue(ValidationMode.Progressive)]
        public virtual T WithScalarValidationMode(ValidationMode value)
        {
            Template.ScalarValidationMode = value;
            return (T)this;
        }

        /// <summary>
        /// Adds scalar binding.
        /// </summary>
        /// <typeparam name="TElement">Type of view element.</typeparam>
        /// <param name="element">The view element.</param>
        /// <param name="scalarBinding">The scalar binding.</param>
        /// <returns>This template builder for fluent coding.</returns>
        public T AddBinding<TElement>(TElement element, ScalarBinding<TElement> scalarBinding)
            where TElement : UIElement, new()
        {
            Template.AddBinding(element, scalarBinding);
            return (T)this;
        }

        /// <summary>
        /// Sets the initial keyboard focus.
        /// </summary>
        /// <param name="initialFocus">The initial keyboard focus.</param>
        /// <returns>This template builder for fluent coding.</returns>
        public T WithInitialFocus(InitialFocus initialFocus)
        {
            Template.InitialFocus = initialFocus.VerifyNotNull(nameof(initialFocus));
            return (T)this;
        }
    }
}
