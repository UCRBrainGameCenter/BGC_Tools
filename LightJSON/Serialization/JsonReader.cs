using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;

namespace LightJson.Serialization
{
    using ErrorType = JsonParseException.ErrorType;

    /// <summary>
    /// Represents a reader that can read JsonValues.
    /// </summary>
    public sealed class JsonReader
    {
        private readonly TextScanner scanner;

        private JsonReader(TextReader reader)
        {
            scanner = new TextScanner(reader);
        }

        private string ReadJsonKey() => ReadString();

        private async Task<string> ReadJsonKeyAsync() => await ReadStringAsync();

        private JsonValue ReadJsonValue()
        {
            scanner.SkipWhitespace();

            char next = scanner.Peek();

            if (char.IsNumber(next))
            {
                return ReadNumber();
            }

            switch (next)
            {
                case '{': return ReadObject();

                case '[': return ReadArray();

                case '"': return ReadString();

                case '-': return ReadNumber();

                case 't':
                case 'f': return ReadBoolean();

                case 'n': return ReadNull();

                default:
                    throw new JsonParseException(
                        type: ErrorType.InvalidOrUnexpectedCharacter,
                        position: scanner.Position);
            }
        }

        private async Task<JsonValue> ReadJsonValueAsync()
        {
            scanner.SkipWhitespace();

            char next = scanner.Peek();

            if (char.IsNumber(next))
            {
                return await ReadNumberAsync();
            }

            switch (next)
            {
                case '{': return await ReadObjectAsync();

                case '[': return await ReadArrayAsync();

                case '"': return await ReadStringAsync();

                case '-': return await ReadNumberAsync();

                case 't':
                case 'f': return await ReadBooleanAsync();

                case 'n': return await ReadNullAsync();

                default:
                    throw new JsonParseException(
                        type: ErrorType.InvalidOrUnexpectedCharacter,
                        position: scanner.Position);
            }
        }

        private JsonValue ReadNull()
        {
            scanner.Assert("null");
            return JsonValue.Null;
        }

        private async Task<JsonValue> ReadNullAsync()
        {
            await scanner.AssertAsync("null");
            return JsonValue.Null;
        }

        private JsonValue ReadBoolean()
        {
            switch (scanner.Peek())
            {
                case 't':
                    scanner.Assert("true");
                    return true;

                case 'f':
                    scanner.Assert("false");
                    return false;

                default:
                    throw new JsonParseException(
                        type: ErrorType.InvalidOrUnexpectedCharacter,
                        position: scanner.Position);
            }
        }

        private async Task<JsonValue> ReadBooleanAsync()
        {
            switch (scanner.Peek())
            {
                case 't':
                    await scanner.AssertAsync("true");
                    return true;

                case 'f':
                    await scanner.AssertAsync("false");
                    return false;

                default:
                    throw new JsonParseException(
                        type: ErrorType.InvalidOrUnexpectedCharacter,
                        position: scanner.Position);
            }
        }

        private void ReadDigits(StringBuilder builder)
        {
            while (scanner.CanRead && char.IsDigit(scanner.Peek()))
            {
                builder.Append(scanner.Read());
            }
        }

        private async Task ReadDigitsAsync(StringBuilder builder)
        {
            while (scanner.CanRead && char.IsDigit(scanner.Peek()))
            {
                builder.Append(await scanner.ReadAsync());
            }
        }

        private JsonValue ReadNumber()
        {
            StringBuilder builder = new StringBuilder();

            if (scanner.Peek() == '-')
            {
                builder.Append(scanner.Read());
            }

            if (scanner.Peek() == '0')
            {
                builder.Append(scanner.Read());
            }
            else
            {
                ReadDigits(builder);
            }

            if (scanner.CanRead && scanner.Peek() == '.')
            {
                builder.Append(scanner.Read());
                ReadDigits(builder);
            }

            if (scanner.CanRead && char.ToLowerInvariant(scanner.Peek()) == 'e')
            {
                builder.Append(scanner.Read());

                char next = scanner.Peek();

                switch (next)
                {
                    case '+':
                    case '-':
                        builder.Append(scanner.Read());
                        break;
                }

                ReadDigits(builder);
            }

            return double.Parse(
                s: builder.ToString(),
                provider: CultureInfo.InvariantCulture);
        }

