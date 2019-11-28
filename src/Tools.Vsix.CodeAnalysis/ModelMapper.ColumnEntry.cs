using DevZest.Data.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace DevZest.Data.CodeAnalysis
{
    partial class ModelMapper
    {
        [CustomValidator(nameof(VAL_DuplicateColumn))]
        public abstract class ColumnEntry : Model, IInitializable<ModelMapper>
        {
            static ColumnEntry()
            {
                RegisterLocalColumn((ColumnEntry _) => _.Column);
            }

            [Required]
            [Display(Name = nameof(UserMessages.Display_ColumnEntry_Column), ResourceType = typeof(UserMessages))]
            public LocalColumn<IPropertySymbol> Column { get; private set; }

            protected ModelMapper ModelMapper { get; private set; }

            public void Initialize(ModelMapper modelMapper)
            {
                ModelMapper = modelMapper;
            }

            [_CustomValidator]
            private CustomValidatorEntry VAL_DuplicateColumn
            {
                get
                {
                    string Validate(DataRow dataRow)
                    {
                        var dataSet = DataSet;
                        foreach (var other in dataSet)
                        {
                            if (other == dataRow)
                                continue;
                            if (Column[dataRow] == Column[other])
                                return UserMessages.ColumnEntry_DuplicateColumn;
                        }
                        return null;
                    }

                    IColumns GetSourceColumns()
                    {
                        return Column;
                    }

                    return new CustomValidatorEntry(Validate, GetSourceColumns);
                }
            }
        }

        private static SyntaxNode[] GenerateArguments<T>(SyntaxGenerator g, DataSet<T> entries)
            where T : ColumnEntry, new()
        {
            var _ = entries._;
            var result = new SyntaxNode[entries.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = g.Argument(g.IdentifierName(_.Column[i].Name));

            return result;
        }
    }
}
