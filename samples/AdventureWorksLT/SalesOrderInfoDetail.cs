using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    [ExtraColumns(typeof(Product.Lookup))]
    public class SalesOrderInfoDetail : SalesOrderDetail
    {
        [ModelValidator]
        private DataValidationError ValidateProduct(DataRow dataRow)
        {
            if (ProductID[dataRow] == null)
                return null;
            var ext = GetExtraColumns<Product.Lookup>();
            var productNumber = ext.ProductNumber;
            var productName = ext.Name;

            if (string.IsNullOrEmpty(productNumber[dataRow]))
                return new DataValidationError(string.Format(UserMessages.Validation_ValueIsRequired, productNumber.DisplayName), productNumber);

            if (string.IsNullOrEmpty(productName[dataRow]))
                return new DataValidationError(string.Format(UserMessages.Validation_ValueIsRequired, productName.DisplayName), productName);

            return null;
        }
    }
}
