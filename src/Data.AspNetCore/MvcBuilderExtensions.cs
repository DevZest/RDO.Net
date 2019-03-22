using DevZest.Data.AspNetCore.Primitives;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DevZest.Data.AspNetCore
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddDataSetMvc(this IMvcBuilder mvcBuilder)
        {
            mvcBuilder.AddMvcOptions(options => {
                options.ModelValidatorProviders.Add(new DataSetValidatorProvider());
                options.ModelBinderProviders.Insert(0, new DataSetModelBinderProvider());
            });

            mvcBuilder.Services.TryAddSingleton<DataSetValidationHtmlAttributeProvider, DefaultDataSetValidationHtmlAttributeProvider>();

            return mvcBuilder;
        }
    }
}