        private async Task<JsonValue> ReadNumberAsync()
        {
            StringBuilder builder = new StringBuilder();

            if (scanner.Peek() == '-')
            {
                builder.Append(await scanner.ReadAsync());
            }

            if (scanner.Peek() == '0')
            {
                builder.Append(await scanner.ReadAsync());
            }
            else
            {
                ReadDigits(builder);
            }

            if (scanner.CanRead && scanner.Peek() == '.')
            {
                builder.Append(await scanner.ReadAsync());
                ReadDigits(builder);
            }

            if (scanner.CanRead && char.ToLowerInvariant(scanner.Peek()) == 'e')
            {
                builder.Append(await scanner.ReadAsync());

                char next = scanner.Peek();

                switch (next)
                {
                    case '+':
                    case '-':
                        builder.Append(await scanner.ReadAsync());
                        break;
                }

                await ReadDigitsAsync(builder);
            }

            return double.Parse(
                s: builder.ToString(),
                provider: CultureInfo.InvariantCulture);
        }

        private string ReadString()
        {
            StringBuilder builder = new StringBuilder();

            scanner.Assert('"');

            while (true)
            {
                char c = scanner.Read();

                if (c == '\\')
                {
                    c = scanner.Read();

                    switch (char.ToLower(c))
                    {
                        case '"':  // "
                        case '\\': // \
                        case '/':  // /
                            builder.Append(c);
                            break;
                        case 'b':
                            builder.Append('\b');
                            break;
                        case 'f':
                            builder.Append('\f');
                            break;
                        case 'n':
                            builder.Append('\n');
                            break;
                        case 'r':
                            builder.Append('\r');
                            break;
                        case 't':
                            builder.Append('\t');
                            break;
                        case 'u':
                            builder.Append(ReadUnicodeLiteral());
                            break;
                        default:
                            throw new JsonParseException(
                                type: ErrorType.InvalidOrUnexpectedCharacter,
                                position: scanner.Position);
                    }
                }
                else if (c == '"')
                {
                    break;
                }
                else
                {
                    if (char.IsControl(c))
                    {
                        throw new JsonParseException(
                            type: ErrorType.InvalidOrUnexpectedCharacter,
                            position: scanner.Position);
                    }
                    else
                    {
                        builder.Append(c);
                    }
                }
            }

            return builder.ToString();
        }

        private async Task<string> ReadStringAsync()
        {
            StringBuilder builder = new StringBuilder();

            await scanner.AssertAsync('"');

            while (true)
            {
                char c = await scanner.ReadAsync();

                if (c == '\\')
                {
                    c = await scanner.ReadAsync();

                    switch (char.ToLower(c))
                    {
                        case '"':  // "
                        case '\\': // \
                        case '/':  // /
                            builder.Append(c);
                            break;
                        case 'b':
                            builder.Append('\b');
                            break;
                        case 'f':
                            builder.Append('\f');
                            break;
                        case 'n':
                            builder.Append('\n');
                            break;
                        case 'r':
                            builder.Append('\r');
                            break;
                        case 't':
                            builder.Append('\t');
                            break;
                        case 'u':
                            builder.Append(await ReadUnicodeLiteralAsync());
                            break;
                        default:
                            throw new JsonParseException(
                                type: ErrorType.InvalidOrUnexpectedCharacter,
                                position: scanner.Position);
                    }
                }
                else if (c == '"')
                {
                    break;
                }
                else
                {
                    if (char.IsControl(c))
                    {
                        throw new JsonParseException(
                            type: ErrorType.InvalidOrUnexpectedCharacter,
                            position: scanner.Position);
                    }
                    else
                    {
                        builder.Append(c);
                    }
                }
            }

            return builder.ToString();
        }

