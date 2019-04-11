using System;
using System.Collections.Generic;
using System.IO;

namespace BGC.IO.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ReadRemainder(this Stream stream)
        {
            long count = stream.GetRemaining();
            byte[] data = new byte[count];

            if (count <= int.MaxValue)
            {
                stream.Read(data, 0, (int)count);
            }
            else
            {
                long offset = 0;
                const int BUFFER_SIZE = 512;
                byte[] buffer = new byte[BUFFER_SIZE];
                int bytesRead;
                do
                {
                    bytesRead = stream.Read(
                        buffer: buffer,
                        offset: 0,
                        count: (int)Math.Min(BUFFER_SIZE, count));

                    Array.Copy(
                        sourceArray: buffer,
                        sourceIndex: 0,
                        destinationArray: data,
                        destinationIndex: offset,
                        length: bytesRead);

                    offset += bytesRead;
                    count -= bytesRead;

                    if (bytesRead == 0)
                    {
                        break;
                    }
                }
                while (count > 0);
            }

            return data;
        }

        public static long GetRemaining(this Stream stream) => stream.Length - stream.Position;

        public static IEnumerable<T> ParseAll<T>(this Stream stream, Func<Stream, T> parser)
        {
            while (stream.Position < stream.Length)
            {
                yield return parser(stream);
            }
        }
    }
}
