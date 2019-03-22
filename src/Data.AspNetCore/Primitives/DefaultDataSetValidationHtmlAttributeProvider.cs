﻿using System.Collections.Generic;
using DevZest.Data.Primitives;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DevZest.Data.AspNetCore.Primitives
{
    public class DefaultDataSetValidationHtmlAttributeProvider : DataSetValidationHtmlAttributeProvider
    {
        public DefaultDataSetValidationHtmlAttributeProvider(DataSetMvcConfiguration config)
        {
            _clientValidators = config.DataSetClientValidators;
        }

        private readonly IList<IDataSetClientValidator> _clientValidators;

        protected override void AddValidationAttributes(ViewContext viewContext, Column column, IDictionary<string, string> attributes)
        {
            var validators = column.GetParent().Validators;
            for (int i = 0; i < validators.Count; i++)
                AddValidationAttributes(validators[i], viewContext, column, attributes);
        }

        private void AddValidationAttributes(IValidator validator, ViewContext viewContext, Column column, IDictionary<string, string> attributes)
        {
            for (int i = 0; i < _clientValidators.Count; i++)
                _clientValidators[i].AddValidation(validator, viewContext, column, attributes);
        }
    }
}
