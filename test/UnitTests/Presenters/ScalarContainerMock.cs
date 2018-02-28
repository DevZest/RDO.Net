using System;

namespace DevZest.Data.Presenters
{
    public static class ScalarContainerMock
    {
        private sealed class ScalarContainerOwner : ScalarContainer.IOwner
        {
            public ScalarContainerOwner(Action<IScalars> onValueChanged)
            {
                _onValueChanged = onValueChanged;
            }

            public void InvalidateView()
            {
            }

            private readonly Action<IScalars> _onValueChanged;
            public void OnValueChanged(IScalars scalars)
            {
                _onValueChanged?.Invoke(scalars);
            }

            public void ResumeInvalidateView()
            {
            }

            public void SuspendInvalidateView()
            {
            }
        }

        public static ScalarContainer New(Action<IScalars> onValueChanged = null)
        {
            return new ScalarContainer(new ScalarContainerOwner(onValueChanged));
        }
    }
}
