using DevZest.Data.Presenters.Primitives;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public sealed class ScalarValidation
    {
        internal ScalarValidation(InputManager inputManager)
        {
            _inputManager = inputManager;
            if (Mode == ValidationMode.Progressive)
                _progress = Scalars.Empty;
        }

        private InputManager _inputManager;
        private bool _showAll;
        private IScalars _progress;

        internal void Reset()
        {
            _showAll = false;

            if (_progress != null)
                _progress = Scalars.Empty;
        }

        internal void MakeProgress<T>(ScalarInput<T> scalarInput)
            where T : UIElement, new()
        {
            var scalars = scalarInput.Target;

            if (!_showAll && _progress != null)
                _progress = _progress.Union(scalars);
        }

        internal void ShowAll()
        {
            if (_showAll)
                return;

            _showAll = true;
            if (_progress != null)
                _progress = Scalars.Empty;
        }

        public ValidationMode Mode
        {
            get { return _inputManager.ScalarValidationMode; }
        }

        public bool IsVisible(IScalars scalars)
        {
            if (scalars == null || scalars.Count == 0)
                return false;

            return _showAll || _progress == null ? true : _progress.IsSupersetOf(scalars);
        }
    }
}
