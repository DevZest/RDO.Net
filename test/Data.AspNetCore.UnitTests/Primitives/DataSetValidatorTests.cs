using DevZest.Data.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Linq;
using Xunit;

namespace DevZest.Data.AspNetCore.Primitives
{
    public class DataSetValidatorTests
    {
        private class SampleModel : Model
        {
            static SampleModel()
            {
                RegisterColumn((SampleModel _) => _.ID);
            }

            [Required]
            public _Int32 ID { get; private set; }
        }

        private class ScalarSampleModelContainer
        {
            [Scalar]
            public DataSet<SampleModel> SampleModel { get; set; }
        }

        private class SampleModelContainer
        {
            public DataSet<SampleModel> SampleModel { get; set; }
        }

        private static readonly ModelMetadataProvider _metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();

        [Fact]
        public void Validate_ReturnsExpectedResults_Scalar()
        {
            // Arrange
            var validator = new DataSetValidator();
            var model = DataSet<SampleModel>.Create();
            model.AddRow();

            var metadata = _metadataProvider.GetMetadataForProperty(
                typeof(ScalarSampleModelContainer),
                nameof(ScalarSampleModelContainer.SampleModel));
            var validationContext = new ModelValidationContext(
                new ActionContext(),
                metadata,
                _metadataProvider,
                container: null,
                model: model);

            // Act
            var results = validator.Validate(validationContext);
            var resultsArray = results.ToArray();

            // Assert
            Assert.NotNull(results);
            Assert.Single(resultsArray);
            Assert.Equal(nameof(SampleModel.ID), resultsArray[0].MemberName);
        }

        [Fact]
        public void Validate_ReturnsExpectedResults_Collection()
        {
            // Arrange
            var validator = new DataSetValidator();
            var model = DataSet<SampleModel>.Create();
            model.AddRow();

            var metadata = _metadataProvider.GetMetadataForProperty(
                typeof(SampleModelContainer),
                nameof(SampleModelContainer.SampleModel));
            var validationContext = new ModelValidationContext(
                new ActionContext(),
                metadata,
                _metadataProvider,
                container: null,
                model: model);

            // Act
            var results = validator.Validate(validationContext);
            var resultsArray = results.ToArray();

            // Assert
            Assert.NotNull(results);
            Assert.Single(resultsArray);
            Assert.Equal(ModelNames.CreatePropertyModelName("[0]", nameof(SampleModel.ID)), resultsArray[0].MemberName);
        }
    }
}
