using DevZest.Data.CodeAnalysis;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevZest.Data.Tools
{
    partial class PrimaryKeyWindow
    {
        private sealed class Presenter : DataPresenter<ModelMapper.PrimaryKeyEntry>
        {
            private readonly ModelMapper _modelMapper;
            private readonly Scalar<string> _pkTypeName;
            private readonly Scalar<bool> _shouldCreateKey;
            private readonly Scalar<string> _keyTypeName;
            private readonly Scalar<bool> _shouldCreateRef;
            private readonly Scalar<string> _refTypeName;

            public Presenter(ModelMapper modelMapper, PrimaryKeyWindow window)
            {
                _window = window;

                _modelMapper = modelMapper;
                _pkTypeName = NewScalar("PK").AddValidator(Extensions.ValidateRequired).AddValidator(modelMapper.ValidateIdentifier);
                _shouldCreateKey = NewScalar(true);
                _keyTypeName = NewScalar("Key");
                _shouldCreateRef = NewScalar(false);
                _refTypeName = NewScalar("Ref");

                Show(_window._dataView, modelMapper.CreatePrimaryKeyEntries());
            }

            private readonly PrimaryKeyWindow _window;

            protected override IScalarValidationErrors ValidateScalars(IScalarValidationErrors result)
            {
                result = TryAdd(result, Validate(_shouldCreateKey, _keyTypeName));
                result = TryAdd(result, Validate(_shouldCreateRef, _refTypeName));
                return result;
            }

            private ScalarValidationError Validate(Scalar<bool> shouldCreate, Scalar<string> typeName)
            {
                string error = null;
                if (shouldCreate.Value)
                {
                    var value = typeName.Value?.Trim();
                    error = value.ValidateRequired();
                    if (string.IsNullOrEmpty(error))
                        error = _modelMapper.ValidateIdentifier(value);
                }

                return string.IsNullOrEmpty(error) ? null : new ScalarValidationError(error, typeName);
            }

            private static IScalarValidationErrors TryAdd(IScalarValidationErrors result, ScalarValidationError error)
            {
                if (error != null)
                    result = result.Add(error);
                return result;
            }

            public ModelMapper ModelMapper
            {
                get { return _modelMapper; }
            }

            private IEnumerable Columns
            {
                get { return _modelMapper.GetColumns().Select(x => new { Value = x, Display = x.Name }); }
            }

            private IEnumerable SortDirections
            {
                get
                {
                    return new[]
                    {
                        new { Value = SortDirection.Unspecified, Display = string.Empty },
                        new { Value = SortDirection.Ascending, Display = UserMessages.SortDirection_Asc },
                        new { Value = SortDirection.Descending, Display = UserMessages.SortDirection_Desc }
                    };
                }
            }

            protected override void BuildTemplate(TemplateBuilder builder)
            {
                var columns = Columns;
                var sortDirections = SortDirections;
                builder.GridRows("Auto", "Auto")
                    .GridColumns("20", "*", "*", "100", "*")
                    .GridLineX(new GridPoint(1, 1), 4).GridLineX(new GridPoint(1, 2), 4)
                    .Layout(Orientation.Vertical)
                    .WithVirtualRowPlacement(VirtualRowPlacement.Tail)
                    .AddBinding(1, 0, this.BindToTextBlock(_.Column.DisplayName))
                    .AddBinding(2, 0, this.BindToTextBlock(_.Mounter.DisplayName))
                    .AddBinding(3, 0, this.BindToTextBlock(_.SortDirection.DisplayName))
                    .AddBinding(4, 0, this.BindToTextBlock(_.ConstructorParamName.DisplayName))
                    .AddBinding(0, 1, _.BindToRowHeader().WithStyle(RowHeader.Styles.Flat))
                    .AddBinding(1, 1, _.Column.BindToComboBox(columns))
                    .AddBinding(2, 1, _.Mounter.BindToTextBlock())
                    .AddBinding(3, 1, _.SortDirection.BindToComboBox(sortDirections).ApplyRefresh(RefreshIsEnabled))
                    .AddBinding(4, 1, _.ConstructorParamName.BindToTextBox().ApplyRefresh(RefreshIsEnabled))
                    .AddBinding(_window._textBoxName, _pkTypeName.BindToTextBox())
                    .AddBinding(_window._checkBoxKey, _shouldCreateKey.BindToCheckBox())
                    .AddBinding(_window._textBoxKey, _keyTypeName.BindToTextBox().ApplyRefresh((v, p) => v.IsEnabled = ShouldCreateKey))
                    .AddBinding(_window._checkBoxRef, _shouldCreateRef.BindToCheckBox())
                    .AddBinding(_window._textBoxRef, _refTypeName.BindToTextBox().ApplyRefresh((v, p) => v.IsEnabled = ShouldCreateRef));
            }

            protected override void OnEdit(Column column)
            {
                if (column == _.Column || column == _.SortDirection)
                    CurrentRow.EndEdit();
            }

            private void RefreshIsEnabled(UIElement v, RowPresenter p)
            {
                v.IsEnabled = p.GetValue(_.Column) != null;
            }

            private string PkTypeName
            {
                get { return _pkTypeName.Value?.Trim(); }
            }

            private bool ShouldCreateKey
            {
                get { return _shouldCreateKey.Value; }
            }

            private string KeyTypeName
            {
                get { return ShouldCreateKey ? _keyTypeName.Value?.Trim() : null; }
            }

            private bool ShouldCreateRef
            {
                get { return _shouldCreateRef.Value; }
            }

            private string RefTypeName
            {
                get { return ShouldCreateRef ? _refTypeName.Value?.Trim() : null; }
            }

            public void Execute(AddPrimaryKeyDelegate addPrimaryKey)
            {
                addPrimaryKey(PkTypeName, DataSet, KeyTypeName, RefTypeName);
            }
        }
    }
}
