using DevZest.Data.Views;
using DevZest.Windows;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace DevZest.Data.Presenters
{
    /// <summary>
    /// Represents operation to set initial keyboard focus.
    /// </summary>
    public abstract class InitialFocus
    {
        /// <summary>
        /// Gets the operation that do not set any initial keyboard focus.
        /// </summary>
        public static InitialFocus None
        {
            get { return MoveToNone.Singleton; }
        }

        /// <summary>
        /// Gets the operation that set initial focus to the first element.
        /// </summary>
        public static InitialFocus First
        {
            get { return MoveToFirst.Singleton; }
        }

        /// <summary>
        /// Returns the operation that set initial focus to explicit row binding.
        /// </summary>
        /// <param name="binding">The row binding.</param>
        /// <returns>The result <see cref="InitialFocus"/>.</returns>
        public static InitialFocus Explicit(RowBinding binding)
        {
            binding.VerifyNotNull(nameof(binding));
            return new MoveToRowBinding(binding);
        }

        /// <summary>
        /// Returns the operation that set initial focus to explicit scalar binding.
        /// </summary>
        /// <param name="binding">The scalar binding.</param>
        /// <returns>The result <see cref="InitialFocus"/>.</returns>
        public static InitialFocus Explicit(ScalarBinding binding)
        {
            binding.VerifyNotNull(nameof(binding));
            return new MoveToScalarBinding(binding);
        }

        /// <summary>
        /// Returns the operation that set initial focus to explicit view element.
        /// </summary>
        /// <param name="element">The view element.</param>
        /// <returns>The result <see cref="InitialFocus"/>.</returns>
        public static InitialFocus Explicit(UIElement element)
        {
            element.VerifyNotNull(nameof(element));
            return new MoveToElement(element);
        }

        private sealed class MoveToNone : InitialFocus
        {
            public static readonly MoveToNone Singleton = new MoveToNone();

            private MoveToNone()
            {
            }

            protected override void MoveFocus(UIElement view)
            {
            }
        }

        private sealed class MoveToFirst : InitialFocus
        {
            public static readonly MoveToFirst Singleton = new MoveToFirst();

            private MoveToFirst()
            {
            }

            protected override void MoveFocus(UIElement view)
            {
                if (view is DataView dataView)
                {
                    var currentRow = dataView.DataPresenter.CurrentRow;
                    if (currentRow != null)
                    {
                        currentRow.View.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                        return;
                    }
                }

                view.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }
        }

        private sealed class MoveToRowBinding : InitialFocus
        {
            public MoveToRowBinding(RowBinding binding)
            {
                Debug.Assert(binding != null);
                _binding = binding;
            }

            private readonly RowBinding _binding;

            protected override void MoveFocus(UIElement view)
            {
                if (view is DataView dataView)
                {
                    var currentRow = dataView.DataPresenter.CurrentRow;
                    if (currentRow != null)
                        _binding[currentRow].Focus();
                }
            }
        }

        private sealed class MoveToScalarBinding : InitialFocus
        {
            public MoveToScalarBinding(ScalarBinding binding)
            {
                Debug.Assert(binding != null);
                _binding = binding;
            }

            private readonly ScalarBinding _binding;

            protected override void MoveFocus(UIElement view)
            {
                _binding[0]?.Focus();
            }
        }

        private sealed class MoveToElement : InitialFocus
        {
            public MoveToElement(UIElement element)
            {
                _element = element;
            }

            private readonly UIElement _element;

            protected override void MoveFocus(UIElement view)
            {
                _element.Focus();
            }
        }

        internal void MoveFocus(BasePresenter presenter)
        {
            var view = (UIElement)presenter.View;
            if (!view.ContainsKeyboardFocus())
                MoveFocus(view);
        }

        /// <summary>
        /// Moves keyboard focus to view element.
        /// </summary>
        /// <param name="view">The view element.</param>
        protected abstract void MoveFocus(UIElement view);
    }
}
