using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using DevZest.Data.Primitives;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace DevZest.Data.AspNetCore.Primitives
{
    public class DataSetModelBinderTest
    {
        public class Student : Model
        {
            static Student()
            {
                RegisterColumn((Student _) => _.StudentId);
                RegisterColumn((Student _) => _.Name);
                RegisterChildModel((Student _) => _.Courses);
            }

            public _Int32 StudentId { get; private set; }

            public _String Name { get; private set; }

            public Course Courses { get; private set; }
        }

        public class Course : Model
        {
            static Course()
            {
                RegisterColumn((Course _) => _.CourseId);
                RegisterColumn((Course _) => _.Name);
            }

            public _Int32 CourseId { get; private set; }

            public _String Name { get; private set; }
        }

        private class ScalarModel
        {
            [Scalar]
            public DataSet<Student> Student { get; set; }
        }

        private class CollectionModel
        {
            public DataSet<Student> Students { get; set; }
        }

        private static readonly TestModelMetadataProvider s_metadataProvider = new TestModelMetadataProvider();

        private static ModelMetadata GetMetadataForType(Type type)
        {
            return s_metadataProvider.GetMetadataForType(type);
        }

        private static ModelMetadata GetMetadataForParameter(Type type, string methodName, int parameterIndex = 0)
        {
            var parameter = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic).GetParameters()[parameterIndex];
            return s_metadataProvider.GetMetadataForParameter(parameter);
        }

        private static ModelMetadata GetMetadataForProperty(Type type, string propertyName)
        {
            return s_metadataProvider.GetMetadataForProperty(type, propertyName);
        }

        private static DefaultModelBindingContext CreateContext(ModelMetadata metadata, object model = null)
        {
            var valueProvider = new TestValueProvider(new Dictionary<string, object>());
            return new DefaultModelBindingContext()
            {
                BinderModelName = metadata.BinderModelName,
                BindingSource = metadata.BindingSource,
                IsTopLevelObject = true,
                Model = model,
                ModelMetadata = metadata,
                ModelName = "theModel",
                ModelState = new ModelStateDictionary(),
                ValueProvider = valueProvider,
            };
        }

        // Provides the ability to easily mock + call each of these APIs
        public class TestableDataSetModelBinder<T> : DataSetModelBinder<T>
            where T : Model, new()
        {
            public TestableDataSetModelBinder()
                : base(NullLoggerFactory.Instance)
            {
            }

            public Dictionary<ModelMetadata, ModelBindingResult> Results { get; } = new Dictionary<ModelMetadata, ModelBindingResult>();

            public virtual DataSet<T> CreateDataSetPublic(ModelBindingContext bindingContext)
            {
                return base.CreateDataSet(bindingContext);
            }

            protected override DataSet<T> CreateDataSet(ModelBindingContext bindingContext)
            {
                return CreateDataSetPublic(bindingContext);
            }
        }

        private IActionResult ActionWithDataSetParameter(DataSet<Student> parameter) => null;

        private IActionResult ActionWithScalarDataSetParameter([Scalar]DataSet<Student> parameter) => null;

        [Theory]
        [InlineData(nameof(ActionWithDataSetParameter), "parameter")]
        [InlineData(nameof(ActionWithScalarDataSetParameter), "parameter")]
        public async Task BindModelAsync_CreatesDataSetForParameter(string methodName, string paramName)
        {
            // Arrange
            var metadata = GetMetadataForParameter(typeof(DataSetModelBinderTest), methodName);
            var bindingContext = new DefaultModelBindingContext
            {
                IsTopLevelObject = true,
                ModelMetadata = metadata,
                ModelName = paramName,
                FieldName = paramName,
                ValueProvider = new TestValueProvider(new Dictionary<string, object>()),
                ModelState = new ModelStateDictionary()
            };

            var binder = new DataSetModelBinder<Student>(NullLoggerFactory.Instance);

            // Act
            await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(bindingContext.Result.IsModelSet);
            Assert.NotNull(bindingContext.Result.Model);
            Assert.IsAssignableFrom<DataSet<Student>>(bindingContext.Result.Model);
            var keyValuePair = Assert.Single(bindingContext.ModelState);
            Assert.Equal(paramName, keyValuePair.Key);
            Assert.Empty(keyValuePair.Value.Errors);
        }

        [Fact]
        public async Task BindModelAsync_ModelIsNotNull_DoesNotCallCreateDataSet()
        {
            // Arrange
            var bindingContext = CreateContext(GetMetadataForType(typeof(DataSet<Student>)), DataSet<Student>.Create());
            var originalModel = bindingContext.Model;

            var binder = new Mock<TestableDataSetModelBinder<Student>>() { CallBase = true };
            binder
                .Setup(b => b.CreateDataSetPublic(It.IsAny<ModelBindingContext>()))
                .Verifiable();

            // Act
            await binder.Object.BindModelAsync(bindingContext);

            // Assert
            Assert.Same(originalModel, bindingContext.Model);
            binder.Verify(o => o.CreateDataSetPublic(bindingContext), Times.Never());
        }

        [Fact]
        public async Task BindModelAsync_ModelIsNull_CallsCreateDataSet()
        {
            // Arrange
            var bindingContext = CreateContext(GetMetadataForType(typeof(DataSet<Student>)), model: null);

            var testableBinder = new Mock<TestableDataSetModelBinder<Student>> { CallBase = true };
            testableBinder
                .Setup(o => o.CreateDataSetPublic(bindingContext))
                .Returns(DataSet<Student>.Create())
                .Verifiable();

            // Act
            await testableBinder.Object.BindModelAsync(bindingContext);

            // Assert
            Assert.NotNull(bindingContext.Model);
            Assert.IsAssignableFrom<DataSet<Student>>(bindingContext.Model);
            testableBinder.Verify();
        }

        [Fact]
        public async Task BindModelAsync_CollectionDataSetParam()
        {
            // Arrange
            var metadata = GetMetadataForParameter(typeof(DataSetModelBinderTest), nameof(ActionWithDataSetParameter));
            var paramName = "parameter";
            var values = new Dictionary<string, object>()
            {
                { "parameter[0].StudentId", "1" },
                { "parameter[0].Name", "John" },
                { "parameter[0].Courses[0].CourseId", "1" },
                { "parameter[0].Courses[0].Name", "Math" },
                { "parameter[0].Courses[1].CourseId", "2" },
                { "parameter[0].Courses[1].Name", "History" },
                { "parameter[1].StudentId", "2" },
                { "parameter[1].Name", "Tony" },
                { "parameter[1].Courses[0].CourseId", "2" },
                { "parameter[1].Courses[0].Name", "History" },
                { "parameter[1].Courses[1].CourseId", "2" },
                { "parameter[1].Courses[1].Name", "Math" },
            };
            var bindingContext = new DefaultModelBindingContext
            {
                IsTopLevelObject = true,
                ModelMetadata = metadata,
                ModelName = paramName,
                FieldName = paramName,
                ValueProvider = new TestValueProvider(values),
                ModelState = new ModelStateDictionary()
            };

            var binder = new DataSetModelBinder<Student>(NullLoggerFactory.Instance);

            // Act
            await binder.BindModelAsync(bindingContext);

            // Assert
            var result = (DataSet<Student>)bindingContext.Model;
            var expectedJson =
@"[
   {
      ""StudentId"" : 1,
      ""Name"" : ""John"",
      ""Courses"" : [
         {
            ""CourseId"" : 1,
            ""Name"" : ""Math""
         },
         {
            ""CourseId"" : 2,
            ""Name"" : ""History""
         }
      ]
   },
   {
      ""StudentId"" : 2,
      ""Name"" : ""Tony"",
      ""Courses"" : [
         {
            ""CourseId"" : 2,
            ""Name"" : ""History""
         },
         {
            ""CourseId"" : 2,
            ""Name"" : ""Math""
         }
      ]
   }
]";
            Assert.Equal(expectedJson, result.ToJsonString(true));
        }

        [Fact]
        public async Task BindModelAsync_CollectionDataSetParam_ExplicitIndex()
        {
            // Arrange
            var metadata = GetMetadataForParameter(typeof(DataSetModelBinderTest), nameof(ActionWithDataSetParameter));
            var paramName = "parameter";

            var formCollection = new FormCollection(
                new Dictionary<string, StringValues>()
                {
                    { "parameter.index", new string[] { "0", "2" } },
                    { "parameter[0].StudentId", "1" },
                    { "parameter[0].Name", "John" },
                    { "parameter[0].Courses[0].CourseId", "1" },
                    { "parameter[0].Courses[0].Name", "Math" },
                    { "parameter[0].Courses[1].CourseId", "2" },
                    { "parameter[0].Courses[1].Name", "History" },
                    { "parameter[2].StudentId", "2" },
                    { "parameter[2].Name", "Tony" },
                    { "parameter[2].Courses[0].CourseId", "2" },
                    { "parameter[2].Courses[0].Name", "History" },
                    { "parameter[2].Courses[1].CourseId", "2" },
                    { "parameter[2].Courses[1].Name", "Math" },
                });
            var vp = new FormValueProvider(BindingSource.Form, formCollection, CultureInfo.CurrentCulture);
            var bindingContext = new DefaultModelBindingContext
            {
                IsTopLevelObject = true,
                ModelMetadata = metadata,
                ModelName = paramName,
                FieldName = paramName,
                ValueProvider = vp,
                ModelState = new ModelStateDictionary()
            };

            var binder = new DataSetModelBinder<Student>(NullLoggerFactory.Instance);

            // Act
            await binder.BindModelAsync(bindingContext);

            // Assert
            var result = (DataSet<Student>)bindingContext.Model;
            var expectedJson =
@"[
   {
      ""StudentId"" : 1,
      ""Name"" : ""John"",
      ""Courses"" : [
         {
            ""CourseId"" : 1,
            ""Name"" : ""Math""
         },
         {
            ""CourseId"" : 2,
            ""Name"" : ""History""
         }
      ]
   },
   {
      ""StudentId"" : 2,
      ""Name"" : ""Tony"",
      ""Courses"" : [
         {
            ""CourseId"" : 2,
            ""Name"" : ""History""
         },
         {
            ""CourseId"" : 2,
            ""Name"" : ""Math""
         }
      ]
   }
]";
            Assert.Equal(expectedJson, result.ToJsonString(true));
        }

        [Fact]
        public async Task BindModelAsync_ScalarDataSetParam()
        {
            // Arrange
            var metadata = GetMetadataForParameter(typeof(DataSetModelBinderTest), nameof(ActionWithScalarDataSetParameter));
            var values = new Dictionary<string, object>()
            {
                { "StudentId", "1" },
                { "Name", "John" },
                { "Courses[0].CourseId", "1" },
                { "Courses[0].Name", "Math" },
                { "Courses[1].CourseId", "2" },
                { "Courses[1].Name", "History" }
            };
            var bindingContext = new DefaultModelBindingContext
            {
                IsTopLevelObject = true,
                ModelMetadata = metadata,
                ModelName = string.Empty,
                ValueProvider = new TestValueProvider(values),
                ModelState = new ModelStateDictionary()
            };

            var binder = new DataSetModelBinder<Student>(NullLoggerFactory.Instance);

            // Act
            await binder.BindModelAsync(bindingContext);

            // Assert
            var result = (DataSet<Student>)bindingContext.Model;
            var expectedJson =
@"[
   {
      ""StudentId"" : 1,
      ""Name"" : ""John"",
      ""Courses"" : [
         {
            ""CourseId"" : 1,
            ""Name"" : ""Math""
         },
         {
            ""CourseId"" : 2,
            ""Name"" : ""History""
         }
      ]
   }
]";
            Assert.Equal(expectedJson, result.ToJsonString(true));
        }

        [Fact]
        public async Task BindModelAsync_ScalarDataSetParam_NoData()
        {
            // Arrange
            var metadata = GetMetadataForParameter(typeof(DataSetModelBinderTest), nameof(ActionWithScalarDataSetParameter));
            var values = new Dictionary<string, object>();
            var bindingContext = new DefaultModelBindingContext
            {
                IsTopLevelObject = true,
                ModelMetadata = metadata,
                ModelName = string.Empty,
                ValueProvider = new TestValueProvider(values),
                ModelState = new ModelStateDictionary()
            };

            var binder = new DataSetModelBinder<Student>(NullLoggerFactory.Instance);

            // Act
            await binder.BindModelAsync(bindingContext);

            // Assert
            var result = (DataSet<Student>)bindingContext.Model;
            Assert.Single(result);
        }
    }
}
