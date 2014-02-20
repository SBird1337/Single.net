using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Single.Compression;
using Single.Core;

namespace Single.Graphics
{
    public class Palette : IRepointable
    {
        #region Fields

        private readonly Color[] _entries;
        private readonly int origSize;
        private UInt32 currentOffset;
        private bool isRepointable;
        private int lenght;

        #endregion

        #region Properties

        public Color[] Entries
        {
            get { return _entries; }
        }

        public bool IsEncoded { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Erstellt eine Palette aus den angegebenen Farbwerten
        /// </summary>
        /// <param name="entries">Array aus System.Color Werten, muss 16 oder 256 Einträge beinhalten</param>
        public Palette(Color[] entries, bool isEncoded = false)
        {
            IsEncoded = isEncoded;
            if (entries.Count() == 16 || entries.Count() == 256)
            {
                _entries = entries;
            }
            else
            {
                throw new Exception("Das angegebene Farbarray muss entweder 16 oder 256 Farben haben.");
            }
            origSize = GetSize();
        }

        /// <summary>
        ///     Erstellt eine Palette aus den Werten an der angegebenen Romadresse
        /// </summary>
        /// <param name="input">Rom Objekt, welches die Palettendaten enthält</param>
        /// <param name="offset">Offset der Daten</param>
        /// <param name="isEncoded">Wenn true: Die Daten werden zuerst dekomprimiert(LZ77)</param>
        /// <param name="is256Pal">Wenn true: Lädt 256 Einträge anstatt 16</param>
        public Palette(Rom input, UInt32 offset, bool isEncoded, bool is256Pal = false)
        {
            IsEncoded = isEncoded;
            isRepointable = true;
            currentOffset = offset;
            short cols = 16;
            if (is256Pal)
            {
                cols = 256;
            }
            _entries = new Color[cols];
            var EntryList = new List<UInt16>();
            if (!isEncoded)
            {
                {
                    lenght = EntryList.Count()*2;
                    input.SetStreamOffset(offset);
                    EntryList = input.ReadUShortArray(cols);
                }
            }
            else
            {
                var unlz = new List<Byte>();
                unlz = RomDecode.UnlzFromOffset(input, offset, out lenght);
                for (int i = 0; i < unlz.Count/2; i++)
                {
                    UInt16 temp = unlz[(2*i)];
                    temp |= (UInt16) ((unlz[1 + (i*2)]) << 8);
                    EntryList.Add(temp);
                }
            }
            for (int i = 0; i < cols; i++)
            {
                var red = (UInt16) ((EntryList[i] & 31)*8);
                var green = (UInt16) (((EntryList[i] & 992) >> 5)*8);
                var blue = (UInt16) (((EntryList[i] & 31744) >> 10)*8);
                _entries[i] = Color.FromArgb(red, green, blue);
            }
            if (_entries.Length != cols)
            {
                throw new Exception("Die angegebene Palette hat nicht die angegebene Anzahl an Farben");
            }
            origSize = GetSize();
        }

        public int GetOriginalSize()
        {
            return origSize;
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Implementation des IRomWritable Interfaces
        /// </summary>
        /// <returns>Byte Array mit Daten, die so im Rom stehen würden</returns>
        public byte[] GetRawData()
        {
            var output = new List<byte>();
            foreach (Color c in _entries)
            {
                var r = (byte) (Math.Round((double) (c.R/8)));
                var g = (byte) (Math.Round((double) (c.G/8)));
                var b = (byte) (Math.Round((double) (c.B/8)));
                var ColorEntry = (UInt16) (r | (g << 5) | (b << 10));
                output.Add((byte) (ColorEntry & 0xFF));
                output.Add((byte) ((ColorEntry & 0xFF00) >> 8));
            }
            if (!IsEncoded)
            {
                return output.ToArray();
            }
            return RomDecode.LzCompressData(output.ToArray());
        }

        /// <summary>
        ///     Gibt die Länge der Palette zurück, nur möglich wenn das Objekt im Repointable ist
        /// </summary>
        /// <returns>Länge der Palette in Bytes</returns>
        public int GetSize()
        {
            return lenght;
        }

        /// <summary>
        ///     Gibt das Offset der Palette zurück, nur möglich wenn das Objekt Repointable ist
        /// </summary>
        /// <returns>Offset der Palette in Kontext auf ein Rom</returns>
        public uint GetCurrentOffset()
        {
            if (!isRepointable)
                throw new Exception(
                    "Das Objekt kann nicht gerepointet werden, die Funktionen von IRepointable stehen nicht zur Verfügung.");
            return currentOffset;
        }

        /// <summary>
        ///     Legt das Offset der Palette fest, dadurch werden die Funktionen von IRepointable verfügbar
        /// </summary>
        /// <param name="offset">Das neue Offset der Palette</param>
        public void SetCurrentOffset(uint offset)
        {
            isRepointable = true;
            currentOffset = offset;
            lenght = GetRawData().Length;
        }

        /// <summary>
        ///     Gibt an ob die Palette 256 Einträge hat oder nicht
        /// </summary>
        /// <returns>True, wenn die Anzahl der Einträge = 256, false wenn die Anzahl der Einträge = 16</returns>
        public bool Is256Color()
        {
            return _entries.Count() == 256;
        }

        #endregion
    }
}