using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO;

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

        private static ViewContext GetViewContext<TModel>(TModel model)
        {
            return GetViewContext(model, new TestModelMetadataProvider());
        }

        private static ViewContext GetViewContext<TModel>(TModel model, IModelMetadataProvider metadataProvider)
        {
            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            var viewData = new ViewDataDictionary<TModel>(metadataProvider, actionContext.ModelState)
            {
                Model = model,
            };

            return new ViewContext(
                actionContext,
                Mock.Of<IView>(),
                viewData,
                Mock.Of<ITempDataDictionary>(),
                TextWriter.Null,
                new HtmlHelperOptions());
        }
    }
}
