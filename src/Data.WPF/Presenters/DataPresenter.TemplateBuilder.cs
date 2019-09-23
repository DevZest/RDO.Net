using DevZest.Data.Presenters.Primitives;
using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using DevZest.Data.Views;

namespace DevZest.Data.Presenters
{
    partial class DataPresenter
    {
        /// <summary>
        /// Supports building template of data presenter.
        /// </summary>
        protected internal sealed class TemplateBuilder : TemplateBuilder<TemplateBuilder>
        {
            internal TemplateBuilder(Template template, Model model, bool inherited = false)
                : base(template)
            {
                _model = model;
                _inherited = inherited;
                if (inherited)
                    template.InitializeAsInherited();
            }

            private readonly bool _inherited;

            private Model _model;

            /// <summary>
            /// Defines layout grid columns.
            /// </summary>
            /// <param name="widths">The width values of grid columns. Must be valid string values of <see cref="GridLength"/>.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder GridColumns(params string[] widths)
            {
                if (_inherited)
                    return this;

                if (widths == null)
                    throw new ArgumentNullException(nameof(widths));

                Template.AddGridColumns(widths);
                return this;
            }

            /// <summary>
            /// Defines layout grid rows.
            /// </summary>
            /// <param name="heights">The height values of grid columns. Must be valid string values of <see cref="GridLength"/>.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder GridRows(params string[] heights)
            {
                if (_inherited)
                    return this;

                if (heights == null)
                    throw new ArgumentNullException(nameof(heights));

                Template.AddGridRows(heights);
                return this;
            }

            /// <summary>
            /// Defines the grid range for row.
            /// </summary>
            /// <param name="left">The left grid column index.</param>
            /// <param name="top">The top grid row index.</param>
            /// <param name="right">The right grid column index.</param>
            /// <param name="bottom">The bottom grid row index.</param>
            /// <returns>This template builder for fluent coding.</returns>
            /// <remarks>By default, row range is calcuated automatically to union all grid cells which contains row binding.
            /// You can call this method to specify custom row range to include grid cells does not contain any row binding.</remarks>
            public TemplateBuilder RowRange(int left, int top, int right, int bottom)
            {
                if (_inherited)
                    return this;

                Template.RowRange = Template.Range(left, top, right, bottom);
                return this;
            }

            /// <summary>
            /// Defines how the layout expands with row collection.
            /// </summary>
            /// <param name="orientation">Specifies the row expanding direction of layout.</param>
            /// <param name="flowRepeatCount">Defines layout that rows will flow in BlockView first, then expand afterwards. The default value is
            /// 1, which means bypass BlockView level. Value of -1 means number of flow repeat will be calculated automatically based on
            /// available space.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder Layout(Orientation orientation, int flowRepeatCount = 1)
            {
                if (_inherited)
                    return this;

                if (flowRepeatCount < 0)
                    throw new ArgumentOutOfRangeException(nameof(flowRepeatCount));

                Template.Layout(orientation, flowRepeatCount);
                return this;
            }

            /// <summary>
            /// Defines left frozen grid columns from scrolling.
            /// </summary>
            /// <param name="tracks">Number of left grid columns</param>
            /// <returns>This template builder for fluent coding.</returns>
            [DefaultValue(0)]
            public TemplateBuilder WithFrozenLeft(int tracks)
            {
                if (_inherited)
                    return this;

                if (tracks < 0)
                    throw new ArgumentOutOfRangeException(nameof(tracks));
                Template.FrozenLeft = tracks;
                return this;
            }

            /// <summary>
            /// Defines top frozen grid rows from scrolling.
            /// </summary>
            /// <param name="tracks">Number of top grid rows.</param>
            /// <returns>This template builder for fluent coding.</returns>
            [DefaultValue(0)]
            public TemplateBuilder WithFrozenTop(int tracks)
            {
                if (_inherited)
                    return this;

                if (tracks < 0)
                    throw new ArgumentOutOfRangeException(nameof(tracks));
                Template.FrozenTop = tracks;
                return this;
            }

            /// <summary>
            /// Defines right frozen grid columns from scrolling.
            /// </summary>
            /// <param name="tracks">Number of right grid columns</param>
            /// <returns>This template builder for fluent coding.</returns>
            [DefaultValue(0)]
            public TemplateBuilder WithFrozenRight(int tracks)
            {
                if (_inherited)
                    return this;

                if (tracks < 0)
                    throw new ArgumentOutOfRangeException(nameof(tracks));
                Template.FrozenRight = tracks;
                return this;
            }

