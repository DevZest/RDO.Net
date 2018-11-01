using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    [Validator(nameof(ValidateProduct))]
    public class SalesOrderInfoDetail : SalesOrderDetail
    {
        static SalesOrderInfoDetail()
        {
            RegisterProjection((SalesOrderInfoDetail _) => _.Product);
        }

        public Product.Lookup Product { get; private set; }

        private DataValidationError ValidateProduct(DataRow dataRow)
        {
            if (ProductID[dataRow] == null)
                return null;
            var productNumber = Product.ProductNumber;
            var productName = Product.Name;

            if (string.IsNullOrEmpty(productNumber[dataRow]))
                return new DataValidationError(string.Format(UserMessages.Validation_ValueIsRequired, productNumber.DisplayName), productNumber);

            if (string.IsNullOrEmpty(productName[dataRow]))
                return new DataValidationError(string.Format(UserMessages.Validation_ValueIsRequired, productName.DisplayName), productName);

            return null;
        }
    }
}
