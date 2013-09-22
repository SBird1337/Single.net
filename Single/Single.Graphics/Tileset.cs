using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Single.Core;
using Single.Compression;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Single.Graphics
{
    public class Tileset : IRepointable
    {
        #region Fields

        private bool is8bpp;
        private bool isRepointable = false;
        private UInt32 currentOffset;
        private int lenght;

        #endregion

        #region Properties

        public List<Tile> Tiles
        { get; set; }

        public bool IsEncoded
        { get; set; }

        #endregion

        #region Constructors

        private Tileset(byte[] data, bool isEncoded, bool is8bpp = false, bool isRepointable = false, int initLenght = 0, uint initoffset = 0)
        {
            this.is8bpp = is8bpp;
            this.isRepointable = isRepointable;
            this.lenght = initLenght;
            this.IsEncoded = isEncoded;
            this.currentOffset = initoffset;
            Tiles = new List<Tile>();
            int multiplier = 32;
            if (is8bpp)
                multiplier = 64;
            List<Byte> tiledata = data.ToList();
            if (((tiledata.Count % multiplier) == 0))
            {
                for (int i = 0; i < (int)(tiledata.Count / multiplier); i++)
                {
                    Tiles.Add(new Tile(tiledata.GetRange(multiplier * i, multiplier), is8bpp));
                }
            }
            else
            {
                throw new Exception("Die angegebene Datenmenge ist kein Tileset");
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Erstellt ein Tileset, eine Palette und eine Tilemap aus dem indizierten Bitmap
        /// </summary>
        /// <param name="bmp">Indiziertes Bitmap Objekt, muss beidseitig durch 8 teilbar sein</param>
        /// <param name="pal">Zurückgegebene Palette</param>
        /// <param name="map">Zurückgegebene Tilemap</param>
        /// <returns>Tileset, welches durch das Bitmap definiert wurde</returns>
        public static Tileset FromBitmap(Bitmap bmp, bool isEncoded, out Palette pal, out Tilemap map, byte paletteMap = 0)
        {
            if (bmp.Width % 8 == 0 && bmp.Height % 8 == 0)
            {
                Tileset set = Tileset.FromBitmap(bmp, out pal, isEncoded);
                map = new Tilemap();
                Dictionary<ushort, ushort> mapdict = new Dictionary<ushort, ushort>();
                List<Tile> firstlist = new List<Tile>();
                for (ushort u = 0; u < set.Tiles.Count; u++)
                {
                    Tile current = set.Tiles[u];
                    if (firstlist.Contains(current))
                    {
                        mapdict.Add(u, (ushort)(firstlist.IndexOf(current)));
                    }
                    else
                    {
                        firstlist.Add(current);
                        mapdict.Add(u, (ushort)(firstlist.Count - 1));
                    }
                }
                set.Tiles = firstlist;
                for (ushort u = 0; u < mapdict.Keys.Count; ++u)
                {
                    map.Entries.Add(new TilemapEntry((ushort)(mapdict[u]), paletteMap, false, false));
                }
                return set;
            }
            else
            {
                throw new Exception("Das angegebene Bitmap hat nicht das Format eines Tilesets. (W=a*8; H=a*8)");
            }
        }

        /// <summary>
        /// Erstellt ein Tileset und eine Palette aus dem angegebenen indizierten Bitmap
        /// </summary>
        /// <param name="bmp">Indiziertes Bitmap Objekt, muss beidseitig durch 8 teilbar sein</param>
        /// <param name="pal">Zurückgegebene Palette</param>
        /// <returns>Tileset, welches durch das Bitmap definiert wurde</returns>
        public static Tileset FromBitmap(Bitmap bmp, out Palette pal, bool isEncoded)
        {
            if (bmp.Width % 8 == 0 && bmp.Height % 8 == 0)
            {
                List<byte> tiles = new List<byte>();
                List<Color> colorEntries = bmp.Palette.Entries.ToList();
                pal = new Palette(bmp.Palette.Entries);
                if (bmp.PixelFormat == PixelFormat.Format4bppIndexed)
                {
                    for (int y = 0; y < bmp.Height; y += 8)
                    {
                        for (int x = 0; x < bmp.Width; x += 8)
                        {

                            for (int y1 = 0; y1 < 8; y1++)
                            {
                                for (int x1 = 0; x1 < 8; x1 += 2)
                                {
                                    Color c1 = bmp.GetPixel(x + x1, y + y1);
                                    Color c2 = bmp.GetPixel(x + x1 + 1, y + y1);

                                    byte b2 = (byte)colorEntries.IndexOf(c1);
                                    byte b1 = (byte)colorEntries.IndexOf(c2);
                                    tiles.Add((byte)((b1 << 4) | b2));
                                }
                            }
                        }
                    }
                    return new Tileset(tiles.ToArray(), isEncoded);
                }
                else if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    for (int y = 0; y < bmp.Height; y += 8)
                    {
                        for (int x = 0; x < bmp.Width; x += 8)
                        {
                            for (int y1 = 0; y1 < 8; y1++)
                            {
                                for (int x1 = 0; x1 < 8; x1++)
                                {
                                    Color c1 = bmp.GetPixel(x + x1, y + y1);
                                    tiles.Add((byte)colorEntries.IndexOf(c1));
                                }
                            }
                        }
                    }
                    return new Tileset(tiles.ToArray(), isEncoded, true);
                }
                else
                {
                    throw new Exception("Image is not indexed");
                }

            }
            else
            {
                throw new Exception("Das angegebene Bitmap hat nicht das Format eines Tilesets. (W=a*8; H=a*8)");
            }
        }

        /// <summary>
        /// Erstellt ein Tileset aus dem angegebenen Byte Array
        /// </summary>
        /// <param name="input">Rohdaten des Tilesets</param>
        /// <param name="is8bpp">Wenn True: Es wird versucht ein 8bpp Tileset zu erstellen</param>
        /// <returns>Tileset Objekt</returns>
        public static Tileset FromByteArray(byte[] input, bool isEncoded, bool is8bpp = false)
        {
            return new Tileset(input, isEncoded, is8bpp);
        }

        /// <summary>
        /// Erstellt ein Tileset von der angegebenen Adresse in input. Daten werden vorher dekomprimiert
        /// </summary>
        /// <param name="input">Rom mit den komprimierten Tileset Daten</param>
        /// <param name="offset">Offset der komprimierten Tileset Daten</param>
        /// <param name="is8bpp">Wenn True: Es wird versucht ein 8bpp Tileset zu erstellen</param>
        /// <returns>Tileset Objekt</returns>
        public static Tileset FromCompressedAddress(Rom input, UInt32 offset, bool is8bpp = false)
        {
            int len;
            List<Byte> tiledata = RomDecode.unlzFromOffset(input, offset, out len);
            return new Tileset(tiledata.ToArray(), true, is8bpp, true, len, offset);
        }

        /// <summary>
        /// Erstellt ein Tileset von der angegebenen Adresse in input
        /// </summary>
        /// <param name="input">Rom mit den unkomprimierten Tileset Daten</param>
        /// <param name="offset">Offset der unkomprimierten Tileset Daten</param>
        /// <param name="tileCount">Anzahl der Tiles im Tileset</param>
        /// <param name="is8bpp">Wenn True: Es wird versucht ein 8bpp Tileset zu erstellen</param>
        /// <returns>Tileset Objekt</returns>
        public static Tileset FromUncompressedAddress(Rom input, UInt32 offset, int tileCount, bool is8bpp = false)
        {
            int multiplier = 32;
            if (is8bpp)
                multiplier = 64;
            List<Byte> tiledata;
            input.SetStreamOffset(offset);
            tiledata = input.ReadByteArray(tileCount * multiplier);
            return new Tileset(tiledata.ToArray(), false, is8bpp, true, tileCount * multiplier, offset);
        }

        /// <summary>
        /// Gibt das angegebene Tile Objekt zurück
        /// </summary>
        /// <param name="index">Index, größer als Null und kleiner als die Gesamtmenge der Tiles sein</param>
        /// <returns>Tile Objekt</returns>
        public Tile GetTileFromIndex(int index)
        {
            if (index < Tiles.Count)
            {
                return Tiles[index];
            }
            return Tiles[0];
        }

        /// <summary>
        /// Gibt die Anzahl der Tiles zurück
        /// </summary>
        /// <returns>Anzahl der vorhandenen Tiles im Tileset</returns>
        public int GetTileCount()
        {
            return this.Tiles.Count;
        }

        /// <summary>
        /// Erzeugt ein Bitmap, welches das Tileset repräsentiert
        /// </summary>
        /// <param name="imgwidth">Breite des Bildes in Tiles</param>
        /// <param name="pal">Zur Darstellung verwendete Palette</param>
        /// <returns>Bitmap, welches das Tileset repräsentiert</returns>
        public Bitmap ToBitmap(int imgwidth, Palette pal)
        {
            if (is8bpp)
            {
                throw new NotImplementedException();
            }
            Bitmap output;
            if (is8bpp)
            {
                output = new Bitmap(imgwidth * 8, (int)(Math.Ceiling((decimal)((decimal)Tiles.Count / (decimal)imgwidth))) * 8, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            }
            else
            {
                output = new Bitmap(imgwidth * 8, (int)(Math.Ceiling((decimal)((decimal)Tiles.Count / (decimal)imgwidth))) * 8, System.Drawing.Imaging.PixelFormat.Format4bppIndexed);
            }
            BitmapData data;
            ColorPalette outpal = output.Palette;
            for (int i = 0; i < pal.Entries.Count(); ++i)
            {
                outpal.Entries[i] = pal.Entries[i];
            }
            output.Palette = outpal;
            if (this.is8bpp)
            {
                data = output.LockBits(new Rectangle(new Point(0, 0), new Size(output.Width, output.Height)), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            }
            else
            {
                data = output.LockBits(new Rectangle(new Point(0, 0), new Size(output.Width, output.Height)), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format4bppIndexed);
            }

            
            byte[] bytes = new byte[data.Height * data.Stride];
            byte[] write = this.GetTileData();
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            
            int w = output.Width / 2;
            int h = output.Height;
            int t = 0;
            for (int y = 0; y < h; y += 8)
            {
                for (int x = 0; x < w; x+= 4)
                {
                    byte[] tdata = Tiles[t].GetRawData();
                    for(int y1 = 0; y1 < 8; ++y1)
                    {
                        for(int x1 = 0; x1 < 4; ++x1)
                        {
                            byte b1 = (byte)(tdata[(y1 * 4) + x1] << 4);
                            byte b2 = (byte)(tdata[(y1 * 4) + x1] >> 4);
                            bytes[((y + y1) * w) + (x + x1)] = (byte)(b1 | b2);
                        }
                    }
                    t++;
                }
            }

            Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            output.UnlockBits(data);
            return output;
        }

        private int ceiling(decimal val)
        {
            return (int)Math.Ceiling((decimal)val);
        }

        /// <summary>
        /// Implementation des IRomWritable Interfaces, gibt die Daten des Tilesets zurück wie sie im Rom stehen würden
        /// </summary>
        /// <returns>Byte Array mit den Tileset Daten</returns>
        public byte[] GetRawData()
        {
            List<byte> output = new List<byte>();
            foreach (Tile t in this.Tiles)
            {
                output.AddRange(t.GetRawData());
            }
            if (!this.IsEncoded)
            {
                return output.ToArray();
            }
            return RomDecode.lzCompressData(output.ToArray()).ToArray();
        }

        /// <summary>
        /// Gibt ein unkomprimiertes Array der Tile Daten zurück.
        /// </summary>
        /// <returns>Byte Array</returns>
        public byte[] GetTileData()
        {
            List<byte> output = new List<byte>();
            foreach (Tile t in this.Tiles)
            {
                output.AddRange(t.GetRawData());
            }
            return output.ToArray();
        }

        /// <summary>
        /// Gibt die Länge des Tilesets zurück, nur möglich wenn das Objekt im Repointable ist
        /// </summary>
        /// <returns>Länge des Tilesets in Bytes</returns>
        public int GetSize()
        {
            if (!isRepointable)
                throw new Exception("Das Objekt kann nicht gerepointet werden, die Funktionen von IRepointable stehen nicht zur Verfügung.");
            return this.lenght;
        }

        /// <summary>
        /// Gibt das Offset des Tilesets zurück, nur möglich wenn das Objekt Repointable ist
        /// </summary>
        /// <returns>Offset des Tilesets in Kontext auf ein Rom</returns>
        public uint GetCurrentOffset()
        {
            if (!isRepointable)
                throw new Exception("Das Objekt kann nicht gerepointet werden, die Funktionen von IRepointable stehen nicht zur Verfügung.");
            return this.currentOffset;
        }

        /// <summary>
        /// Legt das Offset des Tileset fest, dadurch werden die Funktionen von IRepointable verfügbar
        /// </summary>
        /// <param name="Offset">Das neue Offset des Tilesets</param>
        public void SetCurrentOffset(uint Offset)
        {
            this.isRepointable = true;
            this.currentOffset = Offset;
            this.lenght = this.GetRawData().Length;
        }

        #endregion
    }
}
