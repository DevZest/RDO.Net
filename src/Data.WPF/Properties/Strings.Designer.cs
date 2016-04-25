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
        /// BlockItems[{index}] intersects with RowRange.
        /// </summary>
        public static string BlockItem_IntersectsWithRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("BlockItem_IntersectsWithRowRange", "index"), index);
        }

        /// <summary>
        /// BlockItem is invalid when Template.Orientation is null.
        /// </summary>
        public static string BlockItem_NullOrientation
        {
            get { return GetString("BlockItem_NullOrientation"); }
        }

        /// <summary>
        /// BlockItems[{index}] is out of horizontal side of RowRange.
        /// </summary>
        public static string BlockItem_OutOfHorizontalRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("BlockItem_OutOfHorizontalRowRange", "index"), index);
        }

        /// <summary>
        /// BlockItems[{index}] is out of vertical side of RowRange.
        /// </summary>
        public static string BlockItem_OutOfVerticalRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("BlockItem_OutOfVerticalRowRange", "index"), index);
        }

        /// <summary>
        /// DataElementPanel must be element of DataView's control template to implement IScrollInfo.
        /// </summary>
        public static string DataElementPanel_NullScrollHandler
        {
            get { return GetString("DataElementPanel_NullScrollHandler"); }
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
        /// The GridRange is empty.
        /// </summary>
        public static string GridRange_VerifyNotEmpty
        {
            get { return GetString("GridRange_VerifyNotEmpty"); }
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
        /// RowItems[{index}] is out of the RowRange.
        /// </summary>
        public static string RowItem_OutOfRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("RowItem_OutOfRowRange", "index"), index);
        }

        /// <summary>
        /// The value is invalid. It does not belong to this RowManager.
        /// </summary>
        public static string RowManager_InvalidCurrentRow
        {
            get { return GetString("RowManager_InvalidCurrentRow"); }
        }

        /// <summary>
        /// The specified ordinal must be a top level row.
        /// </summary>
        public static string RowManager_OrdinalNotTopLevel
        {
            get { return GetString("RowManager_OrdinalNotTopLevel"); }
        }

        /// <summary>
        /// The EOF row cannot be deleted.
        /// </summary>
        public static string RowPresenter_DeleteEof
        {
            get { return GetString("RowPresenter_DeleteEof"); }
        }

        /// <summary>
        /// The SubviewItem is invalid.
        /// </summary>
        public static string RowPresenter_InvalidSubviewItem
        {
            get { return GetString("RowPresenter_InvalidSubviewItem"); }
        }

        /// <summary>
        /// The column is invalid. It does not belong to the DataSet.
        /// </summary>
        public static string RowPresenter_VerifyColumn
        {
            get { return GetString("RowPresenter_VerifyColumn"); }
        }

        /// <summary>
        /// The row must be hierarchical.
        /// </summary>
        public static string RowPresenter_VerifyHierarchical
        {
            get { return GetString("RowPresenter_VerifyHierarchical"); }
        }

        /// <summary>
        /// ScalarItems[{index}] intersects with RowRange.
        /// </summary>
        public static string ScalarItem_IntersectsWithRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ScalarItem_IntersectsWithRowRange", "index"), index);
        }

        /// <summary>
        /// Multidimensional ScalarItems[{index}] conflicts with one dimensional Template (Template.BlockDimensions=1).
        /// </summary>
        public static string ScalarItem_OneDimensionalTemplate(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ScalarItem_OneDimensionalTemplate", "index"), index);
        }

        /// <summary>
        /// Multidimensional ScalarItems[{index}] is out of horizontal side of RowRange.
        /// </summary>
        public static string ScalarItem_OutOfHorizontalRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ScalarItem_OutOfHorizontalRowRange", "index"), index);
        }

        /// <summary>
        /// Multidimensional ScalarItems[{index}] is out of vertical side of RowRange.
        /// </summary>
        public static string ScalarItem_OutOfVerticalRowRange(object index)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ScalarItem_OutOfVerticalRowRange", "index"), index);
        }

        /// <summary>
        /// The child model is invalid. It must be direct child model and has the same type.
        /// </summary>
        public static string TemplateBuilder_InvalidFlattenHierarchyChildModel
        {
            get { return GetString("TemplateBuilder_InvalidFlattenHierarchyChildModel"); }
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
