using DevZest.Data.AspNetCore.Primitives;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace DevZest.Data.AspNetCore
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddDataSetMvc(this IMvcBuilder mvcBuilder, Action<DataSetMvcConfiguration> configurationExpression = null)
        {
            mvcBuilder.AddMvcOptions(options => {
                options.ModelValidatorProviders.Add(new DataSetValidatorProvider());
                options.ModelBinderProviders.Insert(0, new DataSetModelBinderProvider());
            });

            var expr = configurationExpression ?? delegate { };
            var config = new DataSetMvcConfiguration();

            expr(config);
            mvcBuilder.Services.AddSingleton(config);

            mvcBuilder.Services.TryAddSingleton<DataSetValidationHtmlAttributeProvider, DefaultDataSetValidationHtmlAttributeProvider>();

            return mvcBuilder;
        }
    }
}
