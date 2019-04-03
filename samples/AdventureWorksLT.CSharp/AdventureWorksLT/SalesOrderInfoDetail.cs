using DevZest.Data;
using DevZest.Data.Annotations;

namespace DevZest.Samples.AdventureWorksLT
{
    [CustomValidator(nameof(VAL_ProductNumber))]
    [CustomValidator(nameof(VAL_ProductName))]
    [InvisibleToDbDesigner]
    public class SalesOrderInfoDetail : SalesOrderDetail
    {
        static SalesOrderInfoDetail()
        {
            RegisterProjection((SalesOrderInfoDetail _) => _.Product);
        }

        public Product.Lookup Product { get; private set; }

        [_CustomValidator]
        private CustomValidatorEntry VAL_ProductNumber
        {
            get
            {
                string Validate(DataRow dataRow)
                {
                    if (ProductID[dataRow] == null)
                        return null;
                    var productNumber = Product.ProductNumber;

                    if (string.IsNullOrEmpty(productNumber[dataRow]))
                        return string.Format(UserMessages.Validation_ValueIsRequired, productNumber.DisplayName);
                    else
                        return null;
                }

                IColumns GetSourceColumns()
                {
                    return Product.ProductNumber;
                }

                return new CustomValidatorEntry(Validate, GetSourceColumns);
            }
        }

        [_CustomValidator]
        private CustomValidatorEntry VAL_ProductName
        {
            get
            {
                string Validate(DataRow dataRow)
                {
                    if (ProductID[dataRow] == null)
                        return null;

                    var productName = Product.Name;
                    if (string.IsNullOrEmpty(productName[dataRow]))
                        return string.Format(UserMessages.Validation_ValueIsRequired, productName.DisplayName);

                    return null;
                }

                IColumns GetSourceColumns()
                {
                    return Product.Name;
                }

                return new CustomValidatorEntry(Validate, GetSourceColumns);
            }
        }
    }
}
