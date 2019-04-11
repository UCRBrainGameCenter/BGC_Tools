using System;
using System.Collections.Generic;
using System.IO;
using BGC.Mathematics;

namespace BGC.IO
{
    /// <summary>
    /// Wraps a stream and grants access only to a subsection
    /// Modified from code found here: 
    /// https://social.msdn.microsoft.com/Forums/vstudio/en-US/c409b63b-37df-40ca-9322-458ffe06ea48/how-to-access-part-of-a-filestream-or-memorystream?forum=netfxbcl
    /// </summary>
    public class SubStream : Stream
    {
        private readonly bool ownsStream;
        private readonly long length;
        private readonly long startPosition;

        private Stream baseStream = null;
        private long position = 0;

        public override long Length
        {
            get
            {
                CheckDisposed();
                return length;
            }
        }

        public override bool CanRead
        {
            get
            {
                CheckDisposed();
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                CheckDisposed();
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                CheckDisposed();
                return baseStream.CanSeek;
            }
        }

        public override long Position
        {
            get
            {
                CheckDisposed();
                return position;
            }
            set => Seek(value, SeekOrigin.Begin);
        }

        public SubStream(
            Stream baseStream,
            long startPosition,
            long length,
            bool ownsStream = false)
        {
            if (baseStream == null)
            {
                throw new ArgumentNullException(nameof(baseStream));
            }

            if (!baseStream.CanRead)
            {
                throw new ArgumentException("Can't read BaseStream");
            }

            if (startPosition < 0 || startPosition + length > baseStream.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startPosition));
            }

            this.baseStream = baseStream;
            this.length = length;
            this.ownsStream = ownsStream;
            this.startPosition = startPosition;
            position = 0;

            if (baseStream.Position != startPosition)
            {
                if (baseStream.CanSeek)
                {
                    baseStream.Seek(startPosition, SeekOrigin.Begin);
                }
                else if (baseStream.Position > startPosition)
                {
                    //Can't seek and we're past the start position
                    throw new NotSupportedException("BaseStream can't seek backwards to StartPosition");
                }
                else
                {
                    // fast forward manually...
                    const int BUFFER_SIZE = 512;
                    byte[] buffer = new byte[BUFFER_SIZE];
                    long deltaPosition = startPosition - baseStream.Position;
                    while (deltaPosition > 0)
                    {
                        int read = baseStream.Read(
                            buffer: buffer,
                            offset: 0,
                            count: deltaPosition < BUFFER_SIZE ? (int)deltaPosition : BUFFER_SIZE);

                        deltaPosition -= read;

                        if (read == 0)
                        {
                            throw new Exception("Failed to read past required samples");
                        }
                    }
                }
            }
        }

        public SubStream(
            Stream baseStream,
            long length,
            bool ownsStream = false)
        {
            if (baseStream == null)
            {
                throw new ArgumentNullException("baseStream");
            }

            if (!baseStream.CanRead)
            {
                throw new ArgumentException("Can't read base stream");
            }

            this.baseStream = baseStream;
            this.length = length;
            this.ownsStream = ownsStream;
            startPosition = baseStream.Position;
            position = 0;
        }

        public override int ReadByte()
        {
            if (length - position <= 0)
            {
                return -1;
            }

            position++;

            return baseStream.ReadByte();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();

            count = (int)Math.Min(count, length - position);
            if (count <= 0)
            {
                return 0;
            }

            int read = baseStream.Read(buffer, offset, count);
            position += read;

            return read;
        }

        public override void Flush()
        {
            CheckDisposed();
            baseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!CanSeek)
            {
                throw new NotSupportedException("BaseStream does not support seeking");
            }

            switch (origin)
            {
                case SeekOrigin.Begin:
                    offset = GeneralMath.Clamp(offset, 0, length);
                    position = offset;
                    offset += startPosition;
                    break;

                case SeekOrigin.Current:
                    offset = GeneralMath.Clamp(offset, -position, length - position);
                    position += offset;
                    break;

                case SeekOrigin.End:
                    offset = GeneralMath.Clamp(offset, -length, 0);
                    position = length + offset;
                    offset -= baseStream.Length - (startPosition + Length);
                    break;

                default:
                    throw new NotSupportedException($"SeekOrigin not implemented: {origin}");
            }

            baseStream.Seek(offset, origin);

            return position;
        }


        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        private void CheckDisposed()
        {
            if (_disposed || baseStream == null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        #region IDisposable

        private bool _disposed = false;
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (baseStream != null)
                {
                    if (ownsStream)
                    {
                        try
                        {
                            baseStream.Dispose();
                        }
                        catch
                        {
                            //Dodge dispose exceptions
                        }
                    }
                    else
                    {
                        //Try advancing stream to end
                        if (baseStream.CanSeek)
                        {
                            baseStream.Seek(startPosition + length, SeekOrigin.Begin);
                        }
                    }

                    baseStream = null;
                }
            }

            _disposed = true;
        }

        #endregion IDisposable
    }
}
