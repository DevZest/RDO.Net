using Microsoft.AspNetCore.Mvc;

namespace DevZest.Data.AspNetCore
{
    public static class MvcOptionsExtensions
    {
        public static MvcOptions UseDataSet(this MvcOptions mvcOptions)
        {
            mvcOptions.ModelValidatorProviders.Add(new DataSetValidatorProvider());
            mvcOptions.ModelBinderProviders.Insert(0, new DataSetModelBinderProvider());
            return mvcOptions;
        }
    }
}
