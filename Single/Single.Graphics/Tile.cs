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

        private readonly byte[] data;
        private readonly bool is8bpp;

        #endregion

        #region Constructors

        /// <summary>
        ///     Erstellt ein Tile aus den angegebenen Daten
        /// </summary>
        /// <param name="datalist">
        ///     Liste mit Tileset Einträgen, muss 32 Einträge lang sein wenn es sich um ein 4bpp Tile handelt,
        ///     ansonsten 64 Einträge
        /// </param>
        /// <param name="is8bpp">Wenn true: Es wird versucht ein 8bpp Tile zu laden</param>
        public Tile(List<Byte> datalist, bool is8bpp = false)
        {
            this.is8bpp = is8bpp;
            if (!is8bpp)
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
            data = datalist.ToArray();
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Implementation des IRomWritable Interfaces, gibt die Daten des Tiles zurück wie sie im Rom stehen würden
        /// </summary>
        /// <returns></returns>
        public byte[] GetRawData()
        {
            return data;
        }

        /// <summary>
        ///     Erstellt ein Tile mit Daten aus dem angegebenen Rom Objekt an der angegebenen Stelle
        /// </summary>
        /// <param name="Rom">Rom Objekt, welches die Tile Daten enthält</param>
        /// <param name="Offset">Position der Daten</param>
        /// <param name="is8bpp">Wenn true: Es wird versucht ein 8bpp Tile zu laden</param>
        /// <returns></returns>
        public static Tile FromRomOffset(Rom Rom, UInt32 Offset, bool is8bpp = false)
        {
            Rom.SetStreamOffset(Offset);
            if (!is8bpp)
            {
                return new Tile(Rom.ReadByteArray(32));
            }
            return new Tile(Rom.ReadByteArray(64), is8bpp);
        }

        /// <summary>
        ///     Erstellt ein Bitmap Objekt aus den Tile Daten
        /// </summary>
        /// <param name="pal">Palette, die zur Darstellung verwendet werden soll</param>
        /// <returns>Bitmap Objekt, welches das Tile darstellt</returns>
        public Bitmap ToBitmap(Palette pal)
        {
            var colorentries = new List<Byte>(64);
            if (is8bpp && (!pal.Is256Color()))
            {
                throw new Exception("Die angegebene Palette ist nicht im 8bpp Modus.");
            }
            foreach (byte b in data)
            {
                if (!is8bpp)
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
                output.SetPixel(x, y, pal.Entries[colorentries[i]]);
            }
            return output;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof (Tile))
            {
                var comp = (Tile) obj;
                return data.SequenceEqual(comp.GetRawData());
            }
            return false;
        }

        public override int GetHashCode()
        {
            return data.GetHashCode();
        }

        #endregion
    }
}