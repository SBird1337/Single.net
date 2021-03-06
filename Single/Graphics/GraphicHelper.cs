﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace Single.Graphics
{
    public static class GraphicHelper
    {
        #region Functions

        /// <summary>
        ///     Erstellt aus dem Bitmap img ein Tileset, eine Tilemap und eine Palette, die je nach Bedarf indiziert wird
        /// </summary>
        /// <param name="img">Eingabebitmap im PNG Format</param>
        /// <param name="pal">Ausgabe Palette</param>
        /// <param name="map">Ausgabe Tilemap</param>
        /// <param name="paletteMap">Gibt den Palettenindex an, welcher bei der Tilemaperstellung verwendet wird</param>
        /// <param name="is8Bpp">Wenn true: Erstellt ein 8bpp Tileset / Palette</param>
        /// <param name="isEncoded">Gibt an ob die Grafik komprimiert werden soll</param>
        /// <returns>Fertiges Tileset Objekt</returns>
        public static Tileset CreateIndexedTilesetMap(Bitmap img, out Palette pal, out Tilemap map, byte paletteMap = 0,
            bool is8Bpp = false, bool isEncoded = false)
        {
            if (img.PixelFormat == PixelFormat.Format4bppIndexed || img.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                return Tileset.FromBitmap(img, isEncoded, out pal, out map, paletteMap);
            }
            var colors = new List<Color>();
            var indexes = new List<byte>();
            var coordinates = new Dictionary<Point, byte>();
            Bitmap indexedBitmap = is8Bpp ? new Bitmap(img.Width, img.Height, PixelFormat.Format8bppIndexed) : new Bitmap(img.Width, img.Height, PixelFormat.Format4bppIndexed);

            for (int y = 0; y < img.Height; ++y)
            {
                for (int x = 0; x < img.Width; ++x)
                {
                    Color c = img.GetPixel(x, y);
                    if (!colors.Contains(c))
                    {
                        colors.Add(c);
                        coordinates.Add(new Point(x, y), (byte) (colors.Count - 1));
                        indexes.Add((byte) (colors.Count - 1));
                    }
                    else
                    {
                        coordinates.Add(new Point(x, y), (byte) colors.IndexOf(c));
                        indexes.Add((byte) colors.IndexOf(c));
                    }
                }
            }
            int ccount = 16;
            if (is8Bpp)
            {
                ccount = 256;
            }
            if (colors.Count > ccount)
            {
                throw new ArgumentException(
                    String.Format("Fehler beim indizieren des Bildes, es hat mehr als {0} Farben", ccount));
            }
            while (colors.Count < ccount)
            {
                colors.Add(Color.Magenta);
            }

            ColorPalette ipal = indexedBitmap.Palette;
            for (int i = 0; i < ipal.Entries.Count(); ++i)
            {
                ipal.Entries[i] = colors[i];
            }
            indexedBitmap.Palette = ipal;

            if (!is8Bpp)
            {
                BitmapData data = indexedBitmap.LockBits(
                    new Rectangle(new Point(0, 0), new Size(img.Width, img.Height)), ImageLockMode.ReadWrite,
                    PixelFormat.Format4bppIndexed);
                var bytes = new byte[data.Stride*data.Height];
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                for (int i = 0; i < bytes.Length; i++)
                {
                    var b2 = (byte) (indexes[i*2] << 4);
                    byte b1 = indexes[(i*2) + 1];
                    bytes[i] = (byte) (b1 | b2);
                }
                Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
                indexedBitmap.UnlockBits(data);
            }
            else
            {
                BitmapData data = indexedBitmap.LockBits(
                    new Rectangle(new Point(0, 0), new Size(img.Width, img.Height)), ImageLockMode.ReadWrite,
                    PixelFormat.Format8bppIndexed);
                var bytes = new byte[data.Stride*data.Height];
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                for (int i = 0; i < bytes.Length; ++i)
                {
                    bytes[i] = indexes[i];
                }
                Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
                indexedBitmap.UnlockBits(data);
            }
            pal = new Palette(colors.Select(col => new PaletteEntry(col.R, col.G, col.B)).ToArray());
            Palette p;
            return Tileset.FromBitmap(indexedBitmap, isEncoded, out p, out map);
        }

        /// <summary>
        ///     Setzt den Pixel an [x,y] auf den entsprechenden Index.
        /// </summary>
        /// <param name="bmp">Bild, an dem der Pixel zu setzen ist</param>
        /// <param name="x">X-Koordinate</param>
        /// <param name="y">>-Koordinate</param>
        /// <param name="paletteEntry">Index in der Palette</param>
        [Obsolete("Sehr langsame Methode, verwenden sie LockBits um alle ihre Pixel auf einmal zu setzen")]
        public static void Set4BppPixel(Bitmap bmp, int x, int y, int paletteEntry)
        {
            BitmapData data = bmp.LockBits(new Rectangle(new Point(x, y), new Size(1, 1)), ImageLockMode.ReadWrite,
                PixelFormat.Format4bppIndexed);
            byte b = Marshal.ReadByte(data.Scan0);
            Marshal.WriteByte(data.Scan0, (byte) (b & 0xf | (paletteEntry << 4)));
            bmp.UnlockBits(data);
        }

        /// <summary>
        ///     Setzt den Pixel an [x,y] auf den entsprechenden Index.
        /// </summary>
        /// <param name="bmp">Bild, an dem der Pixel zu setzen ist</param>
        /// <param name="x">X-Koordinate</param>
        /// <param name="y">>-Koordinate</param>
        /// <param name="paletteEntry">Index in der Palette</param>
        [Obsolete("Sehr langsame Methode, verwenden sie LockBits um alle ihre Pixel auf einmal zu setzen")]
        public static void Set8BppPixel(Bitmap bmp, int x, int y, int paletteEntry)
        {
            BitmapData data = bmp.LockBits(new Rectangle(new Point(x, y), new Size(1, 1)), ImageLockMode.ReadWrite,
                PixelFormat.Format8bppIndexed);
            Marshal.ReadByte(data.Scan0);
            Marshal.WriteByte(data.Scan0, (byte) paletteEntry);
            bmp.UnlockBits(data);
        }

        #endregion
    }
}