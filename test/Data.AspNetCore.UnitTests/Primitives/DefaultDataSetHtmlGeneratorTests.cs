using DevZest.Data.Annotations;
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
using System.Collections.Generic;
using System.IO;
using Xunit;

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

        public enum RegularEnum
        {
            Zero,
            One,
            Two,
            Three,
        }

        [Flags]
        public enum FlagsEnum
        {
            None = 0,
            One = 1,
            Two = 2,
            Four = 4,
            FortyTwo = 42,
            All = -1,
        }

        private class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Id);
                RegisterColumn((TestModel _) => _.Name);
                RegisterColumn((TestModel _) => _.RegularEnum);
                RegisterColumn((TestModel _) => _.FlagsEnum);
            }

            public _Int32 Id { get; private set; }

            public _String Name { get; private set; }

            public _ByteEnum<RegularEnum> RegularEnum { get; private set; }

            public _ByteEnum<FlagsEnum> FlagsEnum { get; private set; }
        }

        private class ModelWithMaxLength : Model
        {
            static ModelWithMaxLength()
            {
                RegisterColumn((ModelWithMaxLength _) => _.ColumnWithMaxLength);
                RegisterColumn((ModelWithMaxLength _) => _.ColumnWithStringLength);
                RegisterColumn((ModelWithMaxLength _) => _.ColumnWithoutAttributes);
            }

            internal const int MaxLengthAttributeValue = 77;
            internal const int StringLengthAttributeValue = 7;

            [MaxLength(MaxLengthAttributeValue)]
            public _Binary ColumnWithMaxLength { get; private set; }

            [StringLength(StringLengthAttributeValue)]
            public _String ColumnWithStringLength { get; set; }

            public _String ColumnWithoutAttributes { get; set; }
        }


        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void GetCurrentValues_WithNullDataValue_ReturnsNull(bool allowMultiple)
        {
            // Arrange
            var htmlGenerator = GetGenerator();
            var viewContext = GetViewContext<Model>(model: null);
            var dataSet = DataSet<TestModel>.Create();

            // Act
            var result = htmlGenerator.GetCurrentValues(
                viewContext,
                dataSet._.Name,
                null,
                allowMultiple: allowMultiple);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(nameof(ModelWithMaxLength.ColumnWithMaxLength), ModelWithMaxLength.MaxLengthAttributeValue)]
        [InlineData(nameof(ModelWithMaxLength.ColumnWithStringLength), ModelWithMaxLength.StringLengthAttributeValue)]
        public void GenerateTextArea_RendersMaxLength(string columnName, int expectedValue)
        {
            // Arrange
            var dataSet = DataSet<ModelWithMaxLength>.Create();
            var generator = GetGenerator();
            var viewContext = GetViewContext(dataSet);
            var column = dataSet._.GetColumns()[columnName];

            // Act
            var tagBuilder = generator.GenerateTextArea(viewContext, nameof(dataSet), column, dataValue: null, rows: 1, columns: 1, htmlAttributes: null);

            // Assert
            var attribute = Assert.Single(tagBuilder.Attributes, a => a.Key == "maxlength");
            Assert.Equal(expectedValue, Int32.Parse(attribute.Value));
        }

        [Theory]
        [InlineData(nameof(ModelWithMaxLength.ColumnWithMaxLength), ModelWithMaxLength.MaxLengthAttributeValue)]
        [InlineData(nameof(ModelWithMaxLength.ColumnWithStringLength), ModelWithMaxLength.StringLengthAttributeValue)]
        public void GeneratePassword_RendersMaxLength(string columnName, int expectedValue)
        {
            // Arrange
            var dataSet = DataSet<ModelWithMaxLength>.Create();
            var generator = GetGenerator();
            var viewContext = GetViewContext(dataSet);
            var column = dataSet._.GetColumns()[columnName];

            // Act
            var tagBuilder = generator.GeneratePassword(viewContext, nameof(dataSet), column, value: null, htmlAttributes: null);

            // Assert
            var attribute = Assert.Single(tagBuilder.Attributes, a => a.Key == "maxlength");
            Assert.Equal(expectedValue, Int32.Parse(attribute.Value));
        }

        [Theory]
        [InlineData(nameof(ModelWithMaxLength.ColumnWithMaxLength), ModelWithMaxLength.MaxLengthAttributeValue)]
        [InlineData(nameof(ModelWithMaxLength.ColumnWithStringLength), ModelWithMaxLength.StringLengthAttributeValue)]
        public void GenerateTextBox_RendersMaxLength(string columnName, int expectedValue)
        {
            // Arrange
            var dataSet = DataSet<ModelWithMaxLength>.Create();
            var generator = GetGenerator();
            var viewContext = GetViewContext(dataSet);
            var column = dataSet._.GetColumns()[columnName];

            // Act
            var tagBuilder = generator.GenerateTextBox(viewContext, nameof(dataSet), column, value: null, format: null, htmlAttributes: null);

            // Assert
            var attribute = Assert.Single(tagBuilder.Attributes, a => a.Key == "maxlength");
            Assert.Equal(expectedValue, Int32.Parse(attribute.Value));
        }

        [Fact]
        public void GenerateTextBox_DoesNotRenderMaxLength_WhenNoAttributesPresent()
        {
            // Arrange
            var dataSet = DataSet<ModelWithMaxLength>.Create();
            var generator = GetGenerator();
            var viewContext = GetViewContext(dataSet);
            var column = dataSet._.ColumnWithoutAttributes;

            // Act
            var tagBuilder = generator.GenerateTextBox(viewContext, nameof(dataSet), column, value: null, format: null, htmlAttributes: null);

            // Assert
            Assert.DoesNotContain(tagBuilder.Attributes, a => a.Key == "maxlength");
        }

        [Theory]
        [InlineData(nameof(ModelWithMaxLength.ColumnWithMaxLength), ModelWithMaxLength.MaxLengthAttributeValue)]
        [InlineData(nameof(ModelWithMaxLength.ColumnWithStringLength), ModelWithMaxLength.StringLengthAttributeValue)]
        public void GenerateTextBox_SearchType_RendersMaxLength(string columnName, int expectedValue)
        {
            // Arrange
            var dataSet = DataSet<ModelWithMaxLength>.Create();
            var generator = GetGenerator();
            var viewContext = GetViewContext(dataSet);
            var column = dataSet._.GetColumns()[columnName];
            var htmlAttributes = new Dictionary<string, object>
            {
                { "type", "search"}
            };

            // Act
            var tagBuilder = generator.GenerateTextBox(viewContext, nameof(dataSet), column, value: null, format: null, htmlAttributes);

            // Assert
            var attribute = Assert.Single(tagBuilder.Attributes, a => a.Key == "maxlength");
            Assert.Equal(expectedValue, Int32.Parse(attribute.Value));
        }

        [Theory]
        [InlineData(nameof(ModelWithMaxLength.ColumnWithMaxLength))]
        [InlineData(nameof(ModelWithMaxLength.ColumnWithStringLength))]
        public void GenerateHidden_DoesNotRenderMaxLength(string columnName)
        {
            // Arrange
            var dataSet = DataSet<ModelWithMaxLength>.Create();
            var generator = GetGenerator();
            var viewContext = GetViewContext(dataSet);
            var column = dataSet._.GetColumns()[columnName];

            // Act
            var tagBuilder = generator.GenerateHidden(viewContext, nameof(dataSet), column, value: null, htmlAttributes: null);

            // Assert
            Assert.DoesNotContain(tagBuilder.Attributes, a => a.Key == "maxlength");
        }
    }
}
