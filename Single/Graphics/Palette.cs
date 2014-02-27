﻿using System;
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
        private readonly int _origSize;
        private UInt32 _currentOffset;
        private bool _isRepointable;
        private int _lenght;

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
        /// <param name="isEncoded">Gibt an ob die Palette komprimiert vorhanden ist</param>
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
            _origSize = GetSize();
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
            _isRepointable = true;
            _currentOffset = offset;
            short cols = 16;
            if (is256Pal)
            {
                cols = 256;
            }
            _entries = new Color[cols];
            var entryList = new List<UInt16>();
            if (!isEncoded)
            {
                {
                    _lenght = entryList.Count()*2;
                    input.SetStreamOffset(offset);
                    entryList = input.ReadUShortArray(cols);
                }
            }
            else
            {
                List<byte> unlz = RomDecode.UnlzFromOffset(input, offset, out _lenght);
                for (int i = 0; i < unlz.Count/2; i++)
                {
                    UInt16 temp = unlz[(2*i)];
                    temp |= (UInt16) ((unlz[1 + (i*2)]) << 8);
                    entryList.Add(temp);
                }
            }
            for (int i = 0; i < cols; i++)
            {
                var red = (UInt16) ((entryList[i] & 31)*8);
                var green = (UInt16) (((entryList[i] & 992) >> 5)*8);
                var blue = (UInt16) (((entryList[i] & 31744) >> 10)*8);
                _entries[i] = Color.FromArgb(red, green, blue);
            }
            if (_entries.Length != cols)
            {
                throw new Exception("Die angegebene Palette hat nicht die angegebene Anzahl an Farben");
            }
            _origSize = GetSize();
        }

        public int GetOriginalSize()
        {
            return _origSize;
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
                var r = (byte) (Math.Round((c.R/8.0)));
                var g = (byte) (Math.Round((c.G/8.0)));
                var b = (byte) (Math.Round((c.B/8.0)));
                var colorEntry = (UInt16) (r | (g << 5) | (b << 10));
                output.Add((byte) (colorEntry & 0xFF));
                output.Add((byte) ((colorEntry & 0xFF00) >> 8));
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
            return _lenght;
        }

        /// <summary>
        ///     Gibt das Offset der Palette zurück, nur möglich wenn das Objekt Repointable ist
        /// </summary>
        /// <returns>Offset der Palette in Kontext auf ein Rom</returns>
        public uint GetCurrentOffset()
        {
            if (!_isRepointable)
                throw new Exception(
                    "Das Objekt kann nicht gerepointet werden, die Funktionen von IRepointable stehen nicht zur Verfügung.");
            return _currentOffset;
        }

        /// <summary>
        ///     Legt das Offset der Palette fest, dadurch werden die Funktionen von IRepointable verfügbar
        /// </summary>
        /// <param name="newOffset">Das neue Offset der Palette</param>
        public void SetCurrentOffset(uint newOffset)
        {
            _isRepointable = true;
            _currentOffset = newOffset;
            _lenght = GetRawData().Length;
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