            /// <summary>
            /// Defines bottom frozen grid rows from scrolling.
            /// </summary>
            /// <param name="tracks">Number of bottom grid rows.</param>
            /// <returns>This template builder for fluent coding.</returns>
            [DefaultValue(0)]
            public TemplateBuilder WithFrozenBottom(int tracks)
            {
                if (_inherited)
                    return this;

                if (tracks < 0)
                    throw new ArgumentOutOfRangeException(nameof(tracks));
                Template.FrozenBottom = tracks;
                return this;
            }

            /// <summary>
            /// Defines number of grid tracks stretching to end of view port.
            /// </summary>
            /// <param name="tracks">The number of grid tracks.</param>
            /// <returns>This template builder for fluent coding.</returns>
            [DefaultValue(0)]
            public TemplateBuilder WithStretches(int tracks)
            {
                if (_inherited)
                    return this;

                if (tracks < 0)
                    throw new ArgumentOutOfRangeException(nameof(tracks));
                Template.Stretches = tracks;
                return this;
            }

            /// <summary>
            /// Defines where to display the virtual row for inserting.
            /// </summary>
            /// <param name="value">Specifies where to display the virtual row.</param>
            /// <returns>This template builder for fluent coding.</returns>
            [DefaultValue(VirtualRowPlacement.Explicit)]
            public TemplateBuilder WithVirtualRowPlacement(VirtualRowPlacement value)
            {
                if (_inherited)
                    return this;

                Template.VirtualRowPlacement = value;
                return this;
            }

            /// <summary>
            /// Defines the selection mode.
            /// </summary>
            /// <param name="value">The selection mode.</param>
            /// <returns>This template builder for fluent coding.</returns>
            [DefaultValue(null)]
            public TemplateBuilder WithSelectionMode(SelectionMode value)
            {
                if (_inherited)
                    return this;

                Template.SelectionMode = value;
                return this;
            }

            private static string GetDefaultDisplayName(IColumns sourceColumns)
            {
                if (sourceColumns.Count == 1)
                    return sourceColumns.First<Column>().DisplayName;
                else
                    return string.Format("[{0}]", string.Join(", ", sourceColumns.Select(x => x.DisplayName)));
            }

            /// <summary>
            /// Adds async validator.
            /// </summary>
            /// <param name="sourceColumns">The source columns of the validator.</param>
            /// <param name="validator">The delegate to perform validation.</param>
            /// <param name="displayName">The display name of the validator.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder AddAsyncValidator(IColumns sourceColumns, Func<DataRow, Task<string>> validator, string displayName = null)
            {
                if (sourceColumns == null || sourceColumns.Count == 0)
                    throw new ArgumentNullException(nameof(sourceColumns));
                if (validator == null)
                    throw new ArgumentNullException(nameof(validator));
                if (string.IsNullOrEmpty(displayName))
                    displayName = GetDefaultDisplayName(sourceColumns);
                Template.AddAsyncValidator(RowAsyncValidator.Create(displayName, sourceColumns, validator));
                return this;
            }

            /// <summary>
            /// Adds async validator.
            /// </summary>
            /// <param name="sourceColumns">The source columns of the validator.</param>
            /// <param name="validator">The delegate to perform validation.</param>
            /// <param name="displayName">The display name of the validator.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder AddAsyncValidator(IColumns sourceColumns, Func<DataRow, Task<IEnumerable<string>>> validator, string displayName = null)
            {
                if (sourceColumns == null || sourceColumns.Count == 0)
                    throw new ArgumentNullException(nameof(sourceColumns));
                if (validator == null)
                    throw new ArgumentNullException(nameof(validator));
                if (string.IsNullOrEmpty(displayName))
                    displayName = GetDefaultDisplayName(sourceColumns);
                Template.AddAsyncValidator(RowAsyncValidator.Create(displayName, sourceColumns, validator));
                return this;
            }

            /// <summary>
            /// Adds async validator.
            /// </summary>
            /// <param name="input">The row input.</param>
            /// <param name="validator">The delegate to perform validation.</param>
            /// <param name="displayName">The display name of the validator.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder AddAsyncValidator<T>(RowInput<T> input, Func<DataRow, Task<string>> validator, string displayName = null)
                where T : UIElement, new()
            {
                if (input == null)
                    throw new ArgumentNullException(nameof(input));

                return AddAsyncValidator(input.Target, validator, displayName);
            }

            /// <summary>
            /// Adds async validator.
            /// </summary>
            /// <param name="input">The row input.</param>
            /// <param name="validator">The delegate to perform validation.</param>
            /// <param name="displayName">The display name of the validator.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder AddAsyncValidator<T>(RowInput<T> input, Func<DataRow, Task<IEnumerable<string>>> validator, string displayName = null)
                where T : UIElement, new()
            {
                if (input == null)
                    throw new ArgumentNullException(nameof(input));

                return AddAsyncValidator(input.Target, validator, displayName);
            }

