using System;
using System.Drawing;
using Single.Core;

namespace Single.Graphics
{
    public class TilemapEntry : IRomWritable
    {
        #region Fields

        private byte _paletteIndex;
        private UInt16 _tileNumber;

        #endregion

        #region Properties

        public UInt16 TileNumber
        {
            get { return _tileNumber; }
            set
            {
                if (value > 1023)
                {
                    throw new ArgumentException("Die Tile Nummer überschreitet die Maximalgröße von 9 Bit");
                }
                _tileNumber = value;
            }
        }

        public bool VerticalMirror { get; set; }

        public bool HorizontalMirror { get; set; }

        public byte PaletteIndex
        {
            get { return _paletteIndex; }
            set
            {
                if (value > 15)
                {
                    throw new ArgumentException("Der Wert der Palette darf nicht größer als 4 Bit sein.");
                }
                _paletteIndex = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Erstellt einen einzelnen Tilemap Eintrag aus dem angegebenen unsignierten Half-Word
        /// </summary>
        /// <param name="input">Unsigniertes Half-Word mit Tilemap Daten</param>
        public TilemapEntry(UInt16 input)
        {
            TileNumber = Convert.ToUInt16(input & 1023);
            HorizontalMirror = Convert.ToBoolean(input & 1024);
            VerticalMirror = Convert.ToBoolean(input & 2048);
            PaletteIndex = Convert.ToByte((input & 61440) >> 12);
        }

        /// <summary>
        ///     Erstellt einen einzelnen Tilemap Eintrag mit den angegebenen Daten
        /// </summary>
        /// <param name="tileNumber">Die Nummer des Tiles im Tileset</param>
        /// <param name="paletteNumber">Die zu verwendende Palette(0-16)</param>
        /// <param name="horizontalFlip">Wenn True: Horizontale Spiegelung</param>
        /// <param name="verticalFlip">Wenn True: Vertikale Spiegelung</param>
        public TilemapEntry(UInt16 tileNumber, byte paletteNumber, bool horizontalFlip = false,
            bool verticalFlip = false)
        {
            TileNumber = tileNumber;
            VerticalMirror = verticalFlip;
            HorizontalMirror = horizontalFlip;
            PaletteIndex = paletteNumber;
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Implementation des IRomWritable Interfaces, gibt den Eintrag zurück wie er als Byte Array im Rom stehen würde
        /// </summary>
        /// <returns>Byte Array mit dem Inhalt des Tilemap Eintrags</returns>
        public byte[] GetRawData()
        {
            var parse =
                (byte)
                    (((byte) (PaletteIndex << 4)) | (byte) ((Convert.ToByte(VerticalMirror) << 3)) |
                     (byte) ((Convert.ToByte(HorizontalMirror) << 2)));
            return new[]
            {
                (byte) (TileNumber & 0xFF),
                (byte) ((byte) ((TileNumber >> 8) & 1) | parse)
            };
        }

        /// <summary>
        ///     Erstellt ein Bitmap aus dem einzelnen Tilemap Eintrag
        /// </summary>
        /// <param name="drawingSet">Das zu verwendende Tileset</param>
        /// <param name="drawingPalette">Die zur Darstellung verwendete Palette</param>
        /// <returns>Bitmap, welches den Eintrag darstellt</returns>
        public Bitmap ToBitmap(Tileset drawingSet, Palette drawingPalette)
        {
            if (drawingSet.GetTileCount() < TileNumber)
                throw new Exception(String.Format("Das angegebene Tileset enthält Tile {0} nicht", TileNumber));
            Bitmap output = drawingSet.GetTileFromIndex(TileNumber).ToBitmap(drawingPalette);
            if (HorizontalMirror)
            {
                output.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
            if (VerticalMirror)
            {
                output.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
            return output;
        }

        #endregion
    }
}