        private int ReadHexDigit()
        {
            switch (char.ToUpper(scanner.Read()))
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                case 'A': return 10;
                case 'B': return 11;
                case 'C': return 12;
                case 'D': return 13;
                case 'E': return 14;
                case 'F': return 15;
                default:
                    throw new JsonParseException(
                        type: ErrorType.InvalidOrUnexpectedCharacter,
                        position: scanner.Position);
            }
        }

        private async Task<int> ReadHexDigitAsync()
        {
            switch (char.ToUpper(await scanner.ReadAsync()))
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                case 'A': return 10;
                case 'B': return 11;
                case 'C': return 12;
                case 'D': return 13;
                case 'E': return 14;
                case 'F': return 15;
                default:
                    throw new JsonParseException(
                        type: ErrorType.InvalidOrUnexpectedCharacter,
                        position: scanner.Position);
            }
        }

        private char ReadUnicodeLiteral()
        {
            int value = 0;

            value += ReadHexDigit() * 4096; // 16^3
            value += ReadHexDigit() * 256;  // 16^2
            value += ReadHexDigit() * 16;   // 16^1
            value += ReadHexDigit();        // 16^0

            return (char)value;
        }

        private async Task<char> ReadUnicodeLiteralAsync()
        {
            int value = 0;

            value += (await ReadHexDigitAsync()) * 4096; // 16^3
            value += (await ReadHexDigitAsync()) * 256;  // 16^2
            value += (await ReadHexDigitAsync()) * 16;   // 16^1
            value += (await ReadHexDigitAsync());        // 16^0

            return (char)value;
        }

        private JsonObject ReadObject() => ReadObject(new JsonObject());

        private async Task<JsonObject> ReadObjectAsync() => await ReadObjectAsync(new JsonObject());

        private JsonObject ReadObject(JsonObject jsonObject)
        {
            scanner.Assert('{');

            scanner.SkipWhitespace();

            if (scanner.Peek() == '}')
            {
                scanner.Read();
            }
            else
            {
                while (true)
                {
                    scanner.SkipWhitespace();

                    string key = ReadJsonKey();

                    if (jsonObject.ContainsKey(key))
                    {
                        throw new JsonParseException(
                            type: ErrorType.DuplicateObjectKeys,
                            position: scanner.Position);
                    }

                    scanner.SkipWhitespace();

                    scanner.Assert(':');

                    scanner.SkipWhitespace();

                    JsonValue value = ReadJsonValue();

                    jsonObject.Add(key, value);

                    scanner.SkipWhitespace();

                    char next = scanner.Read();

                    if (next == '}')
                    {
                        break;
                    }
                    else if (next == ',')
                    {
                        continue;
                    }

                    throw new JsonParseException(
                        type: ErrorType.InvalidOrUnexpectedCharacter,
                        position: scanner.Position);
                }
            }

            return jsonObject;
        }

        private async Task<JsonObject> ReadObjectAsync(JsonObject jsonObject)
        {
            await scanner.AssertAsync('{');

            await scanner.SkipWhitespaceAsync();

            if (scanner.Peek() == '}')
            {
                await scanner.ReadAsync();
            }
            else
            {
                while (true)
                {
                    await scanner.SkipWhitespaceAsync();

                    string key = ReadJsonKey();

                    if (jsonObject.ContainsKey(key))
                    {
                        throw new JsonParseException(
                            type: ErrorType.DuplicateObjectKeys,
                            position: scanner.Position);
                    }

                    scanner.SkipWhitespace();

                    scanner.Assert(':');

                    scanner.SkipWhitespace();

                    JsonValue value = ReadJsonValue();

                    jsonObject.Add(key, value);

                    scanner.SkipWhitespace();

                    char next = scanner.Read();

                    if (next == '}')
                    {
                        break;
                    }
                    else if (next == ',')
                    {
                        continue;
                    }

                    throw new JsonParseException(
                        type: ErrorType.InvalidOrUnexpectedCharacter,
                        position: scanner.Position);
                }
            }

            return jsonObject;
        }

        private JsonArray ReadArray() => ReadArray(new JsonArray());
        private async Task<JsonArray> ReadArrayAsync() => await ReadArrayAsync(new JsonArray());

        private JsonArray ReadArray(JsonArray jsonArray)
        {
            scanner.Assert('[');

            scanner.SkipWhitespace();

            if (scanner.Peek() == ']')
            {
                scanner.Read();
            }
            else
            {
                while (true)
                {
                    scanner.SkipWhitespace();

                    JsonValue value = ReadJsonValue();

                    jsonArray.Add(value);

                    scanner.SkipWhitespace();

                    char next = scanner.Read();

                    if (next == ']')
                    {
                        break;
                    }
                    else if (next == ',')
                    {
                        continue;
                    }

                    throw new JsonParseException(
                        type: ErrorType.InvalidOrUnexpectedCharacter,
                        position: scanner.Position);
                }
            }

            return jsonArray;
        }

        private async Task<JsonArray> ReadArrayAsync(JsonArray jsonArray)
        {
            await scanner.AssertAsync('[');

            await scanner.SkipWhitespaceAsync();

            if (scanner.Peek() == ']')
            {
                await scanner.ReadAsync();
            }
            else
            {
                while (true)
                {
                    await scanner.SkipWhitespaceAsync();

                    JsonValue value = await ReadJsonValueAsync();

                    jsonArray.Add(value);

                    await scanner.SkipWhitespaceAsync();

                    char next = await scanner.ReadAsync();

                    if (next == ']')
                    {
                        break;
                    }
                    else if (next == ',')
                    {
                        continue;
                    }

                    throw new JsonParseException(
                        type: ErrorType.InvalidOrUnexpectedCharacter,
                        position: scanner.Position);
                }
            }

            return jsonArray;
        }

        private JsonValue Parse()
        {
            scanner.SkipWhitespace();
            return ReadJsonValue();
        }

        private async Task<JsonValue> ParseAsync()
        {
            scanner.SkipWhitespace();
            return await ReadJsonValueAsync();
        }

        /// <summary>
        /// Creates a JsonValue by using the given TextReader.
        /// </summary>
        /// <param name="reader">The TextReader used to read a JSON message.</param>
        public static JsonValue Parse(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            return new JsonReader(reader).Parse();
        }

        /// <summary>
        /// Creates a JsonValue by using the given TextReader.
        /// </summary>
        /// <param name="reader">The TextReader used to read a JSON message.</param>
        public static async Task<JsonValue> ParseAsync(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            return await new JsonReader(reader).ParseAsync();
        }

        /// <summary>
        /// Creates a JsonValue by reader the JSON message in the given string.
        /// </summary>
        /// <param name="source">The string containing the JSON message.</param>
        public static JsonValue Parse(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            using (StringReader reader = new StringReader(source))
            {
                return new JsonReader(reader).Parse();
            }
        }

        /// <summary>
        /// Creates a JsonValue by reader the JSON message in the given string.
        /// </summary>
        /// <param name="source">The string containing the JSON message.</param>
        public static async Task<JsonValue> ParseAsync(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            using (StringReader reader = new StringReader(source))
            {
                return await new JsonReader(reader).ParseAsync();
            }
        }

        /// <summary>
        /// Creates a JsonValue by reading the given file.
        /// </summary>
        /// <param name="path">The file path to be read.</param>
        public static JsonValue ParseFile(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            using (StreamReader reader = new StreamReader(path))
            {
                return new JsonReader(reader).Parse();
            }
        }

        /// <summary>
        /// Creates a JsonValue by reading the given file.
        /// </summary>
        /// <param name="path">The file path to be read.</param>
        public static async Task<JsonValue> ParseFileAsync(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            using (StreamReader reader = new StreamReader(path))
            {
                return await new JsonReader(reader).ParseAsync();
            }
        }
    }
}
