using System.IO;
using Vostok.Commons.Threading;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Extensions.Http;

namespace Vostok.Clusterclient.Tracing.Helpers
{
    internal class ProxyStream : Stream
    {
        private readonly Stream stream;
        private readonly IHttpRequestSpanBuilder builder;
        private long? read;
        private readonly AtomicBoolean disposed = false;

        public ProxyStream(Stream stream, IHttpRequestSpanBuilder builder)
        {
            this.stream = stream;
            this.builder = builder;
        }

        public override bool CanRead => stream.CanRead;
        public override bool CanSeek => stream.CanSeek;
        public override bool CanWrite => stream.CanWrite;
        public override long Length => stream.Length;
        public override long Position
        {
            get => stream.Position;
            set => stream.Position = value;
        }

        public override void Flush() =>
            stream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            var result = stream.Read(buffer, offset, count);
            read = (read ?? 0) + result;
            return result;
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            stream.Seek(offset, origin);

        public override void SetLength(long value) =>
            stream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) =>
            stream.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            if (!disposed.TrySetTrue())
                return;

            using (builder)
            using (stream)
            {
                if (read.HasValue)
                    builder.SetAnnotation(WellKnownAnnotations.Http.Response.Size, read.Value);
            }
        }
    }
}