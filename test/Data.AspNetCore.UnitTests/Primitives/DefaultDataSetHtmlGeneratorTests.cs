using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;
using Moq;

namespace DevZest.Data.AspNetCore.Primitives
{
    public class DefaultDataSetHtmlGeneratorTests
    {
        private static IDataSetHtmlGenerator GetGenerator()
        {
            var mvcViewOptionsAccessor = new Mock<IOptions<MvcViewOptions>>();
            mvcViewOptionsAccessor.SetupGet(accessor => accessor.Value).Returns(new MvcViewOptions() { AllowRenderingMaxLengthAttribute = true });

            var config = new DataSetMvcConfiguration();
            config.AddClientValidators(new DefaultModelBindingMessageProvider());
            var attributeProvider = new DefaultDataSetValidationHtmlAttributeProvider(config);

            return new DefaultDataSetHtmlGenerator(mvcViewOptionsAccessor.Object, attributeProvider);
        }

    }
}
