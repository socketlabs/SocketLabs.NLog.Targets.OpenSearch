using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SocketLabs.NLog.Targets.OpenSearch.Utilities
{
    internal class ReadOnlyMemoryContent : HttpContent
    {
        private readonly Memory<byte> _memory;

        public ReadOnlyMemoryContent(Memory<byte> memory)
        {
            _memory = memory;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            var buffer = _memory.ToArray();
            return stream.WriteAsync(buffer, 0, buffer.Length);
        }

#if (NET5_0_OR_GREATER)
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
        {
            var buffer = _memory.ToArray();
            return stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
        }
#endif

        protected override bool TryComputeLength(out long length)
        {
            length = _memory.Length;
            return true;
        }
    }
}