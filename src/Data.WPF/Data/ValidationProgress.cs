using DevZest.Data;
using DevZest.Windows.Data.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace DevZest.Windows.Data
{
    public sealed class ValidationProgress
    {
        internal ValidationProgress(InputManager inputManager)
        {
            _inputManager = inputManager;
            if (Mode == ValidationMode.Progressive)
                _progress = new Dictionary<RowPresenter, IColumnSet>();
        }

        private InputManager _inputManager;
        private bool _showAll;
        private Dictionary<RowPresenter, IColumnSet> _progress;

        internal void Reset()
        {
            _showAll = false;

            if (_progress != null)
                _progress.Clear();
        }

        internal void MakeProgress<T>(RowPresenter rowPresenter, RowInput<T> rowInput)
            where T : UIElement, new()
        {
            var sourceColumns = rowInput.Columns;

            if (!_showAll && _progress != null)
            {
                var progress = GetProgress(rowPresenter).Union(rowInput.Columns);
                if (progress.Count > 0)
                    _progress[rowPresenter] = progress;
            }
        }

        internal void ShowAll()
        {
            if (_showAll)
                return;

            _showAll = true;
            if (_progress != null)
                _progress.Clear();
        }

        internal void OnCurrentRowChanged()
        {
            if (_progress != null)
            {
                if (Scope == ValidationScope.CurrentRow)
                    _progress.Clear();
            }
        }

        internal void OnRowDisposed(RowPresenter rowPresenter)
        {
            if (_progress != null && _progress.ContainsKey(rowPresenter))
                _progress.Remove(rowPresenter);
        }

        public ValidationMode Mode
        {
            get { return _inputManager.ValidationMode; }
        }

        public ValidationScope Scope
        {
            get { return _inputManager.ValidationScope; }
        }

        public bool IsVisible(RowPresenter rowPresenter, IColumnSet columns)
        {
            if (_showAll)
                return true;

            if (_progress == null)
                return false;

            if (columns.Count == 0)
                return false;
            return GetProgress(rowPresenter).IsSupersetOf(columns);
        }

        private IColumnSet GetProgress(RowPresenter rowPresenter)
        {
            Debug.Assert(_progress != null);
            IColumnSet result;
            if (_progress.TryGetValue(rowPresenter, out result))
                return result;
            return ColumnSet.Empty;
        }
    }
}
