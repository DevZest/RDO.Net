using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    public class SalesOrderInfoDetail : SalesOrderDetail
    {
        static SalesOrderInfoDetail()
        {
            RegisterColumnGroup((SalesOrderInfoDetail _) => _.LK_Product);
        }

        public Product.Lookup LK_Product { get; private set; }

        [ModelValidator]
        private DataValidationError ValidateProduct(DataRow dataRow)
        {
            if (ProductID[dataRow] == null)
                return null;
            var productNumber = LK_Product.ProductNumber;
            var productName = LK_Product.Name;

            if (string.IsNullOrEmpty(productNumber[dataRow]))
                return new DataValidationError(string.Format(UserMessages.Validation_ValueIsRequired, productNumber.DisplayName), productNumber);

            if (string.IsNullOrEmpty(productName[dataRow]))
                return new DataValidationError(string.Format(UserMessages.Validation_ValueIsRequired, productName.DisplayName), productName);

            return null;
        }
    }
}
