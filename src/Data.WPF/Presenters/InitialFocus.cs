using DevZest.Data.Views;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace DevZest.Data.Presenters
{
    public abstract class InitialFocus
    {
        public static InitialFocus None
        {
            get { return MoveToNone.Singleton; }
        }

        public static InitialFocus First
        {
            get { return MoveToFirst.Singleton; }
        }

        public static InitialFocus Explicit(RowBinding binding)
        {
            Check.NotNull(binding, nameof(binding));
            return new MoveToRowBinding(binding);
        }

        public static InitialFocus Explicit(ScalarBinding binding)
        {
            Check.NotNull(binding, nameof(binding));
            return new MoveToScalarBinding(binding);
        }

        private sealed class MoveToNone : InitialFocus
        {
            public static readonly MoveToNone Singleton = new MoveToNone();

            private MoveToNone()
            {
            }

            protected override void MoveFocus(DataView dataView)
            {
            }
        }

        private sealed class MoveToFirst : InitialFocus
        {
            public static readonly MoveToFirst Singleton = new MoveToFirst();

            private MoveToFirst()
            {
            }

            protected override void MoveFocus(DataView dataView)
            {
                dataView.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
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

            protected override void MoveFocus(DataView dataView)
            {
                var dataPresenter = dataView.DataPresenter;
                var currentRow = dataPresenter.CurrentRow;
                if (currentRow != null)
                    _binding[currentRow].Focus();
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

            protected override void MoveFocus(DataView dataView)
            {
                _binding[0].Focus();
            }
        }

        internal void MoveFocus(DataPresenter dataPresenter)
        {
            var dataView = dataPresenter.View;
            if (!dataView.ContainsKeyboardFocus())
                MoveFocus(dataView);
        }

        protected abstract void MoveFocus(DataView dataView);
    }
}
