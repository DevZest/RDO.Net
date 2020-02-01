using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Resources;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    partial class ModelBindingHelper
    {
        internal static class Resources
        {
            private static ResourceManager s_resourceManager;

            private static ResourceManager ResourceManager
            {
                get
                {
                    return s_resourceManager ?? (s_resourceManager = new ResourceManager("Microsoft.AspNetCore.Mvc.Core.Resources", typeof(BindingBehavior).Assembly));
                }
            }

            private static CultureInfo Culture
            {
                get { return CultureInfo.CurrentCulture; }
            }

            private static string GetResourceString(string resourceKey)
            {
                return ResourceManager.GetString(resourceKey, Culture);
            }

            internal static string FormatInvalid_IncludePropertyExpression(object p0)
            {
                return string.Format(Culture, GetResourceString("Invalid_IncludePropertyExpression"), p0);
            }

            internal static string FormatValueProviderResult_NoConverterExists(object p0, object p1)
            {
                return string.Format(Culture, GetResourceString("ValueProviderResult_NoConverterExists"), p0, p1);
            }
        }
    }
}
