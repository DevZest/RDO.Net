﻿using DevZest.Data.Windows.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using DevZest.Data.Windows.Controls;

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
