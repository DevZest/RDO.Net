using DevZest.Data.Annotations;
using DevZest.Data.AspNetCore.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DevZest.Data.AspNetCore.TagHelpers
{
    public class InputTagHelperTests
    {
        private class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Text);
                RegisterChildModel((TestModel _) => _.NestedModel);
                RegisterColumn((TestModel _) => _.IsACar);
                RegisterColumn((TestModel _) => _.Date);
                RegisterColumn((TestModel _) => _.DateTime);
            }

            public TestModel()
            {
                Date.LogicalDataType = LogicalDataType.Date;
            }

            public _String Text { get; private set; }

            public NestedModel NestedModel { get; private set; }

            [Required]
            [DefaultValue(false)]
            public _Boolean IsACar { get; private set; }

            public _DateTime Date { get; private set; }

            public _DateTime DateTime { get; private set; }
        }

        private class NestedModel : Model
        {
            static NestedModel()
            {
                RegisterColumn((NestedModel _) => _.Text);
            }

            public _String Text { get; private set; }
        }


        private static InputTagHelper GetTagHelper(Column column, bool isScalar = true, DataRow dataRow = null, IDataSetHtmlGenerator generator = null, IModelMetadataProvider metadataProvider = null)
        {
            if (generator == null)
                generator = Factory.GetDataSetHtmlGenerator();

            if (metadataProvider == null)
                metadataProvider = new TestModelMetadataProvider();

            var dataSet = column.GetParent().DataSet;
            DataSetContainer container;
            if (isScalar)
                container = new ScalarDataSetContainer(dataSet);
            else
                container = new DataSetContainer(dataSet);
            var containerType = container.GetType();
            var containerMetadata = metadataProvider.GetMetadataForType(containerType);
            var containerExplorer = metadataProvider.GetModelExplorerForType(containerType, container);

            var propertyMetadata = metadataProvider.GetMetadataForProperty(containerType, nameof(container.DataSet));
            var modelExplorer = containerExplorer.GetExplorerForExpression(propertyMetadata, container.DataSet);

            var modelExpression = new ModelExpression(nameof(container.DataSet), modelExplorer);
            var viewContext = Factory.GetViewContext(container);
            var inputTagHelper = new InputTagHelper(generator)
            {
                DataSetFor = modelExpression,
                Column = column,
                DataRow = dataRow,
                ViewContext = viewContext,
            };

            inputTagHelper.Init(new TagHelperContext(allAttributes: new TagHelperAttributeList(), items: new Dictionary<object, object>(), uniqueId: string.Empty));

            return inputTagHelper;
        }

        public static TheoryData<TagHelperAttributeList, string> MultiAttributeCheckBoxData
        {
            get
            {
                // outputAttributes, expectedAttributeString
                return new TheoryData<TagHelperAttributeList, string>
                {
                    {
                        new TagHelperAttributeList
                        {
                            { "hello", "world" },
                            { "hello", "world2" }
                        },
                        "hello=\"HtmlEncode[[world]]\" hello=\"HtmlEncode[[world2]]\""
                    },
                    {
                        new TagHelperAttributeList
                        {
                            { "hello", "world" },
                            { "hello", "world2" },
                            { "hello", "world3" }
                        },
                        "hello=\"HtmlEncode[[world]]\" hello=\"HtmlEncode[[world2]]\" hello=\"HtmlEncode[[world3]]\""
                    },
                    {
                        new TagHelperAttributeList
                        {
                            { "HelLO", "world" },
                            { "HELLO", "world2" }
                        },
                        "HelLO=\"HtmlEncode[[world]]\" HELLO=\"HtmlEncode[[world2]]\""
                    },
                    {
                        new TagHelperAttributeList
                        {
                            { "Hello", "world" },
                            { "HELLO", "world2" },
                            { "hello", "world3" }
                        },
                        "Hello=\"HtmlEncode[[world]]\" HELLO=\"HtmlEncode[[world2]]\" hello=\"HtmlEncode[[world3]]\""
                    },
                    {
                        new TagHelperAttributeList
                        {
                            { "HeLlO", "world" },
                            { "hello", "world2" }
                        },
                        "HeLlO=\"HtmlEncode[[world]]\" hello=\"HtmlEncode[[world2]]\""
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(MultiAttributeCheckBoxData))]
        public async Task CheckBoxHandlesMultipleAttributesSameNameArePreserved(
            TagHelperAttributeList outputAttributes,
            string expectedAttributeString)
        {
            // Arrange
            var originalContent = "original content";
            var expectedContent = $"<input {expectedAttributeString} type=\"HtmlEncode[[checkbox]]\" data-val=\"HtmlEncode[[true]]\" " +
                "data-val-required=\"HtmlEncode[[Value is required for field 'IsACar'.]]\" id=\"HtmlEncode[[DataSet_IsACar]]\" " +
                $"name=\"HtmlEncode[[DataSet.IsACar]]\" value=\"HtmlEncode[[true]]\" />" +
                "<input name=\"HtmlEncode[[DataSet.IsACar]]\" type=\"HtmlEncode[[hidden]]\" value=\"HtmlEncode[[false]]\" />";

            var context = new TagHelperContext(
                tagName: "input",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                "input",
                outputAttributes,
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(result: null))
            {
                TagMode = TagMode.SelfClosing,
            };

            output.Content.AppendHtml(originalContent);
            var dataSet = DataSet<TestModel>.Create();
            var tagHelper = GetTagHelper(dataSet._.IsACar);

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            Assert.NotNull(output.PostElement);
            Assert.Equal(originalContent, HtmlContentUtilities.HtmlContentToString(output.Content));
            Assert.Equal(expectedContent, HtmlContentUtilities.HtmlContentToString(output));
        }

        [Theory]
        [InlineData("hidden")]
        [InlineData("number")]
        [InlineData("text")]
        public void Process_WithInputTypeName(string inputTypeName)
        {
            // Arrange
            var expectedTagName = "input";
            var attributes = new TagHelperAttributeList
            {
                { "type", inputTypeName }
            };

            var context = new TagHelperContext(attributes, new Dictionary<object, object>(), "test");
            var output = new TagHelperOutput(
                expectedTagName,
                new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(result: null))
            {
                TagMode = TagMode.SelfClosing,
            };

            var dataSet = DataSet<TestModel>.Create();
            var tagHelper = GetTagHelper(dataSet._.IsACar);
            tagHelper.ViewContext.ClientValidationEnabled = false;
            tagHelper.InputTypeName = inputTypeName;

            // Act
            tagHelper.Process(context, output);

            // Assert
            var expectedAttributes = new TagHelperAttributeList
            {
                { "type", inputTypeName },
                { "id", "DataSet_IsACar" },
                { "name", "DataSet.IsACar" },
                { "value", "" }
            };

            Assert.Equal(expectedAttributes, output.Attributes);
            Assert.Equal(expectedTagName, output.TagName);
        }

        [Theory]
        [InlineData("datetime", "datetime")]
        [InlineData(null, "datetime-local")]
        [InlineData("hidden", "hidden")]
        public void Process_GeneratesFormattedOutput_ForDateTime(string specifiedType, string expectedType)
        {
            // Arrange
            var expectedTagName = "not-input";

            var allAttributes = new TagHelperAttributeList
            {
                { "type", specifiedType },
            };
            var context = new TagHelperContext(allAttributes, new Dictionary<object, object>(), "test");
            var output = new TagHelperOutput(
                expectedTagName,
                new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(result: null))
            {
                TagMode = TagMode.SelfClosing,
            };

            var dataSet = DataSet<TestModel>.Create();
            dataSet.AddRow();
            dataSet._.DateTime[0] = new DateTime(2011, 8, 31, hour: 5, minute: 30, second: 45, kind: DateTimeKind.Utc);
            var tagHelper = GetTagHelper(dataSet._.DateTime);
            tagHelper.Format = "datetime: {0:o}";
            tagHelper.InputTypeName = specifiedType;

            // Act
            tagHelper.Process(context, output);

            // Assert
            var expectedAttributes = new TagHelperAttributeList
            {
                { "type", expectedType },
                { "id", "DataSet_DateTime" },
                { "name", "DataSet.DateTime" },
                { "value", "datetime: 2011-08-31T05:30:45.0000000Z" },
            };
            Assert.Equal(expectedAttributes, output.Attributes);
            Assert.Empty(output.PreContent.GetContent());
            Assert.Empty(output.Content.GetContent());
            Assert.Empty(output.PostContent.GetContent());
            Assert.Equal(expectedTagName, output.TagName);
        }

        [Fact]
        public async Task ProcessAsync_CallsGenerateCheckBox_WithExpectedParameters()
        {
            // Arrange
            var originalContent = "original content";
            var expectedPreContent = "original pre-content";
            var expectedContent = "<input class=\"HtmlEncode[[form-control]]\" type=\"HtmlEncode[[checkbox]]\" /><hidden />";
            var expectedPostContent = "original post-content";
            var expectedPostElement = "<hidden />";

            var context = new TagHelperContext(
                tagName: "input",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var originalAttributes = new TagHelperAttributeList
            {
                { "class", "form-control" },
            };
            var output = new TagHelperOutput(
                "input",
                originalAttributes,
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                })
            {
                TagMode = TagMode.SelfClosing,
            };
            output.PreContent.AppendHtml(expectedPreContent);
            output.Content.AppendHtml(originalContent);
            output.PostContent.AppendHtml(expectedPostContent);

            var generator = new Mock<IDataSetHtmlGenerator>(MockBehavior.Strict);
            var dataSet = DataSet<TestModel>.Create();
            dataSet.AddRow();
            dataSet._.IsACar[0] = false;
            var tagHelper = GetTagHelper(dataSet._.IsACar, generator: generator.Object);
            tagHelper.Format = "somewhat-less-null"; // ignored

            var tagBuilder = new TagBuilder("input")
            {
                TagRenderMode = TagRenderMode.SelfClosing
            };
            generator
                .Setup(mock => mock.GenerateCheckBox(
                    tagHelper.ViewContext,
                    tagHelper.FullHtmlFieldName,
                    tagHelper.Column,
                    false,                   // isChecked
                    It.IsAny<object>()))    // htmlAttributes
                .Returns(tagBuilder)
                .Verifiable();
            generator
                .Setup(mock => mock.GenerateHiddenForCheckbox(
                    tagHelper.ViewContext,
                    tagHelper.FullHtmlFieldName))
                .Returns(new TagBuilder("hidden") { TagRenderMode = TagRenderMode.SelfClosing })
                .Verifiable();

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            generator.Verify();

            Assert.NotEmpty(output.Attributes);
            Assert.Equal(expectedPreContent, output.PreContent.GetContent());
            Assert.Equal(originalContent, HtmlContentUtilities.HtmlContentToString(output.Content));
            Assert.Equal(expectedContent, HtmlContentUtilities.HtmlContentToString(output));
            Assert.Equal(expectedPostContent, output.PostContent.GetContent());
            Assert.Equal(expectedPostElement, output.PostElement.GetContent());
            Assert.Equal(TagMode.SelfClosing, output.TagMode);
        }
    }
}
