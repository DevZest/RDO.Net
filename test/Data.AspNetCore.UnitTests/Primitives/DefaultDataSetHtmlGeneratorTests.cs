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
            return Factory.GetDataSetHtmlGenerator(configurationExpression);
        }

        private static ViewContext GetViewContext<TModel>(TModel model)
        {
            return Factory.GetViewContext(model);
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
                RegisterLocalColumn((TestModel _) => _.Collection);
            }

            public _Int32 Id { get; private set; }

            public _String Name { get; private set; }

            public _ByteEnum<RegularEnum> RegularEnum { get; private set; }

            public _ByteEnum<FlagsEnum> FlagsEnum { get; private set; }

            public LocalColumn<List<string>> Collection { get; private set; }
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

        // rawValue -> expected current values
        public static TheoryData<string[], string[]> GetCurrentValues_StringCollectionData
        {
            get
            {
                return new TheoryData<string[], string[]>
                {
                    { new string[] { null }, new [] { string.Empty } },
                    { new [] { string.Empty }, new [] { string.Empty } },
                    { new [] { "some string" }, new [] { "some string" } },
                    { new [] { "some string", "some other string" }, new [] { "some string", "some other string" } },
                    {
                        new [] { null, "some string", "some other string" },
                        new [] { string.Empty, "some string", "some other string" }
                    },
                    // ignores duplicates
                    {
                        new [] { null, "some string", null, "some other string", null, "some string", null },
                        new [] { string.Empty, "some string", "some other string" }
                    },
                    // ignores case of duplicates
                    {
                        new [] { "some string", "SoMe StriNg", "Some String", "soME STRing", "SOME STRING" },
                        new [] { "some string" }
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(GetCurrentValues_StringCollectionData))]
        public void GetCurrentValues_CollectionWithModel_ReturnsModel(
            string[] rawValue,
            IReadOnlyCollection<string> expected)
        {
            // Arrange
            var generator = GetGenerator();
            var dataSet = DataSet<TestModel>.Create();
            var column = dataSet._.Collection;
            var viewContext = GetViewContext(dataSet);

            // Act
            var result = generator.GetCurrentValues(
                viewContext,
                column,
                rawValue,
                allowMultiple: true);

            // Assert
            Assert.Equal<string>(expected, result);
        }

        // property name, rawValue -> expected current values
        public static TheoryData<string, object, string[]> GetCurrentValues_ValueToConvertData
        {
            get
            {
                return new TheoryData<string, object, string[]>
                {
                    { nameof(TestModel.FlagsEnum), FlagsEnum.All, new [] { "-1", "All" } },
                    { nameof(TestModel.FlagsEnum), FlagsEnum.FortyTwo, new [] { "42", "FortyTwo" } },
                    { nameof(TestModel.FlagsEnum), FlagsEnum.None, new [] { "0", "None" } },
                    { nameof(TestModel.FlagsEnum), FlagsEnum.Two, new [] { "2", "Two" } },
                    { nameof(TestModel.FlagsEnum), string.Empty, new [] { string.Empty } },
                    { nameof(TestModel.FlagsEnum), "All", new [] { "-1", "All" } },
                    { nameof(TestModel.FlagsEnum), "FortyTwo", new [] { "42", "FortyTwo" } },
                    { nameof(TestModel.FlagsEnum), "None", new [] { "0", "None" } },
                    { nameof(TestModel.FlagsEnum), "Two", new [] { "2", "Two" } },
                    { nameof(TestModel.FlagsEnum), "Two, Four", new [] { "Two, Four", "6" } },
                    { nameof(TestModel.FlagsEnum), "garbage", new [] { "garbage" } },
                    { nameof(TestModel.FlagsEnum), "0", new [] { "0", "None" } },
                    { nameof(TestModel.FlagsEnum), "   43", new [] { "   43", "43" } },
                    { nameof(TestModel.FlagsEnum), "-5   ", new [] { "-5   ", "-5" } },
                    { nameof(TestModel.FlagsEnum), 0, new [] { "0", "None" } },
                    { nameof(TestModel.FlagsEnum), 1, new [] { "1", "One" } },
                    { nameof(TestModel.FlagsEnum), 43, new [] { "43" } },
                    { nameof(TestModel.FlagsEnum), -5, new [] { "-5" } },
                    { nameof(TestModel.FlagsEnum), int.MaxValue, new [] { "2147483647" } },
                    { nameof(TestModel.FlagsEnum), (uint)int.MaxValue + 1, new [] { "2147483648" } },
                    { nameof(TestModel.FlagsEnum), uint.MaxValue, new [] { "4294967295" } },  // converted to string & used

                    { nameof(TestModel.Id), string.Empty, new [] { string.Empty } },
                    { nameof(TestModel.Id), "garbage", new [] { "garbage" } },                  // no compatibility checks
                    { nameof(TestModel.Id), "0", new [] { "0" } },
                    { nameof(TestModel.Id), "  43", new [] { "  43" } },
                    { nameof(TestModel.Id), "-5  ", new [] { "-5  " } },
                    { nameof(TestModel.Id), 0, new [] { "0" } },
                    { nameof(TestModel.Id), 1, new [] { "1" } },
                    { nameof(TestModel.Id), 43, new [] { "43" } },
                    { nameof(TestModel.Id), -5, new [] { "-5" } },
                    { nameof(TestModel.Id), int.MaxValue, new [] { "2147483647" } },
                    { nameof(TestModel.Id), (uint)int.MaxValue + 1, new [] { "2147483648" } },  // no limit checks
                    { nameof(TestModel.Id), uint.MaxValue, new [] { "4294967295" } },           // no limit checks

                    { nameof(TestModel.RegularEnum), RegularEnum.Zero, new [] { "0", "Zero" } },
                    { nameof(TestModel.RegularEnum), RegularEnum.One, new [] { "1", "One" } },
                    { nameof(TestModel.RegularEnum), RegularEnum.Two, new [] { "2", "Two" } },
                    { nameof(TestModel.RegularEnum), RegularEnum.Three, new [] { "3", "Three" } },
                    { nameof(TestModel.RegularEnum), string.Empty, new [] { string.Empty } },
                    { nameof(TestModel.RegularEnum), "Zero", new [] { "0", "Zero" } },
                    { nameof(TestModel.RegularEnum), "Two", new [] { "2", "Two" } },
                    { nameof(TestModel.RegularEnum), "One, Two", new [] { "One, Two", "3", "Three" } },
                    { nameof(TestModel.RegularEnum), "garbage", new [] { "garbage" } },
                    { nameof(TestModel.RegularEnum), "0", new [] { "0", "Zero" } },
                    { nameof(TestModel.RegularEnum), "   43", new [] { "   43", "43" } },
                    { nameof(TestModel.RegularEnum), "-5   ", new [] { "-5   ", "-5" } },
                    { nameof(TestModel.RegularEnum), 0, new [] { "0", "Zero" } },
                    { nameof(TestModel.RegularEnum), 1, new [] { "1", "One" } },
                    { nameof(TestModel.RegularEnum), 43, new [] { "43" } },
                    { nameof(TestModel.RegularEnum), -5, new [] { "-5" } },
                    { nameof(TestModel.RegularEnum), int.MaxValue, new [] { "2147483647" } },
                    { nameof(TestModel.RegularEnum), (uint)int.MaxValue + 1, new [] { "2147483648" } },
                    { nameof(TestModel.RegularEnum), uint.MaxValue, new [] { "4294967295" } },
                };
            }
        }

        [Theory]
        [MemberData(nameof(GetCurrentValues_ValueToConvertData))]
        public void GetCurrentValues_ValueConvertedAsExpected(
            string columnName,
            object rawValue,
            IReadOnlyCollection<string> expected)
        {
            // Arrange
            var generator = GetGenerator();
            var dataSet = DataSet<TestModel>.Create();
            var column = dataSet._.GetColumns()[columnName];
            var viewContext = GetViewContext(dataSet);

            // Act
            var result = generator.GetCurrentValues(
                viewContext,
                column,
                rawValue,
                allowMultiple: false);

            // Assert
            Assert.Equal<string>(expected, result);
        }

    }
}
