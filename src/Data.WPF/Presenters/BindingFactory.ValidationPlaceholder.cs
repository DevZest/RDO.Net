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
        public static RowBinding<ValidationPlaceholder> BindToValidationPlaceholder(this IColumns source, params RowBinding[] containingBindings)
        {
            Check.NotNull(source, nameof(source));

            source = source.Seal();

            var result = new RowBinding<ValidationPlaceholder>(onSetup: (v, p) =>
            {
                if (containingBindings != null && containingBindings.Length > 0)
                {
                    var containingElements = new UIElement[containingBindings.Length];
                    for (int i = 0; i < containingElements.Length; i++)
                        containingElements[i] = containingBindings[i].GetSettingUpElement();
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

        public static RowBinding<ValidationPlaceholder> BindToValidationPlaceholder(this IReadOnlyList<RowBinding> source)
        {
            Check.NotNull(source, nameof(source));

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

        public static ScalarBinding<ValidationPlaceholder> BindToValidationPlaceholder(this IReadOnlyList<ScalarBinding> source)
        {
            Check.NotNull(source, nameof(source));

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
