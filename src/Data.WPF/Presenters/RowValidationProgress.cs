using DevZest.Data.Presenters.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public sealed class RowValidationProgress
    {
        internal RowValidationProgress(InputManager inputManager)
        {
            _inputManager = inputManager;
            if (Mode == ValidationMode.Progressive)
                _progress = new Dictionary<RowPresenter, IColumns>();
        }

        private InputManager _inputManager;
        private Dictionary<RowPresenter, IColumns> _progress;

        internal void Reset()
        {
            if (_progress != null)
                _progress.Clear();
        }

        internal void MakeProgress<T>(RowInput<T> rowInput)
            where T : UIElement, new()
        {
            var currentRow = _inputManager.CurrentRow;
            Debug.Assert(currentRow != null);
            var sourceColumns = rowInput.Target;

            if (_progress != null)
            {
                var columns = GetProgress(currentRow);
                if (columns == null)
                    return;
                _progress[currentRow] = columns.Union(rowInput.Target);
            }
        }

        internal void ShowAll(RowPresenter rowPresenter)
        {
            if (_progress != null)
                _progress[rowPresenter] = null;
        }

        internal void OnRowDisposed(RowPresenter rowPresenter)
        {
            if (_progress != null && _progress.ContainsKey(rowPresenter))
                _progress.Remove(rowPresenter);
        }

        public ValidationMode Mode
        {
            get { return _inputManager.RowValidationMode; }
        }

        public bool IsVisible(RowPresenter rowPresenter, IColumns columns)
        {
            Check.NotNull(rowPresenter, nameof(rowPresenter));

            if (columns == null || columns.Count == 0)
                return false;

            if (_progress == null)
                return true;

            var progress = GetProgress(rowPresenter);
            return progress == null ? true : progress.IsSupersetOf(columns);
        }

        private IColumns GetProgress(RowPresenter rowPresenter)
        {
            Debug.Assert(_progress != null);
            IColumns result;
            if (_progress.TryGetValue(rowPresenter, out result))
                return result;
            return Columns.Empty;
        }
    }
}
