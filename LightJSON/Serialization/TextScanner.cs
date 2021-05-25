using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LightJson.Serialization
{
    using ErrorType = JsonParseException.ErrorType;

    /// <summary>
    /// Represents a text scanner that reads one character at a time.
    /// </summary>
    public sealed class TextScanner
    {
        private TextReader reader;
        private TextPosition position;

        /// <summary>The position of the scanner within the text.</summary>
        public TextPosition Position => position;

        /// <summary>Indicates whether there are still characters to be read.</summary>
        public bool CanRead => reader.Peek() != -1;

        /// <summary>
        /// Initializes a new instance of TextScanner.
        /// </summary>
        /// <param name="reader">The TextReader to read the text.</param>
        public TextScanner(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            this.reader = reader;
        }

        /// <summary>
        /// Reads the next character in the stream without changing the current position.
        /// </summary>
        public char Peek()
        {
            int next = reader.Peek();

            if (next == -1)
            {
                throw new JsonParseException(
                    type: ErrorType.IncompleteMessage,
                    position: position);
            }

            return (char)next;
        }

        /// <summary>
        /// Reads the next character in the stream, advancing the text position.
        /// </summary>
        public char Read()
        {
            int next = reader.Read();

            if (next == -1)
            {
                throw new JsonParseException(
                    type: ErrorType.IncompleteMessage,
                    position: position);
            }

            switch (next)
            {
                case '\r':
                    // Normalize '\r\n' line encoding to '\n'.
                    if (reader.Peek() == '\n')
                    {
                        reader.Read();
                    }
                    goto case '\n';

                case '\n':
                    position.line += 1;
                    position.column = 0;
                    return '\n';

                default:
                    position.column += 1;
                    return (char)next;
            }
        }

        /// <summary>
        /// Reads the next character in the stream, advancing the text position.
        /// </summary>
        public async Task<char> ReadAsync()
        {
            char[] buffer = new char[1];

            int next = await reader.ReadAsync(buffer, 0, buffer.Length);

            if (next == -1)
            {
                throw new JsonParseException(
                    type: ErrorType.IncompleteMessage,
                    position: position);
            }

            switch (next)
            {
                case '\r':
                    // Normalize '\r\n' line encoding to '\n'.
                    if (reader.Peek() == '\n')
                    {
                        await reader.ReadAsync(buffer, 0, buffer.Length);
                    }
                    goto case '\n';

                case '\n':
                    position.line += 1;
                    position.column = 0;
                    return '\n';

                default:
                    position.column += 1;
                    return (char)next;
            }
        }

        /// <summary>
        /// Advances the scanner to next non-whitespace character.
        /// </summary>
        public void SkipWhitespace()
        {
            while (char.IsWhiteSpace(Peek()))
            {
                Read();
            }
        }

        /// <summary>
        /// Advances the scanner to next non-whitespace character.
        /// </summary>
        public async Task SkipWhitespaceAsync()
        {
            while (char.IsWhiteSpace(Peek()))
            {
                await ReadAsync();
            }
        }

        /// <summary>
        /// Verifies that the given character matches the next character in the stream.
        /// If the characters do not match, an exception will be thrown.
        /// </summary>
        /// <param name="next">The expected character.</param>
        public void Assert(char next)
        {
            if (Peek() == next)
            {
                Read();
            }
            else
            {
                throw new JsonParseException(
                    message: $"Parser expected '{next}', found '{Peek()}'",
                    type: ErrorType.InvalidOrUnexpectedCharacter,
                    position: position);
            }
        }

        /// <summary>
        /// Verifies that the given character matches the next character in the stream.
        /// If the characters do not match, an exception will be thrown.
        /// </summary>
        /// <param name="next">The expected character.</param>
        public async Task AssertAsync(char next)
        {
            if (Peek() == next)
            {
                await ReadAsync();
            }
            else
            {
                throw new JsonParseException(
                    message: $"Parser expected '{next}', found '{Peek()}'",
                    type: ErrorType.InvalidOrUnexpectedCharacter,
                    position: position);
            }
        }

        /// <summary>
        /// Verifies that the given string matches the next characters in the stream.
        /// If the strings do not match, an exception will be thrown.
        /// </summary>
        /// <param name="next">The expected string.</param>
        public void Assert(string next)
        {
            try
            {
                for (int i = 0; i < next.Length; i += 1)
                {
                    Assert(next[i]);
                }
            }
            catch (JsonParseException e) when (e.Type == ErrorType.InvalidOrUnexpectedCharacter)
            {
                throw new JsonParseException(
                    message: $"Parser expected '{next}'",
                    type: ErrorType.InvalidOrUnexpectedCharacter,
                    position: position);
            }
        }

        /// <summary>
        /// Verifies that the given string matches the next characters in the stream.
        /// If the strings do not match, an exception will be thrown.
        /// </summary>
        /// <param name="next">The expected string.</param>
        public async Task AssertAsync(string next)
        {
            try
            {
                for (int i = 0; i < next.Length; i += 1)
                {
                    await AssertAsync(next[i]);
                }
            }
            catch (JsonParseException e) when (e.Type == ErrorType.InvalidOrUnexpectedCharacter)
            {
                throw new JsonParseException(
                    message: $"Parser expected '{next}'",
                    type: ErrorType.InvalidOrUnexpectedCharacter,
                    position: position);
            }
        }
    }
}
