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
            Template.VerifyTemplateItemGridRange();
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

        public TemplateBuilder RowRange(int column, int row)
        {
            Template.RowRange = Template.Range(column, row);
            return this;
        }

        public TemplateBuilder RowRange(int left, int top, int right, int bottom)
        {
            Template.RowRange = Template.Range(left, top, right, bottom);
            return this;
        }

        public TemplateBuilder Layout(Orientation orientation, int blockDimensions = 1)
        {
            if (blockDimensions < 0)
                throw new ArgumentOutOfRangeException(nameof(blockDimensions));

            Template.Block(orientation, blockDimensions);
            return this;
        }

        public TemplateItemBuilderFactory this[int column, int row]
        {
            get { return new TemplateItemBuilderFactory(this, Template.Range(column, row)); }
        }

        public TemplateItemBuilderFactory this[int left, int top, int right, int bottom]
        {
            get { return new TemplateItemBuilderFactory(this, Template.Range(left, top, right, bottom)); }
        }

        public TemplateBuilder Fix(int left = 0, int top = 0, int right = 0, int bottom = 0)
        {
            Template.FixedLeft = left;
            Template.FixedTop = top;
            Template.FixedRight = right;
            Template.FixedBottom = bottom;
            return this;
        }

        public TemplateBuilder EOF(EofVisibility value)
        {
            Template.EofVisibility = value;
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

        public TemplateBuilder BlockView<T>(Action<T> blockViewIntializer = null, Action<T> blockViewCleanupAction = null)
            where T : BlockView, new()
        {
            Template.BlockViewConstructor = () => new T();

            if (blockViewIntializer == null)
                Template.BlockViewInitializer = null;
            else
                Template.BlockViewInitializer = rowView => blockViewIntializer((T)rowView);

            if (blockViewCleanupAction == null)
                Template.BlockViewCleanupAction = null;
            else
                Template.BlockViewCleanupAction = rowView => blockViewCleanupAction((T)rowView);
            return this;
        }

        public TemplateBuilder RowItemGroup(Func<RowPresenter, int> rowItemsSelector)
        {
            Template.RowItemGroupSelector = rowItemsSelector;
            return this;
        }

        public TemplateBuilder Pin(int value)
        {
            Template.PinnedTail = value;
            return this;
        }
    }
}
