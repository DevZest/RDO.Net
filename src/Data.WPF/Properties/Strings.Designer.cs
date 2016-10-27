// <auto-generated />
namespace DevZest.Data.Windows
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    internal static class Strings
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("DevZest.Data.Windows.Strings", typeof(Strings).GetTypeInfo().Assembly);

        /// <summary>
        /// The {frozen} value is invalid. It cuts across {bindings}[{ordinal}].
        /// </summary>
        public static string Binding_InvalidFrozenMargin(object frozen, object bindings, object ordinal)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("Binding_InvalidFrozenMargin", "frozen", "bindings", "ordinal"), frozen, bindings, ordinal);
        }

        /// <summary>
        /// The Binding is sealed and allows no modification.
        /// </summary>
        public static string Binding_VerifyNotSealed
        {
            get { return GetString("Binding_VerifyNotSealed"); }
        }

        /// <summary>
        /// BlockBindings[{index}] intersects with RowRange.
        /// </summary>
        public static string BlockBinding_IntersectsWithRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("BlockBinding_IntersectsWithRowRange", "index"), index);
        }

        /// <summary>
        /// BlockBinding is invalid when Template.Orientation is null.
        /// </summary>
        public static string BlockBinding_NullOrientation
        {
            get { return GetString("BlockBinding_NullOrientation"); }
        }

        /// <summary>
        /// BlockBindings[{index}] is out of horizontal side of RowRange.
        /// </summary>
        public static string BlockBinding_OutOfHorizontalRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("BlockBinding_OutOfHorizontalRowRange", "index"), index);
        }

        /// <summary>
        /// BlockBindings[{index}] is out of vertical side of RowRange.
        /// </summary>
        public static string BlockBinding_OutOfVerticalRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("BlockBinding_OutOfVerticalRowRange", "index"), index);
        }

        /// <summary>
        /// DataElementPanel must be element of DataView's control template to implement IScrollInfo.
        /// </summary>
        public static string DataElementPanel_NullScrollHandler
        {
            get { return GetString("DataElementPanel_NullScrollHandler"); }
        }

        /// <summary>
        /// The row is invalid.
        /// </summary>
        public static string DataPresenter_InvalidRow
        {
            get { return GetString("DataPresenter_InvalidRow"); }
        }

        /// <summary>
        /// The DataPresenter is not initialized: the DataSet property is null.
        /// </summary>
        public static string DataPresenter_NullDataSet
        {
            get { return GetString("DataPresenter_NullDataSet"); }
        }

        /// <summary>
        /// The CanInsert property must be true.
        /// </summary>
        public static string DataPresenter_VerifyCanInsert
        {
            get { return GetString("DataPresenter_VerifyCanInsert"); }
        }

        /// <summary>
        /// Auto width GridColumns[{ordinal}] is invalid for multidimensional layout.
        /// </summary>
        public static string GridColumn_InvalidAutoWidth(object ordinal)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("GridColumn_InvalidAutoWidth", "ordinal"), ordinal);
        }

        /// <summary>
        /// Star width GridColumns[{ordinal}] is invalid for horizontal or multidimensional layout.
        /// </summary>
        public static string GridColumn_InvalidStarWidth(object ordinal)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("GridColumn_InvalidStarWidth", "ordinal"), ordinal);
        }

        /// <summary>
        /// The input string "{input}" is invalid.
        /// </summary>
        public static string GridLengthParser_InvalidInput(object input)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("GridLengthParser_InvalidInput", "input"), input);
        }

        /// <summary>
        /// The GridRange does not belong to the same Template.
        /// </summary>
        public static string GridRange_InvalidOwner
        {
            get { return GetString("GridRange_InvalidOwner"); }
        }

        /// <summary>
        /// Auto height GridRows[{ordinal}] is invalid for multidimensional layout.
        /// </summary>
        public static string GridRow_InvalidAutoHeight(object ordinal)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("GridRow_InvalidAutoHeight", "ordinal"), ordinal);
        }

        /// <summary>
        /// Star height GridRows[{ordinal}] is invalid for vertical or multidemensional layout.
        /// </summary>
        public static string GridRow_InvalidStarHeight(object ordinal)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("GridRow_InvalidStarHeight", "ordinal"), ordinal);
        }

        /// <summary>
        /// The Input has already been assigned to RowBinding.
        /// </summary>
        public static string RowBindingBase_InputAlreadyAssigned
        {
            get { return GetString("RowBindingBase_InputAlreadyAssigned"); }
        }

        /// <summary>
        /// RowBindings[{index}] is out of the RowRange.
        /// </summary>
        public static string RowBinding_OutOfRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("RowBinding_OutOfRowRange", "index"), index);
        }

        /// <summary>
        /// The placeholder row cannot be deleted.
        /// </summary>
        public static string RowPresenter_DeletePlaceholder
        {
            get { return GetString("RowPresenter_DeletePlaceholder"); }
        }

        /// <summary>
        /// The child row is invalid.
        /// </summary>
        public static string RowPresenter_InvalidChildRow
        {
            get { return GetString("RowPresenter_InvalidChildRow"); }
        }

        /// <summary>
        /// The column is invalid. It does not belong to the DataSet.
        /// </summary>
        public static string RowPresenter_VerifyColumn
        {
            get { return GetString("RowPresenter_VerifyColumn"); }
        }

        /// <summary>
        /// RowPresenter.IsCurrent must be true to allow this operation.
        /// </summary>
        public static string RowPresenter_VerifyIsCurrent
        {
            get { return GetString("RowPresenter_VerifyIsCurrent"); }
        }

        /// <summary>
        /// The IsEditing property must be true.
        /// </summary>
        public static string RowPresenter_VerifyIsEditing
        {
            get { return GetString("RowPresenter_VerifyIsEditing"); }
        }

        /// <summary>
        /// There is pending edit not completed.
        /// </summary>
        public static string RowPresenter_VerifyNoPendingEdit
        {
            get { return GetString("RowPresenter_VerifyNoPendingEdit"); }
        }

        /// <summary>
        /// The row must be recursive.
        /// </summary>
        public static string RowPresenter_VerifyRecursive
        {
            get { return GetString("RowPresenter_VerifyRecursive"); }
        }

        /// <summary>
        /// ScalarBindings[{index}] intersects with RowRange.
        /// </summary>
        public static string ScalarBinding_IntersectsWithRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ScalarBinding_IntersectsWithRowRange", "index"), index);
        }

        /// <summary>
        /// The Stretches value is invalid. It cuts across ScalarBindings[{ordinal}].
        /// </summary>
        public static string ScalarBinding_InvalidStretches(object ordinal)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ScalarBinding_InvalidStretches", "ordinal"), ordinal);
        }

        /// <summary>
        /// Multidimensional ScalarBindings[{index}] conflicts with one dimensional Template (Template.BlockDimensions=1).
        /// </summary>
        public static string ScalarBinding_OneDimensionalTemplate(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ScalarBinding_OneDimensionalTemplate", "index"), index);
        }

        /// <summary>
        /// Multidimensional ScalarBindings[{index}] is out of horizontal side of RowRange.
        /// </summary>
        public static string ScalarBinding_OutOfHorizontalRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ScalarBinding_OutOfHorizontalRowRange", "index"), index);
        }

        /// <summary>
        /// Multidimensional ScalarBindings[{index}] is out of vertical side of RowRange.
        /// </summary>
        public static string ScalarBinding_OutOfVerticalRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ScalarBinding_OutOfVerticalRowRange", "index"), index);
        }

        /// <summary>
        /// The child model is invalid. It must be direct child model and has the same type.
        /// </summary>
        public static string TemplateBuilder_InvalidRecursiveChildModel
        {
            get { return GetString("TemplateBuilder_InvalidRecursiveChildModel"); }
        }

        /// <summary>
        /// The RowRange cannot be empty.
        /// </summary>
        public static string Template_EmptyRowRange
        {
            get { return GetString("Template_EmptyRowRange"); }
        }

        /// <summary>
        /// The {frozen} value is invalid. RowRange is always scrollable and cannot be frozen.
        /// </summary>
        public static string Template_InvalidFrozenMargin(object frozen)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("Template_InvalidFrozenMargin", "frozen"), frozen);
        }

        /// <summary>
        /// The Stretches value must be less or equal to {frozen}.
        /// </summary>
        public static string Template_InvalidStretches(object frozen)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("Template_InvalidStretches", "frozen"), frozen);
        }

        private static string GetString(string name, params string[] formatterNames)
        {
            var value = _resourceManager.GetString(name);

            Debug.Assert(value != null);

            if (formatterNames != null)
            {
                for (var i = 0; i < formatterNames.Length; i++)
                {
                    value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
                }
            }

            return value;
        }
    }
}
