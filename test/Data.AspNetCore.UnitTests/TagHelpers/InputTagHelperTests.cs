using DevZest.Data.Annotations;
using DevZest.Data.AspNetCore.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
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

            public NestedModel NestedModel { get; set; }

            [Required]
            [DefaultValue(false)]
            public _Boolean IsACar { get; private set; }

            public _DateTime Date { get; private set; }

            public _DateTime DateTime { get; set; }
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
    }
}
