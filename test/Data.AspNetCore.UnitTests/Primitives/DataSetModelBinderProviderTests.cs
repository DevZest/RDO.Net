using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using Xunit;

namespace DevZest.Data.AspNetCore.Primitives
{
    public class DataSetTypeModelBinderProviderTest
    {
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(List<int>))]
        public void Create_ForNonDataSetType_ReturnsNull(Type modelType)
        {
            // Arrange
            var provider = new DataSetModelBinderProvider();

            var context = new TestModelBinderProviderContext(modelType);

            // Act
            var result = provider.GetBinder(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Create_ForDataSetType_ReturnsBinder()
        {
            // Arrange
            var provider = new DataSetModelBinderProvider();

            var context = new TestModelBinderProviderContext(typeof(DataSet<Person>));
            context.OnCreatingBinder(m =>
            {
                if (m.ModelType == typeof(int) || m.ModelType == typeof(string))
                {
                    return Mock.Of<IModelBinder>();
                }
                else
                {
                    Assert.False(true, "Not the right model type");
                    return null;
                }
            });

            // Act
            var result = provider.GetBinder(context);

            // Assert
            Assert.IsType<DataSetModelBinder<Person>>(result);
        }

        private class Person : Model
        {
            static Person()
            {
                RegisterColumn((Person _) => _.Name);
                RegisterColumn((Person _) => _.Age);
            }

            public _String Name { get; private set; }

            public _Int32 Age { get; private set; }
        }
    }
}
