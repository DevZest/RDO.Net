using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    [Rule(nameof(ValidateProductNumber), SourceColumns = new string[] { nameof(ProductID), nameof(Product) + "." + nameof(SalesOrderInfoDetail.Product.ProductNumber) })]
    [Rule(nameof(ValidateProductName), SourceColumns = new string[] { nameof(ProductID), nameof(Product) + "." + nameof(SalesOrderInfoDetail.Product.Name) })]
    [InvisibleToDbDesigner]
    public class SalesOrderInfoDetail : SalesOrderDetail
    {
        static SalesOrderInfoDetail()
        {
            RegisterProjection((SalesOrderInfoDetail _) => _.Product);
        }

        public Product.Lookup Product { get; private set; }

        [_Rule]
        private DataValidationError ValidateProductNumber(DataRow dataRow)
        {
            if (ProductID[dataRow] == null)
                return null;
            var productNumber = Product.ProductNumber;

            if (string.IsNullOrEmpty(productNumber[dataRow]))
                return new DataValidationError(string.Format(UserMessages.Validation_ValueIsRequired, productNumber.DisplayName), productNumber);

            return null;
        }

        [_Rule]
        private DataValidationError ValidateProductName(DataRow dataRow)
        {
            if (ProductID[dataRow] == null)
                return null;

            var productName = Product.Name;
            if (string.IsNullOrEmpty(productName[dataRow]))
                return new DataValidationError(string.Format(UserMessages.Validation_ValueIsRequired, productName.DisplayName), productName);

            return null;
        }
    }
}
