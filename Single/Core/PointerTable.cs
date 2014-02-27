using System;
using System.Collections.Generic;
using System.IO;

namespace Single.Core
{
    public class PointerTable : IRepointable
    {
        #region Fields

        private readonly List<UInt32> _entries;
        private readonly int _origSize;
        private UInt32 _offset;

        #endregion

        #region Constructors

        /// <summary>
        ///     Erstellt eine neue Pointer Tabelle aus dem Kontext eines Rom Objekts
        /// </summary>
        /// <param name="context">Rom Objekt, welches die Tabelle beinhaltet</param>
        /// <param name="offset">Offset der Tabelle</param>
        /// <param name="count">Anzahl der Pointer</param>
        public PointerTable(Rom context, UInt32 offset, int count)
        {
            _entries = new List<UInt32>();
            _offset = offset;
            context.SetStreamOffset(offset);
            for (int i = 0; i < count; ++i)
            {
                UInt32 pointer = context.ReadUInt32();
                if (((pointer & 0x1FFFFFF) >> 24) > 0)
                {
                    throw new Exception(String.Format("An dieser Stelle wurde kein Pointer gefunden: {0}",
                        pointer.ToString("X")));
                }
                _entries.Add(pointer);
                _origSize = (_entries.Count*4);
            }
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Gibt die Länge der Tabelle zurück
        /// </summary>
        /// <returns>Länge der Tabelle in Bytes</returns>
        public int GetSize()
        {
            return _entries.Count*4;
        }

        /// <summary>
        ///     Gibt das aktuelle Offset der Tabelle zurück
        /// </summary>
        /// <returns></returns>
        public uint GetCurrentOffset()
        {
            return _offset;
        }

        /// <summary>
        ///     Gibt die Bytes, welche die Tabelle ausmachen zurück, wird vom IRomWritable Interface verlangt
        /// </summary>
        /// <returns>Byte Array, repräsantiv für den Inhalt der Tabelle</returns>
        public byte[] GetRawData()
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            foreach (UInt32 entry in _entries)
            {
                bw.Write(entry);
            }
            byte[] output = ms.ToArray();
            ms.Close();
            return output;
        }

        public void SetCurrentOffset(UInt32 newOffset)
        {
            _offset = newOffset;
        }

        public int GetOriginalSize()
        {
            return _origSize;
        }

        /// <summary>
        ///     Fügt einen Pointer hinzu
        /// </summary>
        /// <param name="pointerOffset">Offset aus dem der Pointer berechnet werden soll</param>
        public void AddOffset(UInt32 pointerOffset)
        {
            _entries.Add(pointerOffset | 0x8000000);
        }

        /// <summary>
        ///     Entfernt den angegebenen Pointer
        /// </summary>
        /// <param name="index">Index des zu entfernenden Pointers</param>
        public void RemoveAt(int index)
        {
            _entries.RemoveAt(index);
        }

        /// <summary>
        ///     Gibt das Offset des Pointers zurück
        /// </summary>
        /// <param name="index">Index des Pointers</param>
        /// <returns>Adresse, die aus dem Pointer berechnet wird</returns>
        public UInt32 GetOffset(int index)
        {
            return _entries[index] & 0x1FFFFFF;
        }

        /// <summary>
        ///     Gibt den Pointer zurück
        /// </summary>
        /// <param name="index">Index des Pointers</param>
        /// <returns>Pointer an entsprechendem Index</returns>
        public UInt32 GetPointer(int index)
        {
            return _entries[index] & 0x8FFFFFF;
        }

        #endregion
    }
}