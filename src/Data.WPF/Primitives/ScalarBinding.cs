using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Windows.Primitives
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

        public ScalarPane Parent { get; private set; }

        public sealed override Binding ParentBinding
        {
            get { return Parent; }
        }

        internal void Seal(ScalarPane parent, int ordinal)
        {
            if (IsMultidimensional)
                throw new InvalidOperationException(Strings.ScalarBinding_ChildMultidimensional);
            Parent = parent;
            Ordinal = ordinal;
        }

        internal abstract void BeginSetup(int startOffset);

        internal abstract UIElement Setup(int blockDimension);

        internal abstract void PrepareSettingUpElement(int blockDimension);

        internal abstract void ClearSettingUpElement();

        internal void EnterSetup(int blockDimension)
        {
            var scalarBindings = Template.ScalarBindings;
            for (int i = 0; i < scalarBindings.Count; i++)
                scalarBindings[i].PrepareSettingUpElement(blockDimension);
        }

        internal void ExitSetup()
        {
            var scalarBindings = Template.ScalarBindings;
            for (int i = 0; i < scalarBindings.Count; i++)
                scalarBindings[i].ClearSettingUpElement();
        }

        private bool _isMultidimensional;
        public bool IsMultidimensional
        {
            get { return _isMultidimensional; }
            set
            {
                VerifyNotSealed();
                if (value && Parent != null)
                    throw new InvalidOperationException(Strings.ScalarBinding_ChildMultidimensional);
                _isMultidimensional = value;
            }
        }

        internal int CumulativeBlockDimensionsDelta { get; set; }

        internal override void VerifyRowRange(GridRange rowRange)
        {
            if (GridRange.IntersectsWith(rowRange))
                throw new InvalidOperationException(Strings.ScalarBinding_IntersectsWithRowRange(Ordinal));

            if (!IsMultidimensional)
                return;

            if (Template.IsMultidimensional(Orientation.Horizontal))
            {
                if (!rowRange.Contains(GridRange.Left) || !rowRange.Contains(GridRange.Right))
                    throw new InvalidOperationException(Strings.ScalarBinding_OutOfHorizontalRowRange(Ordinal));
            }
            else if (Template.IsMultidimensional(Orientation.Vertical))
            {
                if (!rowRange.Contains(GridRange.Top) || !rowRange.Contains(GridRange.Bottom))
                    throw new InvalidOperationException(Strings.ScalarBinding_OutOfVerticalRowRange(Ordinal));
            }
            else
                throw new InvalidOperationException(Strings.ScalarBinding_OneDimensionalTemplate(Ordinal));
        }

        private ElementManager ElementManager
        {
            get { return Template.ElementManager; }
        }

        public int BlockDimensions
        {
            get { return IsMultidimensional ? ElementManager.BlockDimensions : 1; }
        }

        public UIElement this[int blockDimension]
        {
            get
            {
                if (Ordinal == -1)
                    return null;

                if (Parent != null)
                    return ((Pane)Parent[blockDimension]).Children[Ordinal];

                if (blockDimension < 0 || blockDimension >= BlockDimensions)
                    throw new ArgumentOutOfRangeException(nameof(blockDimension));

                var ordinal = Ordinal;
                int prevCumulativeBlockDimensionsDelta = ordinal == 0 ? 0 : Template.ScalarBindings[ordinal - 1].CumulativeBlockDimensionsDelta;
                var elementIndex = ordinal * BlockDimensions - prevCumulativeBlockDimensionsDelta + blockDimension;
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
