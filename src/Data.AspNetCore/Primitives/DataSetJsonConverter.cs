using System;
using DevZest.Data.Primitives;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DevZest.Data.AspNetCore.Primitives
{
    /// <summary>
    /// Converts <see cref="DataSet"/> into JSON.
    /// </summary>
    public class DataSetJsonConverter : JsonConverter<DataSet>
    {
        private ref struct JsonTokenizer
        {
            public JsonTokenizer(ref Utf8JsonReader jsonReader)
            {
                _states = new Stack<State>();
                _tokens = new Queue<JsonToken>(2);
                _flagPostValue = false;
                _isEof = false;
            }

            private enum State
            {
                Object,
                Array
            }

            private Stack<State> _states;
            private bool _flagPostValue;
            private Queue<JsonToken> _tokens;

            private State? CurrentState
            {
                get { return _states.Count == 0 ? default(State?) : _states.Peek(); }
            }

            private void EnterState(State state)
            {
                _states.Push(state);
                _flagPostValue = false;
            }

            private void ExitState(State state)
            {
                Debug.Assert(state == CurrentState);
                _states.Pop();
                _flagPostValue = CurrentState.HasValue;
            }

            private bool RestoreEatenComma(ref Utf8JsonReader jsonReader)
            {
                if (!_flagPostValue)
                    return false;

                _flagPostValue = false;

                var currentState = CurrentState;
                var tokenType = jsonReader.TokenType;
                return (currentState == State.Array && tokenType != JsonTokenType.EndArray)
                    || (currentState == State.Object && tokenType != JsonTokenType.EndObject);
            }

            private bool _isEof;

            private JsonToken NextToken(ref Utf8JsonReader jsonReader)
            {
                if (_tokens.Count == 0)
                {
                    EnqueueCurrentJsonTokens(ref jsonReader);
                    _isEof = !jsonReader.Read();
                }

                return _tokens.Count > 0 ? _tokens.Dequeue() : JsonToken.Eof;
            }

            public IList<JsonToken> GetTokens(ref Utf8JsonReader jsonReader)
            {
                var result = new List<JsonToken>();
                bool hasNextToken;
                do
                {
                    hasNextToken = AddNextToken(result, ref jsonReader);
                }
                while (hasNextToken);
                return result;
            }

            private bool AddNextToken(IList<JsonToken> result, ref Utf8JsonReader jsonReader)
            {
                var token = NextToken(ref jsonReader);
                result.Add(token);
                return token.Kind != JsonTokenKind.Eof;
            }

            private void EnqueueCurrentJsonTokens(ref Utf8JsonReader jsonReader)
            {
                Debug.Assert(_tokens.Count == 0);
                if (_isEof)
                    return;

                var tokenType = jsonReader.TokenType;

                if (RestoreEatenComma(ref jsonReader))
                    _tokens.Enqueue(JsonToken.Comma);

                _tokens.Enqueue(GetSingleToken(ref jsonReader));
            }

            private JsonToken GetSingleToken(ref Utf8JsonReader jsonReader)
            {
                var tokenType = jsonReader.TokenType;

                if (tokenType == JsonTokenType.StartObject)
                {
                    EnterState(State.Object);
                    return JsonToken.CurlyOpen;
                }

                if (tokenType == JsonTokenType.EndObject)
                {
                    ExitState(State.Object);
                    return JsonToken.CurlyClose;
                }

                if (tokenType == JsonTokenType.StartArray)
                {
                    EnterState(State.Array);
                    return JsonToken.SquaredOpen;
                }

                if (tokenType == JsonTokenType.EndArray)
                {
                    ExitState(State.Array);
                    return JsonToken.SquaredClose;
                }

                if (tokenType == JsonTokenType.PropertyName)
                    return JsonToken.PropertyName(jsonReader.GetString());

                _flagPostValue = true;
                return GetValueToken(ref jsonReader);
            }

            private JsonToken GetValueToken(ref Utf8JsonReader jsonReader)
            {
                var tokenType = jsonReader.TokenType;

                if (tokenType == JsonTokenType.Null)
                    return JsonToken.Null;

                if (tokenType == JsonTokenType.True)
                    return JsonToken.True;

                if (tokenType == JsonTokenType.False)
                    return JsonToken.False;

                if (tokenType == JsonTokenType.String)
                    return JsonToken.String(jsonReader.GetString());

                var valueString = Encoding.UTF8.GetString(jsonReader.ValueSpan.ToArray());
                return JsonToken.Number(valueString);
            }
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsDataSet();
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, DataSet value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var jsonWriter = new JsonWriterAdapter(writer, null);
            jsonWriter.Write(value);
        }

        /// <inheritdoc/>
        public override DataSet Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonTokenizer = new JsonTokenizer(ref reader);
            var tokens = jsonTokenizer.GetTokens(ref reader);
            var jsonReader = new JsonReaderAdapter(tokens, null);
            var dataSet = CreateDataSet(typeToConvert);
            jsonReader.Deserialize(dataSet, true);

            return dataSet;
        }

        private class DummyModel : Model
        {
        }

        private static DataSet CreateDataSet(Type dataSetType)
        {
            var createMethod = dataSetType.GetMethod(nameof(DataSet<DummyModel>.Create), BindingFlags.Static | BindingFlags.Public);
            return (DataSet)createMethod.Invoke(null, new object[] { null });
        }
    }
}
