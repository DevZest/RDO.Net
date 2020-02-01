using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    public class ModelBindingHelperResourceTests
    {
        [Fact]
        public void FormatInvalid_IncludePropertyExpression()
        {
            Assert.NotNull(ModelBindingHelper.Resources.FormatInvalid_IncludePropertyExpression("p0"));
        }

        [Fact]
        public void FormatValueProviderResult_NoConverterExists()
        {
            Assert.NotNull(ModelBindingHelper.Resources.FormatValueProviderResult_NoConverterExists("p0", "p1"));
        }
    }
}
