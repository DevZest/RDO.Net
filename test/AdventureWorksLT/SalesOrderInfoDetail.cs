using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    [ExtraColumns(typeof(ForeignKeyLookup.Ext))]
    public class SalesOrderInfoDetail : SalesOrderDetail
    {
        [ModelValidator]
        private DataValidationError ValidateProduct(DataRow dataRow)
        {
            if (ProductID[dataRow] == null)
                return null;
            var ext = GetExtraColumns<ForeignKeyLookup.Ext>();
            var productNumber = ext.Product.ProductNumber;
            var productName = ext.Product.Name;

            if (string.IsNullOrEmpty(productNumber[dataRow]))
                return new DataValidationError(string.Format(UserMessages.Validation_ValueIsRequired, productNumber.DisplayName), productNumber);

            if (string.IsNullOrEmpty(productName[dataRow]))
                return new DataValidationError(string.Format(UserMessages.Validation_ValueIsRequired, productName.DisplayName), productName);

            return null;
        }
    }
}
