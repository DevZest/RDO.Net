using DevZest.Data.Primitives;
using System.Collections.Generic;

namespace DevZest.Data.AspNetCore.Primitives
{
    internal class JsonReaderAdapter : JsonReader
    {
        public JsonReaderAdapter(IEnumerable<JsonToken> jsonTokens, IJsonCustomizer jsonCustomizer)
            : base(jsonCustomizer)
        {
            _jsonTokens = jsonTokens.GetEnumerator();
        }

        private readonly IEnumerator<JsonToken> _jsonTokens;

        protected override JsonToken NextToken()
        {
            _jsonTokens.MoveNext();
            return _jsonTokens.Current;
        }
    }
}
