using System.Collections.Generic;
using System.IO;
using Single.Core;

namespace Single.Compression
{
    public class LzCompound : IRomWritable
    {
        private readonly ILzEntry[] _entries = new ILzEntry[8];

        public ILzEntry[] Entries
        {
            get { return _entries; }
        }

        public LzCompound()
        {
            for (int i = 0; i < _entries.Length; ++i)
            {
                _entries[i] = new NormalEntry(0);
            }
        }

        public LzCompound(BinaryReader reader)
        {
            byte decoder = reader.ReadByte();
            for (int i = 0; i < 8; ++i)
            {
                if ((decoder & (1 << 7 - i)) != 0)
                {
                    _entries[i] = new CompressedEntry((ushort) ((reader.ReadByte() << 8) | (reader.ReadByte())));
                }
                else
                {
                    _entries[i] = new NormalEntry(reader.ReadByte());
                }
            }
        }

        public byte[] GetRawData()
        {
            byte status = 0;
            List<byte> output = new List<byte>();
            
            for (int i = 0; i < _entries.Length; ++i)
            {
                if (_entries[i].Compressed)
                    status |= (byte) (1 >> i);
                output.AddRange(_entries[i].Value);
            }
            output.Insert(0, status);
            return output.ToArray();
        }

        public void Decode(List<byte> buffer)
        {
            foreach (ILzEntry entry in _entries)
            {
                entry.Uncompress(buffer);
            }
        }
    }
}
