using System;
using System.Drawing;
using Single.Core;

namespace Single.Graphics
{
    public class PaletteEntry : IRomWritable
    {
        public ushort Data { get; private set; }

        public PaletteEntry(ushort data)
        {
            Data = data;
        }

        public PaletteEntry(byte red, byte green, byte blue)
        {
            Data = (ushort)((red / 8) | ((green / 8) << 5) | ((blue / 8) << 10));
        }

        public byte[] GetRawData()
        {
            return BitConverter.GetBytes(Data);
        }

        public Color ToColor()
        {
            return Color.FromArgb(Data & 0x1F, (Data >> 8) & 0x1F, Data >> 16);
        }
    }
}
