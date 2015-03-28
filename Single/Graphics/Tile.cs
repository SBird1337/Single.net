using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Single.Core;

namespace Single.Graphics
{
    public class Tile : IRomWritable
    {
        #region Fields

        private readonly byte[] _data;
        private readonly bool _is8Bpp;

        #endregion

        #region Constructors

        /// <summary>
        ///     Erstellt ein Tile aus den angegebenen Daten
        /// </summary>
        /// <param name="datalist">
        ///     Liste mit Tileset Einträgen, muss 32 Einträge lang sein wenn es sich um ein 4bpp Tile handelt,
        ///     ansonsten 64 Einträge
        /// </param>
        /// <param name="is8Bpp">Wenn true: Es wird versucht ein 8bpp Tile zu laden</param>
        public Tile(List<Byte> datalist, bool is8Bpp = false)
        {
            _is8Bpp = is8Bpp;
            if (!is8Bpp)
            {
                if (datalist.Count != 32)
                {
                    throw new Exception(
                        "Die Liste der zur Verfügung gestellten Daten enthält mehr oder weniger als 32 Einträge uns ist ungültig für ein 4bpp Tile.");
                }
            }
            else
            {
                if (datalist.Count != 64)
                {
                    throw new Exception(
                        "Die Liste der zur Verfügung gestellten Daten enthält mehr oder weniger als 64 Einträge und ist ungültig für ein 8bpp Tile.");
                }
            }
            _data = datalist.ToArray();
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Implementation des IRomWritable Interfaces, gibt die Daten des Tiles zurück wie sie im Rom stehen würden
        /// </summary>
        /// <returns></returns>
        public byte[] GetRawData()
        {
            return _data;
        }

        /// <summary>
        ///     Erstellt ein Tile mit Daten aus dem angegebenen Rom Objekt an der angegebenen Stelle
        /// </summary>
        /// <param name="rom">Rom Objekt, welches die Tile Daten enthält</param>
        /// <param name="offset">Position der Daten</param>
        /// <param name="is8Bpp">Wenn true: Es wird versucht ein 8bpp Tile zu laden</param>
        /// <returns></returns>
        public static Tile FromRomOffset(Rom rom, UInt32 offset, bool is8Bpp = false)
        {
            rom.SetStreamOffset(offset);
            if (!is8Bpp)
            {
                return new Tile(rom.ReadByteArray(32));
            }
            return new Tile(rom.ReadByteArray(64), true);
        }

        /// <summary>
        ///     Erstellt ein Bitmap Objekt aus den Tile Daten
        /// </summary>
        /// <param name="pal">Palette, die zur Darstellung verwendet werden soll</param>
        /// <returns>Bitmap Objekt, welches das Tile darstellt</returns>
        public Bitmap ToBitmap(Palette pal)
        {
            var colorentries = new List<Byte>(64);
            if (_is8Bpp && (!pal.Is256Color()))
            {
                throw new Exception("Die angegebene Palette ist nicht im 8bpp Modus.");
            }
            foreach (byte b in _data)
            {
                if (!_is8Bpp)
                {
                    var first = (byte) (b & 15);
                    var second = (byte) (b >> 4);
                    colorentries.Add(first);
                    colorentries.Add(second);
                }
                else
                {
                    colorentries.Add(b);
                }
            }
            var output = new Bitmap(8, 8);
            for (int i = 0; i < colorentries.Count; i++)
            {
                int y = i/8;
                int x = i%8;
                output.SetPixel(x, y, pal.Entries[colorentries[i]].ToColor());
            }
            return output;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof (Tile))
            {
                var comp = (Tile) obj;
                return _data.SequenceEqual(comp.GetRawData());
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _data.GetHashCode();
        }

        #endregion
    }
}