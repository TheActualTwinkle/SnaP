using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SnaPDataTransfer
{
    public class StreamString
    {
        private readonly Stream _ioStream;
        private readonly UnicodeEncoding _streamEncoding;

        public StreamString(Stream ioStream)
        {
            this._ioStream = ioStream;
            _streamEncoding = new UnicodeEncoding();
        }

        public async Task<string> ReadStringAsync()
        {
            var buffer = new byte[2];
            await _ioStream.ReadAsync(buffer, 0, 2); // Waiting for request to appear.

            int len = buffer[0] * 256;
            len += _ioStream.ReadByte();

            if (len < 0)
            {
                return "404";
            }

            //len++;
            byte[] inBuffer = new byte[len];
            await _ioStream.ReadAsync(inBuffer, 0, len);

            return _streamEncoding.GetString(inBuffer);
        }

        public async Task<int> WriteStringAsync(string outString)
        {
            byte[] outBuffer = _streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            _ioStream.WriteByte((byte)(len / 256));
            _ioStream.WriteByte((byte)(len & 255));
            await _ioStream.WriteAsync(outBuffer, 0, len);
            await _ioStream.FlushAsync();

            return outBuffer.Length + 2;
        }
    }
}