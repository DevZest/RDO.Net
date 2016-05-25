using DevZest.Data.Windows.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;

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
            Template.VerifyLayout();
            Template = null;
        }

        internal Template Template { get; private set; }

        private Model _model;

        public TemplateBuilder GridColumns(params string[] widths)
        {
            if (widths == null)
                throw new ArgumentNullException(nameof(widths));

            Template.AddGridColumns(widths);
            return this;
        }

        public TemplateBuilder GridRows(params string[] heights)
        {
            if (heights == null)
                throw new ArgumentNullException(nameof(heights));

            Template.AddGridRows(heights);
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

            Template.Layout(orientation, blockDimensions);
            return this;
        }

        public TemplateBuilder FrozenLeft(int tracks)
        {
            if (tracks < 0)
                throw new ArgumentOutOfRangeException(nameof(tracks));
            Template.FrozenLeft = tracks;
            return this;
        }

        public TemplateBuilder FrozenTop(int tracks)
        {
            if (tracks < 0)
                throw new ArgumentOutOfRangeException(nameof(tracks));
            Template.FrozenTop = tracks;
            return this;
        }

        public TemplateBuilder FrozenRight(int tracks)
        {
            if (tracks < 0)
                throw new ArgumentOutOfRangeException(nameof(tracks));
            Template.FrozenRight = tracks;
            return this;
        }

        public TemplateBuilder FrozenBottom(int tracks)
        {
            if (tracks < 0)
                throw new ArgumentOutOfRangeException(nameof(tracks));
            Template.FrozenBottom = tracks;
            return this;
        }

        public TemplateBuilder Stretch(int tracks)
        {
            if (tracks < 0)
                throw new ArgumentOutOfRangeException(nameof(tracks));
            Template.Stretches = tracks;
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
                Template.BlockViewInitializer = blockView => blockViewIntializer((T)blockView);

            if (blockViewCleanupAction == null)
                Template.BlockViewCleanupAction = null;
            else
                Template.BlockViewCleanupAction = blockView => blockViewCleanupAction((T)blockView);
            return this;
        }

        public TemplateBuilder RowItemGroup(Func<RowPresenter, int> rowItemsSelector)
        {
            Template.RowItemGroupSelector = rowItemsSelector;
            return this;
        }

        public ScalarItem.Builder<T> ScalarItem<T>(bool isMultidimensional = false)
            where T : UIElement, new()
        {
            return new ScalarItem.Builder<T>(this, isMultidimensional);
        }

        public BlockItem.Builder<T> BlockItem<T>()
            where T : UIElement, new()
        {
            return new BlockItem.Builder<T>(this);
        }

        public RowItem.Builder<T> RowItem<T>()
            where T : UIElement, new()
        {
            return new RowItem.Builder<T>(this);
        }

        public SubviewItem.Builder<TView> SubviewItem<TModel, TView>(TModel childModel, Action<TemplateBuilder, TModel> buildTemplateAction)
            where TModel : Model, new()
            where TView : DataView, new()
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));
            if (buildTemplateAction == null)
                throw new ArgumentNullException(nameof(buildTemplateAction));

            return new SubviewItem.Builder<TView>(this, rowPresenter =>
            {
                if (rowPresenter.IsEof)
                    return null;
                return DataPresenter.Create(rowPresenter, childModel, buildTemplateAction);
            });
        }

        public SubviewItem.Builder<TView> SubviewItem<TModel, TView>(_DataSet<TModel> child, Action<TemplateBuilder, TModel> buildTemplateAction)
            where TModel : Model, new()
            where TView : DataView, new()
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));
            if (buildTemplateAction == null)
                throw new ArgumentNullException(nameof(buildTemplateAction));

            return new SubviewItem.Builder<TView>(this, rowPresenter =>
            {
                var dataRow = rowPresenter.DataRow;
                if (dataRow == null)
                    return null;
                var childDataSet = child[dataRow];
                if (childDataSet == null)
                    return null;
                return DataPresenter.Create(childDataSet, buildTemplateAction);
            });
        }

        public TemplateBuilder GridLineX(int startGridOffset, int endGridOffset, Pen pen, GridLinePosition position = GridLinePosition.Both)
        {
            return GridLine(Orientation.Horizontal, startGridOffset, endGridOffset, pen, position);
        }

        public TemplateBuilder GridLineY(int startGridOffset, int endGridOffset, Pen pen, GridLinePosition position = GridLinePosition.Both)
        {
            return GridLine(Orientation.Vertical, startGridOffset, endGridOffset, pen, position);
        }

        private TemplateBuilder GridLine(Orientation orientation, int startGridOffset, int endGridOffset, Pen pen, GridLinePosition position = GridLinePosition.Both)
        {
            IReadOnlyList<GridTrack> gridTracks;
            if (orientation == Orientation.Horizontal)
                gridTracks = Template.GridColumns;
            else
                gridTracks = Template.GridRows;

            if (startGridOffset < 0 || startGridOffset > gridTracks.Count)
                throw new ArgumentOutOfRangeException(nameof(startGridOffset));

            if (endGridOffset <= startGridOffset || endGridOffset > gridTracks.Count)
                throw new ArgumentOutOfRangeException(nameof(endGridOffset));

            if (pen == null)
                throw new ArgumentNullException(nameof(pen));

            var gridLine = new GridLine(orientation, startGridOffset, endGridOffset, pen, position);
            Template.AddGridLine(gridLine);
            return this;
        }
    }
}
