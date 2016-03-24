using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevZest.Data.Windows.Primitives
{
    internal static class IElementCollectionFactory
    {
        internal static IElementCollection Create(FrameworkElement parent)
        {
            if (parent == null)
                return new ElementList();
            else
                return new ChildElementCollection(parent);
        }

        private sealed class ElementList : List<UIElement>, IElementCollection
        {
            public FrameworkElement Parent
            {
                get { return null; }
            }
        }

        private sealed class ChildElementCollection : UIElementCollection, IElementCollection
        {
            public ChildElementCollection(FrameworkElement parent)
                : base(parent, parent)
            {
                Parent = parent;
            }

            public FrameworkElement Parent { get; private set; }

            public bool IsReadOnly
            {
                get { return false; }
            }

            void ICollection<UIElement>.Add(UIElement item)
            {
                base.Add(item);
            }

            IEnumerator<UIElement> IEnumerable<UIElement>.GetEnumerator()
            {
                for (int i = 0; i < Count; i++)
                    yield return this[i];
            }

            bool ICollection<UIElement>.Remove(UIElement item)
            {
                if (Contains(item))
                {
                    base.Remove(item);
                    return true;
                }

                return false;
            }
        }
    }
}
