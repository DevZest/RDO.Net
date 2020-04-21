using DevZest.Data.Primitives;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a pagination result of DataSet.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class PagedDataSet<T>
        where T : Model, new()
    {
        private sealed class _Model : Model
        {
            static _Model()
            {
                RegisterColumn((_Model _) => _.Page);
                RegisterColumn((_Model _) => _.PageSize);
                RegisterColumn((_Model _) => _.TotalCount);
                RegisterColumn((_Model _) => _.Data);
            }

            public _Int32 Page { get; private set; }
            public _Int32 PageSize { get; private set; }
            public _Int32 TotalCount { get; private set; }
            public _DataSet<T> Data { get; private set; }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PagedDataSet{T}"/> class.
        /// </summary>
        public PagedDataSet()
        {
            _dataSet = DataSet<_Model>.Create();
            _dataSet.AddRow();
            Page = PageSize = TotalCount = 0;
        }

        private readonly DataSet<_Model> _dataSet;

        /// <inheritdoc />
        public override string ToString()
        {
            return ToJsonString(true);
        }

        /// <summary>
        /// Serializes into JSON string.
        /// </summary>
        /// <param name="isPretty">Specifies whether serialized JSON should be indented.</param>
        /// <param name="customizer">Customizer for JSON serialization.</param>
        /// <returns>The serialized JSON string.</returns>
        public string ToJsonString(bool isPretty, IJsonCustomizer customizer = null)
        {
            return _dataSet.ToJsonString(isPretty, customizer);
        }

        private _Model _
        {
            get { return _dataSet._;  }
        }

        /// <summary>
        /// Gets or sets the page number.
        /// </summary>
        public int Page
        {
            get { return (int)_.Page[0]; }
            set { _.Page[0] = value; }
        }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize
        {
            get { return (int)_.PageSize[0]; }
            set { _.PageSize[0] = value; }
        }

        /// <summary>
        /// Gets or sets the total count of the data.
        /// </summary>
        public int TotalCount
        {
            get { return (int)_.TotalCount[0]; }
            set { _.TotalCount[0] = value; }
        }

        /// <summary>
        /// Gets or sets the pagination result of the data.
        /// </summary>
        public DataSet<T> Data
        {
            get { return _.Data[0]; }
            set { _.Data[0] = value; }
        }

        /// <summary>
        /// Deserializes JSON string into <see cref="PagedDataSet{T}"/> object.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="customizer">The customizer for deserialization.</param>
        /// <returns>The deserialized object.</returns>
        public static PagedDataSet<T> ParseJson(string json, IJsonCustomizer customizer = null)
        {
            json.VerifyNotEmpty(nameof(json));

            var result = new PagedDataSet<T>();
            var dataSet = result._dataSet;
            if (dataSet != null)
                dataSet.RemoveAt(0);
            JsonReader.Create(json, customizer).Deserialize(result._dataSet, true);
            return dataSet.Count == 1 ? result : null;
        }
    }
}
