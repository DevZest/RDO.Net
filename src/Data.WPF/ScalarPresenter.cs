namespace DevZest.Data.Windows
{
    public sealed class ScalarPresenter
    {
        internal ScalarPresenter()
        {
        }

        public int FlowIndex { get; private set; } = -1;

        public ViewInputError InputError { get; private set; }

        public ViewInputError ValueError { get; private set; }

        internal void SetFlowIndex(int flowIndex)
        {
            FlowIndex = flowIndex;
            InputError = null;
            ValueError = null;
        }

        internal void SetErrors(ViewInputError inputError, ViewInputError valueError)
        {
            InputError = inputError;
            ValueError = valueError;
        }
    }
}
