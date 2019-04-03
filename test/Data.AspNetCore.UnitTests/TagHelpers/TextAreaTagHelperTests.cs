using DevZest.Data.AspNetCore.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DevZest.Data.AspNetCore.TagHelpers
{
    public class TextAreaTagHelperTests
    {
        public class NameAndId
        {
            public NameAndId(string name, string id)
            {
                Name = name;
                Id = id;
            }

            public string Name { get; private set; }

            public string Id { get; private set; }
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

        private static TextAreaTagHelper GetTagHelper(Column column, bool isScalar = true, DataRow dataRow = null, IDataSetHtmlGenerator generator = null, IModelMetadataProvider metadataProvider = null)
        {
            return Factory.GetTagHelper(column, isScalar, dataRow, generator, metadataProvider, g => new TextAreaTagHelper(g));
        }

        public static TheoryData<Column, DataRow, NameAndId, string> TestDataSet
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

                return new TheoryData<Column, DataRow, NameAndId, string>
                {
                    { DataSet<TestModel>.Create()._.Text, null, new NameAndId("DataSet.Text", "DataSet_Text"), Environment.NewLine },
                    { dataSetWithNull._.Text, null, new NameAndId("DataSet.Text", "DataSet_Text"), Environment.NewLine },
                    { dataSetWithText._.Text, null, new NameAndId("DataSet.Text", "DataSet_Text"), Environment.NewLine + "HtmlEncode[[outer text]]" },
                    { dataSetWithNull._.NestedModel.Text, dataSetWithNull[0].GetChildDataSet(dataSetWithNull._.NestedModel)[0],
                        new NameAndId("DataSet.NestedModel[0].Text", "DataSet_NestedModel_0__Text"), Environment.NewLine },
                    { dataSetWithText._.NestedModel.Text, dataSetWithText[0].GetChildDataSet(dataSetWithText._.NestedModel)[0],
                        new NameAndId("DataSet.NestedModel[0].Text", "DataSet_NestedModel_0__Text"), Environment.NewLine + "HtmlEncode[[inner text]]" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(TestDataSet))]
        public async Task Process_GeneratesExpectedOutput(
            Column column,
            DataRow dataRow,
            NameAndId nameAndId,
            string expectedContent)
        {
            // Arrange
            var expectedAttributes = new TagHelperAttributeList
            {
                { "class", "form-control" },
                { "id", nameAndId.Id },
                { "name", nameAndId.Name },
            };
            var expectedTagName = "not-textarea";

            var tagHelper = GetTagHelper(column, dataRow: dataRow);

            var tagHelperContext = new TagHelperContext(
                tagName: "text-area",
                allAttributes: new TagHelperAttributeList(Enumerable.Empty<TagHelperAttribute>()),
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
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                })
            {
                TagMode = TagMode.SelfClosing,
            };
            output.Content.SetContent("original content");

            // Act
            await tagHelper.ProcessAsync(tagHelperContext, output);

            // Assert
            Assert.Equal(TagMode.SelfClosing, output.TagMode);
            Assert.Equal(expectedAttributes, output.Attributes);
            Assert.Equal(expectedContent, HtmlContentUtilities.HtmlContentToString(output.Content));
            Assert.Equal(expectedTagName, output.TagName);
        }
    }
}
