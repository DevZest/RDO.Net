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

        public TemplateBuilder With(RowPlaceholderMode value)
        {
            Template.RowPlaceholderMode = value;
            return this;
        }

        public TemplateBuilder Recurse(Model childModel)
        {
            if (childModel == null)
                throw new ArgumentNullException(nameof(childModel));

            if (childModel.GetParentModel() != _model || childModel.GetType() != _model.GetType())
                throw new ArgumentException(Strings.TemplateBuilder_InvalidRecursiveChildModel);

            Template.RecursiveModelOrdinal = childModel.GetOrdinal();
            return this;
        }

        public TemplateBuilder RowView<T>(Action<T> onSetup = null, Action<T> onCleanup = null)
            where T : RowView, new()
        {
            Template.RowViewConstructor = () => new T();

            if (onSetup == null)
                Template.OnSetupRowView = null;
            else
                Template.OnSetupRowView = rowView => onSetup((T)rowView);

            if (onCleanup == null)
                Template.OnCleanupRowView = null;
            else
                Template.OnCleanupRowView = rowView => onCleanup((T)rowView);
            return this;
        }

        public TemplateBuilder BlockView<T>(Action<T> onSetup = null, Action<T> onCleanup = null)
            where T : BlockView, new()
        {
            Template.BlockViewConstructor = () => new T();

            if (onSetup == null)
                Template.OnSetupBlockView = null;
            else
                Template.OnSetupBlockView = blockView => onSetup((T)blockView);

            if (onCleanup == null)
                Template.OnCleanupBlockView = null;
            else
                Template.OnCleanupBlockView = blockView => onCleanup((T)blockView);
            return this;
        }

        public ScalarBinding.Builder<T> ScalarBinding<T>(bool isMultidimensional = false)
            where T : UIElement, new()
        {
            return new ScalarBinding.Builder<T>(this, isMultidimensional);
        }

        public BlockBinding.Builder<T> BlockBinding<T>()
            where T : UIElement, new()
        {
            return new BlockBinding.Builder<T>(this);
        }

        public RowBinding.Builder<T> RowBinding<T>()
            where T : UIElement, new()
        {
            return new RowBinding.Builder<T>(this);
        }

        public TemplateBuilder GridLineX(GridPoint startGridPoint, int length, Pen pen = null, GridLinePosition position = GridLinePosition.Both)
        {
            return GridLine(Orientation.Horizontal, startGridPoint, length, pen, position);
        }

        public TemplateBuilder GridLineY(GridPoint startGridPoint, int length, Pen pen = null, GridLinePosition position = GridLinePosition.Both)
        {
            return GridLine(Orientation.Vertical, startGridPoint, length, pen, position);
        }

        private static readonly Pen DefaultGridLinePen = GetDefaultGridLinePen();
        private static Pen GetDefaultGridLinePen()
        {
            var result = new Pen(Brushes.LightGray, 1);
            result.Freeze();
            return result;
        }

        private TemplateBuilder GridLine(Orientation orientation, GridPoint startGridPoint, int length, Pen pen, GridLinePosition position = GridLinePosition.Both)
        {
            IReadOnlyList<GridTrack> gridTracks;
            if (orientation == Orientation.Horizontal)
                gridTracks = Template.GridColumns;
            else
                gridTracks = Template.GridRows;

            if (startGridPoint.X > Template.GridColumns.Count || startGridPoint.Y > Template.GridRows.Count)
                throw new ArgumentOutOfRangeException(nameof(startGridPoint));

            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (pen == null)
                pen = DefaultGridLinePen;

            int endGridOffsetX = startGridPoint.X;
            int endGridOffsetY = startGridPoint.Y;
            if (orientation == Orientation.Horizontal)
            {
                endGridOffsetX += length;
                if (endGridOffsetX > Template.GridColumns.Count)
                    throw new ArgumentOutOfRangeException(nameof(length));
            }
            else
            {
                endGridOffsetY += length;
                if (endGridOffsetY > Template.GridRows.Count)
                    throw new ArgumentOutOfRangeException(nameof(length));
            }

            var gridLine = new GridLine(startGridPoint, new GridPoint(endGridOffsetX, endGridOffsetY), pen, position);
            Template.AddGridLine(gridLine);
            return this;
        }
    }
}
