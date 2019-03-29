using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using JsonToken = DevZest.Data.Primitives.JsonToken;
using TokenType = Newtonsoft.Json.JsonToken;

namespace DevZest.Data.AspNetCore.Primitives
{
    internal class JsonReaderAdapter : Data.Primitives.JsonReader
    {
        public JsonReaderAdapter(JsonReader jsonReader, IJsonCustomizer jsonCustomizer)
            : base(jsonCustomizer)
        {
            _jsonReader = jsonReader;
        }

        private enum State
        {
            Object,
            Array
        }

        private readonly JsonReader _jsonReader;
        private bool _flagPropertyName;
        private Stack<State> _states = new Stack<State>();
        private bool _flagPostValue;

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

        private bool RestoreEatenComma()
        {
            if (!_flagPostValue)
                return false;

            _flagPostValue = false;

            var currentState = CurrentState;
            var tokenType = _jsonReader.TokenType;
            return (currentState == State.Array && tokenType != TokenType.EndArray)
                || (currentState == State.Object && tokenType != TokenType.EndObject);
        }

        protected override JsonToken NextToken()
        {
            if (_flagPropertyName)
            {
                _flagPropertyName = false;
                return JsonToken.Colon;
            }

            if (!_jsonReader.Read())
                return JsonToken.Eof;

            if (RestoreEatenComma())
                return JsonToken.Comma;

            var tokenType = _jsonReader.TokenType;

            if (tokenType == TokenType.StartObject)
            {
                EnterState(State.Object);
                return JsonToken.CurlyOpen;
            }

            if (tokenType == TokenType.EndObject)
            {
                ExitState(State.Object);
                return JsonToken.CurlyClose;
            }

            if (tokenType == TokenType.StartArray)
            {
                EnterState(State.Array);
                return JsonToken.SquaredOpen;
            }

            if (tokenType == TokenType.EndArray)
            {
                ExitState(State.Array);
                return JsonToken.SquaredClose;
            }

            if (tokenType == TokenType.PropertyName)
            {
                _flagPropertyName = true;
                return JsonToken.String(_jsonReader.Value.ToString());
            }

            _flagPostValue = true;
            return GetValueToken();
        }

        private JsonToken GetValueToken()
        {
            var tokenType = _jsonReader.TokenType;
            var value = _jsonReader.Value;

            if (tokenType == TokenType.Null)
                return JsonToken.Null;

            if (tokenType == TokenType.Boolean)
                return (bool)value ? JsonToken.True : JsonToken.False;

            if (tokenType == TokenType.String)
                return JsonToken.String((string)value);

            var converter = TypeDescriptor.GetConverter(value.GetType());
            var text = converter.ConvertToInvariantString(value);
            if (IsNumber(tokenType))
                return JsonToken.Number(text);
            else
                return JsonToken.String(text);
        }

        private static bool IsNumber(TokenType tokenType)
        {
            return tokenType == TokenType.Float || tokenType == TokenType.Integer;
        }

        public override void ExpectEof()
        {
        }
    }
}
