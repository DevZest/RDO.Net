using DevZest.Data.Views;
using DevZest.Data.Presenters.Primitives;
using System;
using System.Diagnostics;
using System.Windows;
using System.ComponentModel;

namespace DevZest.Data.Presenters
{
    public static class BindingManager
    {
        [DefaultValue(false)]
        public static T WithFrozenLeftShrink<T>(this T binding, bool value)
            where T : Binding
        {
            binding.VerifyNotSealed();
            binding.FrozenLeftShrink = value;
            return binding;
        }

        [DefaultValue(false)]
        public static T WithFrozenTopShrink<T>(this T binding, bool value)
            where T : Binding
        {
            binding.VerifyNotSealed();
            binding.FrozenTopShrink = value;
            return binding;
        }

        [DefaultValue(false)]
        public static T WithFrozenRightShrink<T>(this T binding, bool value)
            where T : Binding
        {
            binding.VerifyNotSealed();
            binding.FrozenRightShrink = value;
            return binding;
        }

        [DefaultValue(false)]
        public static T WithFrozenBottomShrink<T>(this T binding, bool value)
            where T : Binding
        {
            binding.VerifyNotSealed();
            binding.FrozenBottomShrink = value;
            return binding;
        }

        public static T WithStyle<T>(this T binding, Style value)
            where T : Binding
        {
            binding.VerifyNotSealed();
            binding.Style = value;
            return binding;
        }

        public static T WithStyle<T>(this T binding, StyleId value)
            where T : Binding
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            return binding.WithStyle<T>(value.GetOrLoad());
        }

        public static T WithAutoSizeOrder<T>(this T binding, int value)
            where T : Binding
        {
            binding.VerifyNotSealed();
            binding.AutoSizeOrder = value;
            return binding;
        }

        public static T WithAutoSizeWaiver<T>(this T binding, AutoSizeWaiver value)
            where T : Binding
        {
            binding.VerifyNotSealed();
            binding.AutoSizeWaiver = value;
            return binding;
        }

        [DefaultValue(false)]
        public static T WithFlowRepeatable<T>(this T scalarBinding, bool value)
            where T : ScalarBinding
        {
            scalarBinding.VerifyNotSealed();
            scalarBinding.FlowRepeatable = value;
            return scalarBinding;
        }

        private static readonly DependencyProperty BindingProperty = DependencyProperty.RegisterAttached(nameof(Binding),
            typeof(Binding), typeof(BindingManager), new PropertyMetadata(null));

        public static Binding GetBinding(this UIElement element)
        {
            return (Binding)element.GetValue(BindingProperty);
        }

        internal static void SetBinding(this UIElement element, Binding value)
        {
            Debug.Assert(value != null && element.GetBinding() == null);
            element.SetValue(BindingProperty, value);
        }

        private static readonly DependencyProperty ScalarFlowIndexProperty = DependencyProperty.RegisterAttached("ScalarFlowIndex",
            typeof(int), typeof(BindingManager), new PropertyMetadata(0));

        internal static int GetScalarFlowIndex(this UIElement element)
        {
            return (int)element.GetValue(ScalarFlowIndexProperty);
        }

        internal static void SetScalarFlowIndex(this UIElement element, int value)
        {
            if (value == 0)
                element.ClearValue(ScalarFlowIndexProperty);
            else
                element.SetValue(ScalarFlowIndexProperty, value);
        }


        private static readonly DependencyProperty BlockViewProperty = DependencyProperty.RegisterAttached(nameof(BlockView),
            typeof(BlockView), typeof(BindingManager), new PropertyMetadata(null));

        public static BlockView GetBlockView(this UIElement element)
        {
            return (BlockView)element.GetValue(BlockViewProperty);
        }

        internal static void SetBlockView(this UIElement element, BlockView value)
        {
            if (value == null)
                element.ClearValue(BlockViewProperty);
            else
                element.SetValue(BlockViewProperty, value);
        }

        private static readonly DependencyProperty RowPresenterProperty = DependencyProperty.RegisterAttached(nameof(RowPresenter),
            typeof(RowPresenter), typeof(BindingManager), new PropertyMetadata(null));

        public static RowPresenter GetRowPresenter(this UIElement element)
        {
            return (RowPresenter)element.GetValue(RowPresenterProperty);
        }

        internal static void SetRowPresenter(this UIElement element, RowBinding rowBinding, RowPresenter value)
        {
            element.SetRowPresenter(value);
            var childBindings = rowBinding.ChildBindings;
            for (int i = 0; i < childBindings.Count; i++)
            {
                var childBinding = childBindings[i];
                var childElement = rowBinding.GetChild(element, i);
                childElement.SetRowPresenter(childBinding, value);
            }
        }

        private static void SetRowPresenter(this UIElement element, RowPresenter value)
        {
            if (value == null)
                element.ClearValue(RowPresenterProperty);
            else
                element.SetValue(RowPresenterProperty, value);
        }

        internal static Action<TElement, TPresenter> Override<TElement, TPresenter>(this Action<TElement, TPresenter> action, Action<TElement, TPresenter> overrideAction)
        {
            if (action == null)
                action = (e, sp) => { };
            return (e, sp) =>
            {
                action(e, sp);
                overrideAction(e, sp);
            };
        }

        internal static void Verify<T>(this T binding, UIElement compositeView, string paramName)
            where T : Binding
        {
            Debug.Assert(binding != null);

            if (compositeView == null)
                throw new ArgumentNullException(paramName);
            if (compositeView.GetBinding() != binding)
                throw new ArgumentException(DiagnosticMessages.Binding_InvalidCompositeView, paramName);
        }

        public static RowBinding<T> AddBehavior<T>(this RowBinding<T> rowBinding, IRowBindingBehavior<T> behavior)
            where T : UIElement, new()
        {
            if (behavior == null)
                throw new ArgumentNullException(nameof(behavior));
            rowBinding.InternalAddBehavior(behavior);
            return rowBinding;
        }

        public static ScalarBinding<T> AddBehavior<T>(this ScalarBinding<T> scalarBinding, IScalarBindingBehavior<T> behavior)
            where T : UIElement, new()
        {
            if (behavior == null)
                throw new ArgumentNullException(nameof(behavior));
            scalarBinding.InternalAddBehavior(behavior);
            return scalarBinding;
        }

        public static BlockBinding<T> AddBehavior<T>(this BlockBinding<T> blockBinding, IBlockBindingBehavior<T> behavior)
            where T : UIElement, new()
        {
            if (behavior == null)
                throw new ArgumentNullException(nameof(behavior));
            blockBinding.InternalAddBehavior(behavior);
            return blockBinding;
        }

        public static DataPresenter GetAttachedTo(this UIElement element)
        {
            Check.NotNull(element, nameof(element));
            return AttachedScalarBinding.GetAttachedScalarBinding(element)?.DataPresenter;
        }
    }
}
