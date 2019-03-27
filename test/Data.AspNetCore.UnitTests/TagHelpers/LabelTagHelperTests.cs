using DevZest.Data.AspNetCore.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace DevZest.Data.AspNetCore.TagHelpers
{
    public class LabelTagHelperTests
    {
        public class TagHelperOutputContent
        {
            public TagHelperOutputContent(
                string originalChildContent,
                string outputContent,
                string expectedContent,
                string expectedId)
            {
                OriginalChildContent = originalChildContent;
                OriginalContent = outputContent;
                ExpectedContent = expectedContent;
                ExpectedId = expectedId;
            }

            public string OriginalChildContent { get; set; }

            public string OriginalContent { get; set; }

            public string ExpectedContent { get; set; }

            public string ExpectedId { get; set; }
        }

        private class TestModel : Model
        {
            static TestModel()
            {
                RegisterColumn((TestModel _) => _.Text);
                RegisterChildModel((TestModel _) => _.NestedModel);
            }

            public _String Text { get; private set; }

            public NestedModel NestedModel { get; set; }
        }

        private class NestedModel : Model
        {
            static NestedModel()
            {
                RegisterColumn((NestedModel _) => _.Text);
            }

            public _String Text { get; private set; }
        }

        private static LabelTagHelper GetTagHelper(Column column, bool isScalar = true, DataRow dataRow = null, IDataSetHtmlGenerator generator = null, IModelMetadataProvider metadataProvider = null)
        {
            return Factory.GetTagHelper(column, isScalar, dataRow, generator, metadataProvider, g => new LabelTagHelper(g));
        }

        // Model (List<Model> or Model instance), container type (Model or NestModel), model accessor,
        // property path, TagHelperOutput values. All accessors should end at a Text property.
        public static TheoryData<Column, DataRow, TagHelperOutputContent> TestDataSet
        {
            get
            {
                var dataSetWithNull = DataSet<TestModel>.Create();
                dataSetWithNull.AddRow();
                dataSetWithNull._.Text[0] = null;
                {
                    var nested = dataSetWithNull[0].GetChildDataSet(dataSetWithNull._.NestedModel);
                    nested.AddRow();
                    nested._.Text[0] = null;
                }

                var dataSetWithText = DataSet<TestModel>.Create();
                dataSetWithText.AddRow();
                dataSetWithText._.Text[0] = "outer text";
                {
                    var nested = dataSetWithText[0].GetChildDataSet(dataSetWithText._.NestedModel);
                    nested.AddRow();
                    nested._.Text[0] = "inner text";
                }

                return new TheoryData<Column, DataRow, TagHelperOutputContent>
                {
                    { DataSet<TestModel>.Create()._.Text, null,
                        new TagHelperOutputContent(string.Empty, Environment.NewLine, Environment.NewLine, "DataSet_Text") },

                    { dataSetWithNull._.Text, null,
                        new TagHelperOutputContent(string.Empty, Environment.NewLine, Environment.NewLine, "DataSet_Text") },
                    { dataSetWithNull._.Text, null,
                        new TagHelperOutputContent(Environment.NewLine, string.Empty, "HtmlEncode[[Text]]", "DataSet_Text") },
                    { dataSetWithNull._.Text, null,
                        new TagHelperOutputContent(Environment.NewLine, "Hello World", "Hello World", "DataSet_Text") },
                    { dataSetWithText._.Text, null,
                        new TagHelperOutputContent(string.Empty, Environment.NewLine, Environment.NewLine, "DataSet_Text") },
                    { dataSetWithText._.Text, null,
                        new TagHelperOutputContent(Environment.NewLine, string.Empty, "HtmlEncode[[Text]]", "DataSet_Text") },
                    { dataSetWithText._.Text, null,
                        new TagHelperOutputContent(Environment.NewLine, "Hello World", "Hello World", "DataSet_Text") },
                    { dataSetWithText._.Text, null,
                        new TagHelperOutputContent(string.Empty, "Hello World", "Hello World", "DataSet_Text") },
                    { dataSetWithText._.Text, null,
                        new TagHelperOutputContent("Hello World", string.Empty, "Hello World", "DataSet_Text") },
                    { dataSetWithText._.Text, null,
                        new TagHelperOutputContent("Hello World1", "Hello World2", "Hello World2", "DataSet_Text") },

                    { dataSetWithNull._.NestedModel.Text, dataSetWithNull[0].GetChildDataSet(dataSetWithNull._.NestedModel)[0],
                        new TagHelperOutputContent(Environment.NewLine, string.Empty, "HtmlEncode[[Text]]", "DataSet_NestedModel_0__Text") },
                    { dataSetWithNull._.NestedModel.Text, dataSetWithNull[0].GetChildDataSet(dataSetWithNull._.NestedModel)[0],
                        new TagHelperOutputContent(Environment.NewLine, "Hello World", "Hello World", "DataSet_NestedModel_0__Text") },
                    { dataSetWithNull._.NestedModel.Text, dataSetWithNull[0].GetChildDataSet(dataSetWithNull._.NestedModel)[0],
                        new TagHelperOutputContent(string.Empty, Environment.NewLine, Environment.NewLine, "DataSet_NestedModel_0__Text") },
                    { dataSetWithNull._.NestedModel.Text, dataSetWithNull[0].GetChildDataSet(dataSetWithNull._.NestedModel)[0],
                        new TagHelperOutputContent(Environment.NewLine, string.Empty, "HtmlEncode[[Text]]", "DataSet_NestedModel_0__Text") },
                };
            }
        }

        [Theory]
        [MemberData(nameof(TestDataSet))]
        public async Task ProcessAsync_GeneratesExpectedOutput(Column column, DataRow dataRow, TagHelperOutputContent tagHelperOutputContent)
        {
            // Arrange
            var expectedTagName = "not-label";
            var expectedAttributes = new TagHelperAttributeList
            {
                { "class", "form-control" },
                { "for", tagHelperOutputContent.ExpectedId }
            };

            var tagHelper = GetTagHelper(column, dataRow: dataRow);
            
            var expectedPreContent = "original pre-content";
            var expectedPostContent = "original post-content";

            var tagHelperContext = new TagHelperContext(
                tagName: "not-label",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var htmlAttributes = new TagHelperAttributeList
            {
                { "class", "form-control" },
            };
            var output = new TagHelperOutput(
                expectedTagName,
                htmlAttributes,
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.AppendHtml(tagHelperOutputContent.OriginalChildContent);
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            output.PreContent.AppendHtml(expectedPreContent);
            output.PostContent.AppendHtml(expectedPostContent);

            // LabelTagHelper checks IsContentModified so we don't want to forcibly set it if
            // tagHelperOutputContent.OriginalContent is going to be null or empty.
            if (!string.IsNullOrEmpty(tagHelperOutputContent.OriginalContent))
            {
                output.Content.AppendHtml(tagHelperOutputContent.OriginalContent);
            }

            // Act
            await tagHelper.ProcessAsync(tagHelperContext, output);

            // Assert
            Assert.Equal(expectedAttributes, output.Attributes);
            Assert.Equal(expectedPreContent, output.PreContent.GetContent());
            Assert.Equal(
                tagHelperOutputContent.ExpectedContent,
                HtmlContentUtilities.HtmlContentToString(output.Content));
            Assert.Equal(expectedPostContent, output.PostContent.GetContent());
            Assert.Equal(TagMode.StartTagAndEndTag, output.TagMode);
            Assert.Equal(expectedTagName, output.TagName);
        }

        // Display name, original child content, expected child content, and expected ID.
        // Uses TagHelperOutputContent.OriginalContent to pass HtmlFieldPrefix values.
        public static TheoryData<string, string, string, string> DisplayNameDataSet
        {
            get
            {
                return new TheoryData<string, string, string, string>
                {
                    { null, string.Empty, $"HtmlEncode[[Text]]", "DataSet_Text" },
                    { string.Empty, string.Empty, $"HtmlEncode[[Text]]", "DataSet_Text" },
                    { "a label", string.Empty, $"HtmlEncode[[a label]]", "DataSet_Text" },
                    { null, "original label", "original label", "DataSet_Text" },
                    { string.Empty, "original label", "original label", "DataSet_Text" },
                    { "a label", "original label", "original label", "DataSet_Text" }
                };
            }
        }

        // Prior to aspnet/Mvc#6638 fix, helpers generated nothing in this test when displayName was empty.
        [Theory]
        [MemberData(nameof(DisplayNameDataSet))]
        public async Task ProcessAsync_GeneratesExpectedOutput_WithDisplayName(
            string displayName,
            string originalChildContent,
            string expectedContent,
            string expectedId)
        {
            // Arrange
            var expectedAttributes = new TagHelperAttributeList
            {
                { "for", expectedId }
            };

            var dataSet = DataSet<TestModel>.Create(_ => _.Text.DisplayName = displayName);
            var tagHelper = GetTagHelper(dataSet._.Text);

            var tagHelperContext = new TagHelperContext(
                tagName: "label",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                "label",
                new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.AppendHtml(originalChildContent);
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });

            // Act
            await tagHelper.ProcessAsync(tagHelperContext, output);

            // Assert
            Assert.Equal(expectedAttributes, output.Attributes);
            Assert.Equal(expectedContent, HtmlContentUtilities.HtmlContentToString(output.Content));
        }
    }
}
