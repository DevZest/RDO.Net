using DevZest.Data.Presenters.Primitives;
using DevZest.Data.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public static partial class BindingFactory
    {
        /// <summary>
        /// Binds <see cref="IColumns"/> to <see cref="ValidationPlaceholder"/>.
        /// </summary>
        /// <param name="source">The source <see cref="IColumns"/>.</param>
        /// <param name="bindings">The bindings to determine whether the <see cref="ValidationPlaceholder"/> is active.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<ValidationPlaceholder> BindToValidationPlaceholder(this IColumns source, params RowBinding[] bindings)
        {
            source.VerifyNotNull(nameof(source));

            source = source.Seal();

            var result = new RowBinding<ValidationPlaceholder>(onSetup: (v, p) =>
            {
                if (bindings != null && bindings.Length > 0)
                {
                    var containingElements = new UIElement[bindings.Length];
                    for (int i = 0; i < containingElements.Length; i++)
                        containingElements[i] = bindings[i].GetSettingUpElement();
                    v.Setup(containingElements);
                }
            }, onRefresh: null, onCleanup: (v, p) =>
            {
                v.Cleanup();
            });
            var input = result.BeginInput(new ValueChangedTrigger<ValidationPlaceholder>(source, result));
            foreach (var column in source)
                input.WithFlush(column, (r, v) => true);
            return input.EndInput();
        }

        /// <summary>
        /// Binds collection of <see cref="RowBinding"/> to <see cref="ValidationPlaceholder"/>.
        /// </summary>
        /// <param name="source">The source bindings.</param>
        /// <returns>The row binding object.</returns>
        public static RowBinding<ValidationPlaceholder> BindToValidationPlaceholder(this IReadOnlyList<RowBinding> source)
        {
            source.VerifyNotNull(nameof(source));

            var columns = GetTargetColumns(source);
            var input = new RowBinding<ValidationPlaceholder>(onSetup: (v, p) =>
            {
                var containingElements = new UIElement[source.Count];
                for (int i = 0; i < containingElements.Length; i++)
                    containingElements[i] = source[i].GetSettingUpElement();
                v.Setup(containingElements);
            }, onRefresh: null, onCleanup: (v, p) =>
            {
                v.Cleanup();
            }).BeginInput(new ExplicitTrigger<ValidationPlaceholder>());
            foreach (var column in columns)
                input.WithFlush(column, (r, v) => true);
            return input.EndInput();
        }

        private static IColumns GetTargetColumns(IReadOnlyList<RowBinding> source)
        {
            var result = Columns.Empty;
            for (int i = 0; i < source.Count; i++)
            {
                var binding = source[i];
                if (binding == null)
                    throw new ArgumentNullException(string.Format("source[{0}]", i));
                result = GetTargetColumns(result, binding);
            }
            return result;
        }

        private static IColumns GetTargetColumns(IColumns result, RowBinding binding)
        {
            Debug.Assert(binding != null);
            var input = binding.RowInput;
            if (input != null)
                return result.Union(input.Target);
            var childBindings = binding.ChildBindings;
            if (childBindings == null)
                return result;
            for (int i = 0; i < childBindings.Count; i++)
                result = GetTargetColumns(result, childBindings[i]);
            return result;
        }

        /// <summary>
        /// Binds collection of <see cref="ScalarBinding"/> to <see cref="ValidationPlaceholder"/>.
        /// </summary>
        /// <param name="source">The source bindings.</param>
        /// <returns>The scalar binding object.</returns>
        public static ScalarBinding<ValidationPlaceholder> BindToValidationPlaceholder(this IReadOnlyList<ScalarBinding> source)
        {
            source.VerifyNotNull(nameof(source));

            var scalars = GetTargetScalars(source);
            var input = new ScalarBinding<ValidationPlaceholder>(onSetup: (v, p) =>
            {
                var containingElements = new UIElement[source.Count];
                for (int i = 0; i < containingElements.Length; i++)
                    containingElements[i] = source[i].GetSettingUpElement();
                v.Setup(containingElements);
            }, onRefresh: null, onCleanup: (v, p) =>
            {
                v.Cleanup();
            }).BeginInput(new ExplicitTrigger<ValidationPlaceholder>());
            foreach (var scalar in scalars)
                input.WithFlush(scalar, v => true);
            return input.EndInput();
        }

        private static IScalars GetTargetScalars(IReadOnlyList<ScalarBinding> source)
        {
            var result = Scalars.Empty;
            for (int i = 0; i < source.Count; i++)
            {
                var binding = source[i];
                if (binding == null)
                    throw new ArgumentNullException(string.Format("source[{0}]", i));
                result = GetTargetScalars(result, binding);
            }
            return result;
        }

        private static IScalars GetTargetScalars(IScalars result, ScalarBinding binding)
        {
            Debug.Assert(binding != null);
            var input = binding.ScalarInput;
            if (input != null)
                return result.Union(input.Target);
            var childBindings = binding.ChildBindings;
            if (childBindings == null)
                return result;
            for (int i = 0; i < childBindings.Count; i++)
                result = GetTargetScalars(result, childBindings[i]);
            return result;
        }
    }
}
