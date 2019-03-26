using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;
using Moq;
using System;

namespace DevZest.Data.AspNetCore.Primitives
{
    public class DefaultDataSetHtmlGeneratorTests
    {
        private static IDataSetHtmlGenerator GetGenerator(Action<DataSetMvcConfiguration> configurationExpression = null)
        {
            var mvcViewOptionsAccessor = new Mock<IOptions<MvcViewOptions>>();
            mvcViewOptionsAccessor.SetupGet(accessor => accessor.Value).Returns(new MvcViewOptions() { AllowRenderingMaxLengthAttribute = true });

            var config = new DataSetMvcConfiguration();
            config.AddClientValidators(new DefaultModelBindingMessageProvider());
            configurationExpression?.Invoke(config);
            var attributeProvider = new DefaultDataSetValidationHtmlAttributeProvider(config);

            return new DefaultDataSetHtmlGenerator(mvcViewOptionsAccessor.Object, attributeProvider);
        }

    }
}
