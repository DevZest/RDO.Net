using Microsoft.Extensions.DependencyInjection;

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

            return mvcBuilder;
        }
    }
}
