using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Single.Compression;
using Single.Core;

namespace Single.Graphics
{
    public class Tilemap : IRepointable
    {
        #region Fields

        private readonly int _origSize;
        private UInt32 _currentOffset;
        private bool _isRepointable;
        private int _lenght;

        #endregion

        #region Properties

        public List<TilemapEntry> Entries { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Erstellt eine leere Tilemap
        /// </summary>
        public Tilemap()
        {
            Entries = new List<TilemapEntry>();
            _origSize = GetSize();
        }

        /// <summary>
        ///     Erstellt eine Tilemap mit Daten vom angegebenen Rom Objekt
        /// </summary>
        /// <param name="input">Rom Objekt in welchem die Tilemap Daten liegen</param>
        /// <param name="offset">Position der Daten</param>
        /// <param name="entrycount">Länge der Tilemap in Tiles</param>
        public Tilemap(Rom input, UInt32 offset, uint entrycount)
        {
            _isRepointable = true;
            Entries = new List<TilemapEntry>();
            input.SetStreamOffset(offset);
            for (int i = 0; i < entrycount; ++i)
            {
                Entries.Add(new TilemapEntry(input.ReadUInt16()));
            }
            _origSize = GetSize();
        }

        /// <summary>
        ///     Erstellt eine Tilemap mit Daten vom angegebenen Rom Objekt und dekomprimiert die Daten vorher
        /// </summary>
        /// <param name="input">Rom Objekt in welchem die komprimierten Tilemap Daten liegen</param>
        /// <param name="offset">Position der Daten</param>
        public Tilemap(Rom input, uint offset)
        {
            long origSize;
            byte[] data = RomDecode.LzUncompress(input, offset, out origSize);
            var ms = new MemoryStream(data);
            var br = new BinaryReader(ms);
            var tmap = new List<UInt16>();
            if (data.Length%2 != 0)
            {
                throw new Exception("An der angegebenen Stelle befindet sich keine komprimierte Tilemap");
            }

            for (int i = 0; i < data.Length/2; ++i)
            {
                tmap.Add(br.ReadUInt16());
            }
            Initialize(tmap.ToArray());
            _origSize = (int)origSize;
        }

        /// <summary>
        ///     Erstellt eine Tilemap aus den angegebenen Einträgen
        /// </summary>
        /// <param name="shortEntries">Array mit unsignierten Tilemap Daten</param>
        public Tilemap(IEnumerable<ushort> shortEntries)
        {
            Initialize(shortEntries);
            _origSize = GetSize();
        }

        #endregion

        #region Methods

        private void Initialize(IEnumerable<ushort> shortEntries)
        {
            Entries = new List<TilemapEntry>();
            foreach (UInt16 entry in shortEntries)
            {
                Entries.Add(new TilemapEntry(entry));
            }
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Implementation des IRomWritable Interfaces, gibt die Tilemap Daten zurück wie sie als Array im Rom stehen würden
        /// </summary>
        /// <returns>Byte Array mit den Tilemap Daten</returns>
        public byte[] GetRawData()
        {
            var output = new List<byte>();
            foreach (TilemapEntry entry in Entries)
            {
                output.AddRange(entry.GetRawData());
            }
            return output.ToArray();
        }

        /// <summary>
        ///     Gibt die Länge der Tilemap zurück, nur möglich wenn das Objekt im Repointable ist
        /// </summary>
        /// <returns>Länge der Tilemap in Bytes</returns>
        public int GetSize()
        {
            return _lenght;
        }

        /// <summary>
        ///     Gibt das Offset der Tilemap zurück, nur möglich wenn das Objekt Repointable ist
        /// </summary>
        /// <returns>Offset der Tilemap in Kontext auf ein Rom</returns>
        public uint GetCurrentOffset()
        {
            if (!_isRepointable)
                throw new Exception(
                    "Das Objekt kann nicht gerepointet werden, die Funktionen von IRepointable stehen nicht zur Verfügung.");
            return _currentOffset;
        }

        /// <summary>
        ///     Legt das Offset der Tilemap fest, dadurch werden die Funktionen von IRepointable verfügbar
        /// </summary>
        /// <param name="newOffset">Das neue Offset der Tilemap</param>
        public void SetCurrentOffset(uint newOffset)
        {
            _isRepointable = true;
            _currentOffset = newOffset;
            _lenght = GetRawData().Length;
        }

        /// <summary>
        ///     Erstellt ein Bitmap Objekt unter Verwendung von Tileset, Tilemap und Palette
        /// </summary>
        /// <param name="set">Tileset, welches die Grafikdaten enthält</param>
        /// <param name="tileWidth">Breite der Ausgabegrafik in Tiles</param>
        /// <param name="fillColor">Farbe mit der nicht vorhandene Flächen gefüllt werden</param>
        /// <param name="drawingPalette">Palette, die zur Darstellung verwendet werden soll</param>
        /// <returns></returns>
        public Bitmap ToBitmap(Tileset set, int tileWidth, Color fillColor, Palette drawingPalette)
        {
            int tileHeight = Entries.Count/tileWidth;
            var output = new Bitmap(tileWidth*8, tileHeight*8);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(output);
            for (int y = 0; y < tileHeight; ++y)
            {
                for (int x = 0; x < tileWidth; ++x)
                {
                    int index = (y*tileWidth) + x;
                    if (index > Entries.Count)
                    {
                        g.FillRectangle(new SolidBrush(fillColor), x*8, y*8, 8, 8);
                        continue;
                    }
                    g.DrawImage(set.GetTileFromIndex(Entries[index].TileNumber).ToBitmap(drawingPalette),
                        new Point(x*8, y*8));
                }
            }
            g.Dispose();
            return output;
        }

        #endregion

        public int GetOriginalSize()
        {
            return _origSize;
        }
    }
}