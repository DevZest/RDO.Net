using System.Collections.Generic;
using DevZest.Data.AspNetCore.ClientValidation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DevZest.Data.AspNetCore.Primitives
{
    /// <summary>
    /// The default implementation to provide validation attributes for expressions.
    /// </summary>
    public class DefaultDataSetValidationHtmlAttributeProvider : DataSetValidationHtmlAttributeProvider
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DefaultDataSetValidationHtmlAttributeProvider"/> class.
        /// </summary>
        /// <param name="config">The <see cref="DataSetMvcConfiguration"/>.</param>
        public DefaultDataSetValidationHtmlAttributeProvider(DataSetMvcConfiguration config)
        {
            _clientValidators = config.DataSetClientValidators;
        }

        private readonly IList<IDataSetClientValidator> _clientValidators;

        /// <inheritdoc/>
        protected override void AddValidationAttributes(ViewContext viewContext, Column column, IDictionary<string, string> attributes)
        {
            for (int i = 0; i < _clientValidators.Count; i++)
                _clientValidators[i].AddValidation(viewContext, column, attributes);
        }
    }
}