            /// <summary>
            /// Defines row validation mode.
            /// </summary>
            /// <param name="value">The row validaiton mode.</param>
            /// <returns>This template builder for fluent coding.</returns>
            [DefaultValue(ValidationMode.Progressive)]
            public TemplateBuilder WithRowValidationMode(ValidationMode value)
            {
                if (_inherited)
                    return this;

                Template.RowValidationMode = value;
                return this;
            }

            /// <inheritdoc/>
            public override TemplateBuilder WithScalarValidationMode(ValidationMode value)
            {
                if (_inherited)
                    return this;
                return base.WithScalarValidationMode(value);
            }

            /// <summary>
            /// Defines row collection as self recursive to show tree view like UI.
            /// </summary>
            /// <param name="childModel">The child model.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder MakeRecursive(Model childModel)
            {
                if (_inherited)
                    return this;

                if (childModel == null)
                    throw new ArgumentNullException(nameof(childModel));

                if (childModel.GetParent() != _model || childModel.GetType() != _model.GetType())
                    throw new ArgumentException(DiagnosticMessages.TemplateBuilder_InvalidRecursiveChildModel);

                Template.RecursiveModelOrdinal = childModel.GetOrdinal();
                return this;
            }

            /// <summary>
            /// Customizes block view.
            /// </summary>
            /// <typeparam name="T">Type of block view.</typeparam>
            /// <param name="style">The style applied to the block view.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder BlockView<T>(Style style = null)
                where T : BlockView, new()
            {
                if (_inherited)
                    return this;

                Template.BlockView<T>(style);
                return this;
            }

            /// <summary>
            /// Customizes block view.
            /// </summary>
            /// <typeparam name="T">Type of block view.</typeparam>
            /// <param name="styleId">The style applied to the block view.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder BlockView<T>(StyleId styleId)
                where T : BlockView, new()
            {
                if (_inherited)
                    return this;

                if (styleId == null)
                    throw new ArgumentNullException(nameof(styleId));
                return BlockView<T>(styleId.GetOrLoad());
            }

            /// <summary>
            /// Customizes row view.
            /// </summary>
            /// <typeparam name="T">Type of row view.</typeparam>
            /// <param name="style">The style applied to the row view.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder RowView<T>(Style style = null)
                where T : RowView, new()
            {
                if (_inherited)
                    return this;

                Template.RowView<T>(style);
                return this;
            }

            /// <summary>
            /// Customizes row view.
            /// </summary>
            /// <typeparam name="T">Type of row view.</typeparam>
            /// <param name="styleId">The style applied to the row view.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder RowView<T>(StyleId styleId)
                where T : RowView, new()
            {
                if (_inherited)
                    return this;

                if (styleId == null)
                    throw new ArgumentNullException(nameof(styleId));
                return RowView<T>(styleId.GetOrLoad());
            }

            /// <summary>
            /// Adds scalar binding.
            /// </summary>
            /// <param name="column">Index of grid column.</param>
            /// <param name="row">Index of grid row.</param>
            /// <param name="scalarBinding">The scalar binding.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder AddBinding(int column, int row, ScalarBinding scalarBinding)
            {
                return AddBinding(column, row, column, row, scalarBinding);
            }

            /// <summary>
            /// Adds scalar binding.
            /// </summary>
            /// <param name="left">Index of left grid column.</param>
            /// <param name="top">Index of top grid row.</param>
            /// <param name="right">Index of right grid column.</param>
            /// <param name="bottom">Index of bottom grid row.</param>
            /// <param name="scalarBinding">The scalar binding.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder AddBinding(int left, int top, int right, int bottom, ScalarBinding scalarBinding)
            {
                Binding.VerifyAdding(scalarBinding, nameof(scalarBinding));
                Template.AddBinding(Template.Range(left, top, right, bottom), scalarBinding);
                return this;
            }

            /// <summary>
            /// Adds block binding.
            /// </summary>
            /// <param name="column">Index of grid column.</param>
            /// <param name="row">Index of grid row.</param>
            /// <param name="blockBinding">The block binding.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder AddBinding(int column, int row, BlockBinding blockBinding)
            {
                return AddBinding(column, row, column, row, blockBinding);
            }

