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
        /// This GridItem has its Owner property set already.
        /// </summary>
        public static string GridItem_OwnerAlreadySet
        {
            get { return GetString("GridItem_OwnerAlreadySet"); }
        }

        /// <summary>
        /// The GridItem is sealed and cannot be changed.
        /// </summary>
        public static string GridItem_VerifyNotSealed
        {
            get { return GetString("GridItem_VerifyNotSealed"); }
        }

        /// <summary>
        /// The input string "{input}" is invalid.
        /// </summary>
        public static string GridLengthParser_InvalidInput(object input)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("GridLengthParser_InvalidInput", "input"), input);
        }

        /// <summary>
        /// The GridRange does not belong to the same GridTemplate.
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
        /// Invalid GridColumn.Width/Orientation combination: GridColumns[{index}]="{gridColumnWidth}", Orientation="{orientation}".
        /// </summary>
        public static string GridTemplate_InvalidGridColumnWidthOrientation(object index, object gridColumnWidth, object orientation)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("GridTemplate_InvalidGridColumnWidthOrientation", "index", "gridColumnWidth", "orientation"), index, gridColumnWidth, orientation);
        }

        /// <summary>
        /// Invalid GridRow.Height/Orientation combination: GridRows[{index}]="{gridRowHeight}", Orientation="{orientation}".
        /// </summary>
        public static string GridTemplate_InvalidGridRowHeightOrientation(object index, object gridRowHeight, object orientation)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("GridTemplate_InvalidGridRowHeightOrientation", "index", "gridRowHeight", "orientation"), index, gridRowHeight, orientation);
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
