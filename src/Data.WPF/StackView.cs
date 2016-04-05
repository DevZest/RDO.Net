using DevZest.Data.Windows.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Collections;

namespace DevZest.Data.Windows
{
    public class StackView : Control, IReadOnlyList<RowPresenter>
    {
        public StackView()
        {
            Index = -1;
        }

        internal void Initialize(ElementManager elementManager, int index)
        {
            ElementManager = elementManager;
            Index = index;
        }

        internal void Cleanup()
        {
            CleanupElements();
            Index = -1;
            ElementManager = null;
        }

        internal ElementManager ElementManager { get; private set; }

        public int Index { get; private set; }

        public int Count
        {
            get
            {
                if (ElementManager == null)
                    return 0;

                var stackDimensions = ElementManager.StackDimensions;
                var nextStackFirstRowOrdinal = (Index + 1) * stackDimensions;
                var rowCount = ElementManager.Rows.Count;
                return nextStackFirstRowOrdinal <= rowCount ? stackDimensions : stackDimensions - (nextStackFirstRowOrdinal - rowCount);
            }
        }

        public RowPresenter this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return ElementManager.Rows[Index * ElementManager.StackDimensions + index];
            }
        }

        public IEnumerator<RowPresenter> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private ReadOnlyCollection<StackItem> StackItems
        {
            get { return ElementManager.Template.StackItems; }
        }

        private int StackItemsSplit
        {
            get { return ElementManager.Template.StackItemsSplit; }
        }

        internal IElementCollection ElementCollection { get; private set; }

        internal IReadOnlyList<UIElement> Elements
        {
            get { return ElementCollection; }
        }

        internal void InitializeElements(FrameworkElement elementsPanel)
        {
            Debug.Assert(ElementCollection == null);

            if (ElementManager == null)
                return;

            ElementCollection = ElementCollectionFactory.Create(elementsPanel);

            var stackItems = StackItems;
            for (int i = 0; i < StackItemsSplit; i++)
                AddElement(stackItems[i]);

            for (int i = 0; i < ElementManager.StackDimensions; i++)
            {
                var success = AddElement(Index, i);
                if (!success)   // Exceeded the total count of the rows
                    break;
            }

            for (int i = StackItemsSplit; i < StackItems.Count; i++)
                AddElement(stackItems[i]);

            if (ElementManager.Template.StackViewInitializer != null)
                ElementManager.Template.StackViewInitializer(this);
        }

        private void AddElement(StackItem stackItem)
        {
            var element = stackItem.Generate();
            ElementCollection.Add(element);
            stackItem.Initialize(element);
        }

        private bool AddElement(int stackIndex, int offset)
        {
            var rows = ElementManager.Rows;
            var index = stackIndex * ElementManager.StackDimensions + offset;
            if (index >= rows.Count)
                return false;
            var row = rows[index];
            var rowView = ElementManager.Realize(row);
            ElementCollection.Add(rowView);
            return true;
        }

        private void CleanupElements()
        {
            if (ElementCollection == null)
                return;

            int stackDimensions = Elements.Count - StackItems.Count;

            var stackItems = StackItems;
            for (int i = StackItems.Count - 1; i >= StackItemsSplit; i--)
                RemoveLastElement(stackItems[i]);

            for (int i = stackDimensions - 1; i >= 0; i--)
                RemoveLastRow();

            for (int i = StackItemsSplit - 1; i >= 0 ; i--)
                RemoveLastElement(stackItems[i]);

            ElementCollection = null;
        }

        private void RemoveLastElement(StackItem stackItem)
        {
            var lastIndex = Elements.Count - 1;
            var element = Elements[lastIndex];
            stackItem.Cleanup(element);
            ElementCollection.RemoveAt(lastIndex);
        }

        private void RemoveLastRow()
        {
            var lastIndex = Elements.Count - 1;
            var rowView = (RowView)Elements[lastIndex];
            ElementManager.Virtualize(rowView.RowPresenter);
            ElementCollection.RemoveAt(lastIndex);
        }

        internal void RefreshElements()
        {
            if (Elements == null)
                return;

            var stackItems = StackItems;
            int stackDimensions = Elements.Count - stackItems.Count;
            var index = 0;

            for (int i = 0; i < StackItemsSplit; i++)
                RefreshElement(stackItems[i], index++);

            for (int i = 0; i < stackDimensions; i++)
                ((RowView)Elements[index++]).RowPresenter.RefreshElements();

            for (int i = StackItemsSplit; i < StackItems.Count; i++)
                RefreshElement(stackItems[i], index++);
        }

        private void RefreshElement(StackItem stackItem, int index)
        {
            var element = Elements[index];
            stackItem.UpdateTarget(element);
        }
    }
}
