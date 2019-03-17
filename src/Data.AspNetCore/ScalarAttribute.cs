using System;
using System.ComponentModel.DataAnnotations;

namespace DevZest.Data.AspNetCore
{
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class ScalarAttribute : ValidationAttribute
    {
        public ScalarAttribute()
            : base(() => UserMessages.ScalarAttribute_ValidationError)
        {
        }

        public override bool IsValid(object value)
        {
            return (value is DataSet dataSet) && dataSet.Count == 1;
        }
    }
}
