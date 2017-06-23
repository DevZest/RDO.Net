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

        public CompositeScalarBinding Parent { get; private set; }

        public sealed override Binding ParentBinding
        {
            get { return Parent; }
        }

        internal void Seal(CompositeScalarBinding parent, int ordinal)
        {
            Parent = parent;
            Ordinal = ordinal;
        }

        internal abstract void BeginSetup(int startOffset);

        internal abstract UIElement Setup(int flowIndex);

        internal abstract void PrepareSettingUpElement(int flowIndex);

        internal abstract void ClearSettingUpElement();

        internal ScalarPresenter ScalarPresenter
        {
            get { return Template.ScalarPresenter; }
        }

        internal void EnterSetup(int flowIndex)
        {
            var scalarBindings = Template.ScalarBindings;
            for (int i = 0; i < scalarBindings.Count; i++)
                scalarBindings[i].PrepareSettingUpElement(flowIndex);

            ScalarPresenter.SetFlowIndex(flowIndex);
        }

        internal void ExitSetup()
        {
            var scalarBindings = Template.ScalarBindings;
            for (int i = 0; i < scalarBindings.Count; i++)
                scalarBindings[i].ClearSettingUpElement();

            ScalarPresenter.SetFlowIndex(0);
        }

        private bool _flowable;
        [DefaultValue(false)]
        public bool Flowable
        {
            get { return Parent != null ? Parent.Flowable : _flowable; }
            set
            {
                VerifyNotSealed();
                _flowable = value;
            }
        }

        internal int CumulativeFlowCountDelta { get; set; }

        internal override void VerifyRowRange(GridRange rowRange)
        {
            if (GridRange.IntersectsWith(rowRange))
                throw new InvalidOperationException(Strings.ScalarBinding_IntersectsWithRowRange(Ordinal));

            if (!Flowable)
                return;

            if (Template.Flowable(Orientation.Horizontal))
            {
                if (!rowRange.Contains(GridRange.Left) || !rowRange.Contains(GridRange.Right))
                    throw new InvalidOperationException(Strings.ScalarBinding_OutOfHorizontalRowRange(Ordinal));
            }
            else if (Template.Flowable(Orientation.Vertical))
            {
                if (!rowRange.Contains(GridRange.Top) || !rowRange.Contains(GridRange.Bottom))
                    throw new InvalidOperationException(Strings.ScalarBinding_OutOfVerticalRowRange(Ordinal));
            }
            else
                throw new InvalidOperationException(Strings.ScalarBinding_FlowableNotAllowedByTemplate(Ordinal));
        }

        private ElementManager ElementManager
        {
            get { return Template.ElementManager; }
        }

        public int FlowCount
        {
            get { return Parent != null ? Parent.FlowCount : (Flowable ? ElementManager.FlowCount : 1); }
        }

        public UIElement this[int flowIndex]
        {
            get
            {
                if (Ordinal == -1)
                    return null;

                if (Parent != null)
                    return ((ICompositeView)Parent[flowIndex]).CompositeBinding.Children[Ordinal];

                if (flowIndex < 0 || flowIndex >= FlowCount)
                    throw new ArgumentOutOfRangeException(nameof(flowIndex));

                var ordinal = Ordinal;
                int prevCumulativeFlowCountDelta = ordinal == 0 ? 0 : Template.ScalarBindings[ordinal - 1].CumulativeFlowCountDelta;
                var elementIndex = ordinal * FlowCount - prevCumulativeFlowCountDelta + flowIndex;
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
                throw new InvalidOperationException(Strings.ScalarBinding_InvalidStretches(Ordinal));
        }

        private void VerifyVerticalStretches()
        {
            if (GridRange.VerticallyIntersectsWith(Template.GridRows.Count - Template.Stretches))
                throw new InvalidOperationException(Strings.ScalarBinding_InvalidStretches(Ordinal));
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
