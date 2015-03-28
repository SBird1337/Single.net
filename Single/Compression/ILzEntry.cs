using System.Collections.Generic;

namespace Single.Compression
{
    public interface ILzEntry
    {
        byte[] Value { get; }
        bool Compressed { get; }
        void Uncompress(List<byte> buffer);
    }
}