            /// <summary>
            /// Adds block binding.
            /// </summary>
            /// <param name="left">Index of left grid column.</param>
            /// <param name="top">Index of top grid row.</param>
            /// <param name="right">Index of right grid column.</param>
            /// <param name="bottom">Index of bottom grid row.</param>
            /// <param name="blockBinding">The block binding.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder AddBinding(int left, int top, int right, int bottom, BlockBinding blockBinding)
            {
                Binding.VerifyAdding(blockBinding, nameof(blockBinding));
                Template.AddBinding(Template.Range(left, top, right, bottom), blockBinding);
                return this;
            }

            /// <summary>
            /// Adds row binding.
            /// </summary>
            /// <param name="column">Index of grid column.</param>
            /// <param name="row">Index of grid row.</param>
            /// <param name="rowBinding">The row binding.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder AddBinding(int column, int row, RowBinding rowBinding)
            {
                return AddBinding(column, row, column, row, rowBinding);
            }

            /// <summary>
            /// Adds row binding.
            /// </summary>
            /// <param name="left">Index of left grid column.</param>
            /// <param name="top">Index of top grid row.</param>
            /// <param name="right">Index of right grid column.</param>
            /// <param name="bottom">Index of bottom grid row.</param>
            /// <param name="rowBinding">The row binding.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder AddBinding(int left, int top, int right, int bottom, RowBinding rowBinding)
            {
                Binding.VerifyAdding(rowBinding, nameof(rowBinding));
                Template.AddBinding(Template.Range(left, top, right, bottom), rowBinding);
                return this;
            }

            /// <summary>
            /// Defines horizontal grid line.
            /// </summary>
            /// <param name="startGridPoint">The start point of the grid line.</param>
            /// <param name="length">The length of the grid line.</param>
            /// <param name="pen">The pen used to draw the grid line.</param>
            /// <param name="placement">The placement of the grid line.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder GridLineX(GridPoint startGridPoint, int length, Pen pen = null, GridPlacement? placement = null)
            {
                if (_inherited)
                    return this;

                return GridLine(Orientation.Horizontal, startGridPoint, length, pen, placement);
            }

            /// <summary>
            /// Defines vertical grid line.
            /// </summary>
            /// <param name="startGridPoint">The start point of the grid line.</param>
            /// <param name="length">The length of the grid line.</param>
            /// <param name="pen">The pen used to draw the grid line.</param>
            /// <param name="placement">The placement of the grid line.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder GridLineY(GridPoint startGridPoint, int length, Pen pen = null, GridPlacement? placement = null)
            {
                if (_inherited)
                    return this;

                return GridLine(Orientation.Vertical, startGridPoint, length, pen, placement);
            }

            private static readonly Pen DefaultGridLinePen = GetDefaultGridLinePen();
            private static Pen GetDefaultGridLinePen()
            {
                var result = new Pen(Brushes.LightGray, 1);
                result.Freeze();
                return result;
            }

            private TemplateBuilder GridLine(Orientation orientation, GridPoint startGridPoint, int length, Pen pen, GridPlacement? placement)
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

                int endGridPointX = startGridPoint.X;
                int endGridPointY = startGridPoint.Y;
                if (orientation == Orientation.Horizontal)
                {
                    endGridPointX += length;
                    if (endGridPointX > Template.GridColumns.Count)
                        throw new ArgumentOutOfRangeException(nameof(length));
                }
                else
                {
                    endGridPointY += length;
                    if (endGridPointY > Template.GridRows.Count)
                        throw new ArgumentOutOfRangeException(nameof(length));
                }

                var gridLine = new GridLine(startGridPoint, new GridPoint(endGridPointX, endGridPointY), pen, placement);
                Template.AddGridLine(gridLine);
                return this;
            }

            /// <summary>
            /// Adds block view behavior.
            /// </summary>
            /// <param name="behavior">The block view behavior.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder AddBehavior(BlockViewBehavior behavior)
            {
                if (_inherited)
                    return this;

                if (behavior == null)
                    throw new ArgumentNullException(nameof(behavior));

                Template.AddBehavior(behavior);
                return this;
            }

            /// <summary>
            /// Adds row view behavior.
            /// </summary>
            /// <param name="behavior">The row view behavior.</param>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder AddBehavior(RowViewBehavior behavior)
            {
                if (_inherited)
                    return this;

                if (behavior == null)
                    throw new ArgumentNullException(nameof(behavior));

                Template.AddBehavior(behavior);
                return this;
            }

            /// <summary>
            /// Allows data deletion operation.
            /// </summary>
            /// <returns>This template builder for fluent coding.</returns>
            public TemplateBuilder AllowDelete()
            {
                Template.AllowsDelete = true;
                return this;
            }
        }
    }
}
