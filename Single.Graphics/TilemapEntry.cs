using Single.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Single.Graphics
{
    public class TilemapEntry : IRomWritable
    {
        #region Fields

        private UInt16 _tileNumber;
        private byte _paletteIndex;

        #endregion

        #region Properties

        public UInt16 TileNumber
        {
            get
            {
                return _tileNumber;
            }
            set
            {
                if (value > 1023)
                {
                    throw new ArgumentException("Die Tile Nummer überschreitet die Maximalgröße von 9 Bit");
                    
                }
                _tileNumber = value;
            }
        }

        public bool VerticalMirror
        { get; set; }

        public bool HorizontalMirror
        { get; set; }

        public byte PaletteIndex
        {
            get
            {
                return _paletteIndex;
            }
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
        /// Erstellt einen einzelnen Tilemap Eintrag aus dem angegebenen unsignierten Half-Word
        /// </summary>
        /// <param name="input">Unsigniertes Half-Word mit Tilemap Daten</param>
        public TilemapEntry(UInt16 input)
        {
            this.TileNumber = Convert.ToUInt16(input & 1023);
            this.HorizontalMirror = Convert.ToBoolean(input & 1024);
            this.VerticalMirror = Convert.ToBoolean(input & 2048);
            this.PaletteIndex = Convert.ToByte((input & 61440) >> 12);
        }

        /// <summary>
        /// Erstellt einen einzelnen Tilemap Eintrag mit den angegebenen Daten
        /// </summary>
        /// <param name="tileNumber">Die Nummer des Tiles im Tileset</param>
        /// <param name="paletteNumber">Die zu verwendende Palette(0-16)</param>
        /// <param name="horizontalFlip">Wenn True: Horizontale Spiegelung</param>
        /// <param name="verticalFlip">Wenn True: Vertikale Spiegelung</param>
        public TilemapEntry(UInt16 tileNumber, byte paletteNumber, bool horizontalFlip = false, bool verticalFlip = false)
        {
            this.TileNumber = tileNumber;
            this.VerticalMirror = verticalFlip;
            this.HorizontalMirror = verticalFlip;
            this.PaletteIndex = paletteNumber;
        }

        #endregion

        #region Functions

        /// <summary>
        /// Erstellt ein Bitmap aus dem einzelnen Tilemap Eintrag
        /// </summary>
        /// <param name="drawingSet">Das zu verwendende Tileset</param>
        /// <param name="drawingPalette">Die zur Darstellung verwendete Palette</param>
        /// <returns>Bitmap, welches den Eintrag darstellt</returns>
        public Bitmap ToBitmap(Tileset drawingSet, Palette drawingPalette)
        {
            if (drawingSet.GetTileCount() < this.TileNumber)
                throw new Exception(String.Format("Das angegebene Tileset enthält Tile {0} nicht", this.TileNumber));
            Bitmap output = drawingSet.GetTileFromIndex(this.TileNumber).ToBitmap(drawingPalette);
            if (this.HorizontalMirror)
            {
                output.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
            if (this.VerticalMirror)
            {
                output.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
            return output;
        }

        /// <summary>
        /// Implementation des IRomWritable Interfaces, gibt den Eintrag zurück wie er als Byte Array im Rom stehen würde
        /// </summary>
        /// <returns>Byte Array mit dem Inhalt des Tilemap Eintrags</returns>
        public byte[] GetRawData()
        {
            byte parse = (byte)(((byte)(this.PaletteIndex << 4)) | (byte)((Convert.ToByte(this.VerticalMirror) << 3)) | (byte)((Convert.ToByte(this.HorizontalMirror) << 2)));
            return new byte[] 
            {
                (byte)(this.TileNumber & 0xFF), 
                (byte)((byte)((this.TileNumber >> 8) & 1) | parse)
            };
        }

        #endregion
    }
}
