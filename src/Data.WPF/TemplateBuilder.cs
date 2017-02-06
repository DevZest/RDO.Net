using DevZest.Data.Windows.Primitives;
using DevZest.Data.Primitives;
using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.ComponentModel;

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

        [DefaultValue(0)]
        public TemplateBuilder WithFrozenLeft(int tracks)
        {
            if (tracks < 0)
                throw new ArgumentOutOfRangeException(nameof(tracks));
            Template.FrozenLeft = tracks;
            return this;
        }

        [DefaultValue(0)]
        public TemplateBuilder WithFrozenTop(int tracks)
        {
            if (tracks < 0)
                throw new ArgumentOutOfRangeException(nameof(tracks));
            Template.FrozenTop = tracks;
            return this;
        }

        [DefaultValue(0)]
        public TemplateBuilder WithFrozenRight(int tracks)
        {
            if (tracks < 0)
                throw new ArgumentOutOfRangeException(nameof(tracks));
            Template.FrozenRight = tracks;
            return this;
        }

        [DefaultValue(0)]
        public TemplateBuilder WithFrozenBottom(int tracks)
        {
            if (tracks < 0)
                throw new ArgumentOutOfRangeException(nameof(tracks));
            Template.FrozenBottom = tracks;
            return this;
        }

        [DefaultValue(0)]
        public TemplateBuilder WithStretches(int tracks)
        {
            if (tracks < 0)
                throw new ArgumentOutOfRangeException(nameof(tracks));
            Template.Stretches = tracks;
            return this;
        }

        [DefaultValue(RowPlaceholderMode.Explicit)]
        public TemplateBuilder WithRowPlaceholderMode(RowPlaceholderMode value)
        {
            Template.RowPlaceholderMode = value;
            return this;
        }

        [DefaultValue(true)]
        public TemplateBuilder WithTransactionalEdit(bool value)
        {
            Template.TransactionalEdit = value;
            return this;
        }

        [DefaultValue(null)]
        public TemplateBuilder WithSelectionMode(SelectionMode value)
        {
            Template.SelectionMode = value;
            return this;
        }

        [DefaultValue(ValidationScope.CurrentRow)]
        public TemplateBuilder WithValidationScope(ValidationScope value)
        {
            Template.ValidationScope = value;
            return this;
        }

        [DefaultValue(ValidationMode.Progressive)]
        public TemplateBuilder WithValidationMode(ValidationMode value)
        {
            Template.ValidationMode = value;
            return this;
        }

        [DefaultValue(100)]
        public TemplateBuilder WithValidationErrorMaxEntries(int value)
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value));
            Template.ValidationErrorMaxEntries = value;
            return this;
        }

        [DefaultValue(100)]
        public TemplateBuilder WithValidationWarningMaxEntries(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            Template.ValidationWarningMaxEntries = value;
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

        public TemplateBuilder BlockView<T>(Style style = null)
            where T : BlockView, new()
        {
            Template.BlockView<T>(style);
            return this;
        }

        public TemplateBuilder RowView<T>(Style style = null)
            where T : RowView, new()
        {
            Template.RowView<T>(style);
            return this;
        }

        public TemplateBuilder AddBinding(int column, int row, ScalarBinding scalarBinding)
        {
            return AddBinding(column, row, column, row, scalarBinding);
        }

        public TemplateBuilder AddBinding(int left, int top, int right, int bottom, ScalarBinding scalarBinding)
        {
            Binding.VerifyAdding(scalarBinding, nameof(scalarBinding));
            Template.AddBinding(Template.Range(left, top, right, bottom), scalarBinding);
            return this;
        }

        public TemplateBuilder AddBinding(int column, int row, BlockBinding blockBinding)
        {
            return AddBinding(column, row, column, row, blockBinding);
        }

        public TemplateBuilder AddBinding(int left, int top, int right, int bottom, BlockBinding blockBinding)
        {
            Binding.VerifyAdding(blockBinding, nameof(blockBinding));
            Template.AddBinding(Template.Range(left, top, right, bottom), blockBinding);
            return this;
        }

        public TemplateBuilder AddBinding(int column, int row, RowBinding rowBinding)
        {
            return AddBinding(column, row, column, row, rowBinding);
        }

        public TemplateBuilder AddBinding(int left, int top, int right, int bottom, RowBinding rowBinding)
        {
            Binding.VerifyAdding(rowBinding, nameof(rowBinding));
            Template.AddBinding(Template.Range(left, top, right, bottom), rowBinding);
            return this;
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
