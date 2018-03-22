using DevZest.Data.Presenters.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;

namespace DevZest.Data.Presenters
{
    public abstract class RowBinding : TwoWayBinding, IConcatList<RowBinding>
    {
        #region IConcatList<RowBinding>

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<RowBinding>.Count
        {
            get { return 1; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        RowBinding IReadOnlyList<RowBinding>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IConcatList<RowBinding>.Sort(Comparison<RowBinding> comparision)
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<RowBinding> IEnumerable<RowBinding>.GetEnumerator()
        {
            yield return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        #endregion

        public RowBinding Parent { get; private set; }

        public abstract IReadOnlyList<RowBinding> ChildBindings { get; }

        public abstract Input<RowBinding, IColumns> RowInput { get; }

        public sealed override Binding ParentBinding
        {
            get { return Parent; }
        }

        private IReadOnlyList<Column> _serializableColumns;
        public IReadOnlyList<Column> SerializableColumns
        {
            get
            {
                if (_serializableColumns == null)
                {
                    var rowInputTarget = RowInput?.Target;
                    _serializableColumns = GetSerializableColumns().ToArray();
                }
                return _serializableColumns;
            }
            set
            {
                VerifyNotSealed();
                _serializableColumns = value;
            }
        }

        private IEnumerable<Column> GetSerializableColumns()
        {
            var rowInputTarget = RowInput?.Target;
            if (rowInputTarget != null)
            {
                foreach (var column in rowInputTarget)
                {
                    if (column != null)
                        yield return column;
                }
            }

            foreach (var childBinding in ChildBindings)
            {
                foreach (var column in childBinding.GetSerializableColumns())
                    yield return column;
            }
        }

        internal void Seal(RowBinding parent, int ordinal)
        {
            VerifyNotSealed();
            Parent = parent;
            Ordinal = ordinal;
        }

        internal abstract UIElement Setup(RowPresenter rowPresenter);

        internal sealed override void VerifyRowRange(GridRange rowRange)
        {
            if (!rowRange.Contains(GridRange))
                throw new InvalidOperationException(DiagnosticMessages.RowBinding_OutOfRowRange(Ordinal));
        }

        internal abstract UIElement GetChild(UIElement parent, int index);

        public UIElement this[RowPresenter rowPresenter]
        {
            get
            {
                if (Ordinal == -1)
                    return null;

                if (Parent != null)
                    return Parent.GetChild(Parent[rowPresenter], Ordinal);

                if (rowPresenter == null || rowPresenter.Template != Template)
                    return null;

                var rowView = rowPresenter.View;
                if (rowView == null)
                    return null;
                var elements = rowView.Elements;
                return elements == null ? null : elements[Ordinal];
            }
        }

        internal abstract void BeginSetup(UIElement value);

        public bool IsEditable
        {
            get { return RowInput != null || ChildBindings.Any(x =>  x.IsEditable); }
        }
    }
}
