using System;
using System.Collections.Generic;

namespace Single.Compression
{
    public class CompressedEntry : ILzEntry
    {
        private readonly ushort _backPosition;
        private readonly ushort _backLength;

        public byte[] Value
        {
            get { return BitConverter.GetBytes(Codec); }
        }

        public ushort Codec
        {
            get
            {
                return (ushort) (((_backLength - 3) << 12) | (_backPosition - 1));
            }
        }

        public bool Compressed
        {
            get { return true; }
        }

        public void Uncompress(List<byte> buffer)
        {
            List<byte> backBuffer = new List<byte>();
            int index = buffer.Count - _backPosition;
            while (backBuffer.Count < _backLength)
            {
                backBuffer.Add(buffer[index]);
                index++;
                if (index >= buffer.Count)
                    index = buffer.Count - _backPosition;
            }
            buffer.AddRange(backBuffer);
        }

        public CompressedEntry(byte backPosition, byte backLength)
        {
            _backLength = (ushort) (backLength + 1);
            _backPosition = (ushort) (backPosition + 3);
        }

        public CompressedEntry(ushort codec)
        {
            _backPosition = (ushort) (((codec & 0xFFF)) + 1);
            _backLength = (ushort) ((codec >> 12) + 3);
        }
    }
}
