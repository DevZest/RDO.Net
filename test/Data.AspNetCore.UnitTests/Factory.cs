using DevZest.Data.AspNetCore.Primitives;
using DevZest.Data.AspNetCore.TagHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;

namespace DevZest.Data.AspNetCore
{
    internal static class Factory
    {
        public static T GetTagHelper<T>(Column column, bool isScalar, DataRow dataRow, IDataSetHtmlGenerator generator, IModelMetadataProvider metadataProvider,
            Func<IDataSetHtmlGenerator, T> create)
            where T : DataSetTagHelperBase
        {
            if (generator == null)
                generator = GetDataSetHtmlGenerator();

            if (metadataProvider == null)
                metadataProvider = new TestModelMetadataProvider();

            var container = column.GetContainer(isScalar);
            var containerType = container.GetType();
            var containerMetadata = metadataProvider.GetMetadataForType(containerType);
            var containerExplorer = metadataProvider.GetModelExplorerForType(containerType, container);

            var propertyMetadata = metadataProvider.GetMetadataForProperty(containerType, nameof(container.DataSet));
            var modelExplorer = containerExplorer.GetExplorerForExpression(propertyMetadata, container.DataSet);

            var modelExpression = new ModelExpression(nameof(container.DataSet), modelExplorer);
            var viewContext = Factory.GetViewContext(container);
            var result = create(generator);
            result.DataSetFor = modelExpression;
            result.Column = column;
            result.DataRow = dataRow;
            result.ViewContext = viewContext;

            result.Init(new TagHelperContext(allAttributes: new TagHelperAttributeList(), items: new Dictionary<object, object>(), uniqueId: string.Empty));

            return result;
        }

        private static DataSetContainer GetContainer(this Column column, bool isScalar)
        {
            var dataSet = column.GetParent().GetRootModel().DataSet;
            if (isScalar)
                return new ScalarDataSetContainer(dataSet);
            else
                return new DataSetContainer(dataSet);
        }

        private static Model GetRootModel(this Model model)
        {
            var parentModel = model.GetParent();
            return parentModel == null ? model : parentModel.GetRootModel();
        }

        public static IDataSetHtmlGenerator GetDataSetHtmlGenerator(Action<DataSetMvcConfiguration> configurationExpression = null)
        {
            var mvcViewOptionsAccessor = new Mock<IOptions<MvcViewOptions>>();
            mvcViewOptionsAccessor.SetupGet(accessor => accessor.Value).Returns(new MvcViewOptions());

            var config = new DataSetMvcConfiguration();
            config.AddClientValidators(new DefaultModelBindingMessageProvider());
            configurationExpression?.Invoke(config);
            var attributeProvider = new DefaultDataSetValidationHtmlAttributeProvider(config);

            return new DefaultDataSetHtmlGenerator(mvcViewOptionsAccessor.Object, attributeProvider);
        }

        public static ViewContext GetViewContext<TModel>(TModel model)
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
