using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Xps.Serialization;

namespace DevZest.Data.Windows.Utilities
{
    internal static class UIElementExtensions
    {
        /// <summary>
        /// Render a UIElement such that the visual tree is generated, 
        /// without actually displaying the UIElement
        /// anywhere
        /// </summary>
        public static void CreateVisualTree(this UIElement element)
        {
            if (LogicalTreeHelper.GetParent(element) != null)
                return;

            var fixedDoc = new FixedDocument();
            var pageContent = new PageContent();
            var fixedPage = new FixedPage();
            fixedPage.Children.Add(element);
            ((IAddChild)pageContent).AddChild(fixedPage);
            fixedDoc.Pages.Add(pageContent);

            var f = new XpsSerializerFactory();
            var w = f.CreateSerializerWriter(new MemoryStream());
            w.Write(fixedDoc);
        }
    }
}
