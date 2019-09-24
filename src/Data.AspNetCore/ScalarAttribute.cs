using System;
using System.ComponentModel.DataAnnotations;

namespace DevZest.Data.AspNetCore
{
    /// <summary>
    /// Validates property or parameter is single-item DataSet.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class ScalarAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ScalarAttribute"/> class.
        /// </summary>
        public ScalarAttribute()
            : base(() => UserMessages.ScalarAttribute_ValidationError)
        {
        }

        /// <inheritdoc/>
        public override bool IsValid(object value)
        {
            return (value is DataSet dataSet) && dataSet.Count == 1;
        }
    }
}
