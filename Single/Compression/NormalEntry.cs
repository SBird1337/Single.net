using System.Collections.Generic;

namespace Single.Compression
{
    public class NormalEntry : ILzEntry
    {
        private readonly byte _value;

        public byte[] Value
        {
            get { return new[] {_value}; }
        }

        public bool Compressed
        {
            get { return false; }
        }

        public void Uncompress(List<byte> buffer)
        {
            buffer.Add(_value);
        }

        public NormalEntry(byte value)
        {
            _value = value;
        }
    }
}
