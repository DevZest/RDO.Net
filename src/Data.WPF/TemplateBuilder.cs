using DevZest.Data.Windows.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Windows.Controls;

namespace DevZest.Data.Windows
{
    public sealed class TemplateBuilder : IDisposable
    {
        internal TemplateBuilder(Template template, Model model)
        {
            Debug.Assert(template != null);
            Template = template;
            _model = model;
        }

        public void Dispose()
        {
            Template = null;
        }

        internal Template Template { get; private set; }

        private Model _model;

        public TemplateBuilder AddGridColumn(string width)
        {
            Template.AddGridColumn(width);
            return this;
        }

        public TemplateBuilder AddGridColumn(string width, out int index)
        {
            index = Template.AddGridColumn(width);
            return this;
        }

        public TemplateBuilder AddGridColumns(params string[] widths)
        {
            if (widths == null)
                throw new ArgumentNullException(nameof(widths));

            Template.AddGridColumns(widths);
            return this;
        }

        public TemplateBuilder AddGridRow(string height)
        {
            Template.AddGridRow(height);
            return this;
        }

        public TemplateBuilder AddGridRow(string height, out int index)
        {
            index = Template.AddGridRow(height);
            return this;
        }

        public TemplateBuilder AddGridRows(params string[] heights)
        {
            if (heights == null)
                throw new ArgumentNullException(nameof(heights));

            Template.AddGridRows(heights);
            return this;
        }

        public TemplateBuilder Stack(Orientation orientation, int stackDimensions = 1)
        {
            if (stackDimensions < 0)
                throw new ArgumentOutOfRangeException(nameof(stackDimensions));

            Template.Stack(orientation, stackDimensions);
            return this;
        }

        public GridRangeBuilder Range(int column, int row)
        {
            return new GridRangeBuilder(this, Template.Range(column, row));
        }

        public GridRangeBuilder Range(int left, int top, int right, int bottom)
        {
            return new GridRangeBuilder(this, Template.Range(left, top, right, bottom));
        }

        public TemplateBuilder PinLeft(int value)
        {
            Template.PinnedLeft = value;
            return this;
        }

        public TemplateBuilder PinTop(int value)
        {
            Template.PinnedTop = value;
            return this;
        }

        public TemplateBuilder PinRight(int value)
        {
            Template.PinnedRight = value;
            return this;
        }

        public TemplateBuilder PinBottom(int value)
        {
            Template.PinnedBottom = value;
            return this;
        }

        public TemplateBuilder Pin(int left, int top, int right, int bottom)
        {
            Template.PinnedLeft = left;
            Template.PinnedTop = top;
            Template.PinnedRight = right;
            Template.PinnedBottom = bottom;
            return this;
        }

        public TemplateBuilder EofRow(EofRowMapping value)
        {
            Template.EofRowMapping = value;
            return this;
        }

        public TemplateBuilder SetVirtualizationThreshold(int value)
        {
            Template.VirtualizationThreshold = value;
            return this;
        }

        public TemplateBuilder Hierarchical(Model childModel)
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));

            if (childModel.GetParentModel() != _model || childModel.GetType() != _model.GetType())
                throw new ArgumentException(Strings.TemplateBuilder_InvalidFlattenHierarchyChildModel);

            Template.HierarchicalModelOrdinal = childModel.GetOrdinal();
            return this;
        }

        public TemplateBuilder RowView<T>(Action<T> rowViewIntializer = null, Action<T> rowViewCleanupAction = null)
            where T : RowView, new()
        {
            Template.RowViewConstructor = () => new T();

            if (rowViewIntializer == null)
                Template.RowViewInitializer = null;
            else
                Template.RowViewInitializer = rowView => rowViewIntializer((T)rowView);

            if (rowViewCleanupAction == null)
                Template.RowViewCleanupAction = null;
            else
                Template.RowViewCleanupAction = rowView => rowViewCleanupAction((T)rowView);
            return this;
        }
    }
}
