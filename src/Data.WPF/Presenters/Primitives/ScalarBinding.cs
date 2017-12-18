using DevZest.Data.Views;
using DevZest.Data.Views.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Presenters.Primitives
{
    public abstract class ScalarBinding : TwoWayBinding, IConcatList<ScalarBinding>
    {
        #region IConcatList<ScalarBinding>

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        int IReadOnlyCollection<ScalarBinding>.Count
        {
            get { return 1; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        ScalarBinding IReadOnlyList<ScalarBinding>.this[int index]
        {
            get
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        void IConcatList<ScalarBinding>.Sort(Comparison<ScalarBinding> comparision)
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator<ScalarBinding> IEnumerable<ScalarBinding>.GetEnumerator()
        {
            yield return this;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Child types will not call this method.")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this;
        }

        #endregion

        public ScalarBinding Parent { get; private set; }

        public sealed override Binding ParentBinding
        {
            get { return Parent; }
        }

        internal void Seal<T>(ScalarCompositeBinding<T> parent, int ordinal)
            where T : UIElement, new()
        {
            Parent = parent;
            Ordinal = ordinal;
        }

        internal abstract void BeginSetup(int startOffset, UIElement[] elements);

        internal abstract void BeginSetup(UIElement element);

        internal abstract UIElement Setup(int flowIndex);

        internal ScalarPresenter ScalarPresenter
        {
            get { return Template.ScalarPresenter; }
        }

        private bool _flowRepeatable;
        [DefaultValue(false)]
        public bool FlowRepeatable
        {
            get { return Parent != null ? Parent.FlowRepeatable : _flowRepeatable; }
            set
            {
                VerifyNotSealed();
                _flowRepeatable = value;
            }
        }

        internal int CumulativeFlowRepeatCountDelta { get; set; }

        internal override void VerifyRowRange(GridRange rowRange)
        {
            if (GridRange.IntersectsWith(rowRange))
                throw new InvalidOperationException(DiagnosticMessages.ScalarBinding_IntersectsWithRowRange(Ordinal));

            if (!FlowRepeatable)
                return;

            if (Template.Flowable(Orientation.Horizontal))
            {
                if (!rowRange.Contains(GridRange.Left) || !rowRange.Contains(GridRange.Right))
                    throw new InvalidOperationException(DiagnosticMessages.ScalarBinding_OutOfHorizontalRowRange(Ordinal));
            }
            else if (Template.Flowable(Orientation.Vertical))
            {
                if (!rowRange.Contains(GridRange.Top) || !rowRange.Contains(GridRange.Bottom))
                    throw new InvalidOperationException(DiagnosticMessages.ScalarBinding_OutOfVerticalRowRange(Ordinal));
            }
            else
                throw new InvalidOperationException(DiagnosticMessages.ScalarBinding_FlowRepeatableNotAllowedByTemplate(Ordinal));
        }

        private ElementManager ElementManager
        {
            get { return Template.ElementManager; }
        }

        public int FlowRepeatCount
        {
            get { return Parent != null ? Parent.FlowRepeatCount : (FlowRepeatable ? ElementManager.FlowRepeatCount : 1); }
        }

        internal abstract UIElement GetChild(UIElement parent, int index);

        public UIElement this[int flowIndex]
        {
            get
            {
                if (Ordinal == -1)
                    return null;

                if (Parent != null)
                    return Parent.GetChild(Parent[flowIndex], Ordinal);

                if (flowIndex < 0 || flowIndex >= FlowRepeatCount)
                    throw new ArgumentOutOfRangeException(nameof(flowIndex));

                var ordinal = Ordinal;
                int prevCumulativeFlowRepeatCountDelta = ordinal == 0 ? 0 : Template.ScalarBindings[ordinal - 1].CumulativeFlowRepeatCountDelta;
                var elementIndex = ordinal * FlowRepeatCount - prevCumulativeFlowRepeatCountDelta + flowIndex;
                if (ordinal >= Template.ScalarBindingsSplit)
                {
                    elementIndex += ElementManager.ContainerViewList.Count;
                    if (ElementManager.IsCurrentContainerViewIsolated)
                        elementIndex++;
                }
                return ElementManager.Elements[elementIndex];
            }
        }

        internal override void VerifyFrozenMargins(string templateItemsName)
        {
            base.VerifyFrozenMargins(templateItemsName);
            if (LayoutOrientation == Orientation.Horizontal)
                VerifyHorizontalStretches();
            else
                VerifyVerticalStretches();
        }

        private Orientation LayoutOrientation
        {
            get { return Template.Orientation.Value; }
        }

        private void VerifyHorizontalStretches()
        {
            if (GridRange.HorizontallyIntersectsWith(Template.GridColumns.Count - Template.Stretches))
                throw new InvalidOperationException(DiagnosticMessages.ScalarBinding_InvalidStretches(Ordinal));
        }

        private void VerifyVerticalStretches()
        {
            if (GridRange.VerticallyIntersectsWith(Template.GridRows.Count - Template.Stretches))
                throw new InvalidOperationException(DiagnosticMessages.ScalarBinding_InvalidStretches(Ordinal));
        }

        internal override AutoSizeWaiver CoercedAutoSizeWaiver
        {
            get
            {
                var result = base.CoercedAutoSizeWaiver;
                if (!Template.Orientation.HasValue)
                    return result;

                if (LayoutOrientation == Orientation.Horizontal)
                {
                    if (Template.RowRange.ColumnSpan.IntersectsWith(GridRange.ColumnSpan))
                        result |= AutoSizeWaiver.Width;
                }
                else
                {
                    if (Template.RowRange.RowSpan.IntersectsWith(GridRange.RowSpan))
                        result |= AutoSizeWaiver.Height;
                }

                return result;
            }
        }
    }
}
