using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Single.Core
{
    public class Rom : IDisposable
    {
        #region Constants

        public const UInt32 MB_16_ROM = 0x1000000;
        public const UInt32 MB_32_ROM = 0x2000000;

        #endregion

        #region Structures

        private struct PatchEntry
        {
            public byte[] Data;
            public UInt32 Offset;
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Erstellt ein Rom Objekt mit angegebener Länge aus dem angegebenen Stream
        /// </summary>
        /// <param name="input">Der Eingangsstream mit festgelegter Leseposition</param>
        /// <param name="romlenght">Die Länge des Rom Objekts, als Hilfe dienen die statischen Konstanten in der Rom Klasse</param>
        public Rom(Stream input, UInt32 romlenght)
        {
            if (!Load(input, romlenght))
                throw new ArgumentException();
        }

        /// <summary>
        ///     Erstellt einen Filestream der angegebenen Datei und bildet aus der Datei und ihrer Länge ein Rom Objekt
        /// </summary>
        /// <param name="path">Der Pfad zur Rom Datei</param>
        public Rom(String path)
        {
            Stream instream = new FileStream(path, FileMode.Open);
            if (!Load(instream, (UInt32) instream.Length))
            {
                instream.Close();
                throw new ArgumentException();
            }
        }

        #endregion

        #region Destructor

        ~Rom()
        {
            Dispose(false);
        }

        #endregion

        #region Fields

        private bool _disposed;
        private List<PatchEntry> _patchentries = new List<PatchEntry>();
        private byte[] _rawdata;
        private BinaryReader _romReader;
        private BinaryWriter _romWriter;
        private MemoryStream _romstream;

        #endregion

        #region Properties

        public Romheader Header { get; set; }

        public byte[] RawData
        {
            get { return _rawdata; }
        }

        public long CurrentPosition
        {
            get { return _romstream.Position; }
        }

        #endregion

        #region Functions

        private Boolean Load(Stream instream, UInt32 romlenght)
        {
            if (instream != null && instream.Position + romlenght <= instream.Length)
            {
                var br = new BinaryReader(instream);
                _rawdata = br.ReadBytes((int) romlenght);
                instream.Close();
                _romstream = new MemoryStream(_rawdata);
                _romReader = new BinaryReader(_romstream);
                _romWriter = new BinaryWriter(_romstream);
                SetStreamOffset(0);
                Header = new Romheader(ReadByteArray(192).ToArray());
                SetStreamOffset(0x0);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Speichert das komplette Byte Array des Roms in die angegebene Datei
        /// </summary>
        /// <param name="path">Pfad zur Datei in welche die Daten geschrieben werden sollen, benötigt Schreibzugriff</param>
        /// <returns>Erfolg des Vorgangs</returns>
        public Boolean Save(string path)
        {
            var fs = new FileStream(path, FileMode.OpenOrCreate);
            var bw = new BinaryWriter(fs);
            bw.Write(_rawdata);
            fs.Close();
            return true;
        }

        /// <summary>
        ///     Patcht die angegebene Rom Datei mit allen im Rom Objekt vorgenommenen Änderungen, überschreibt dabei keine anderen
        ///     Daten
        /// </summary>
        /// <param name="path">Pfad zur Datei in welche die Daten geschrieben werden sollen, benötigt Schreibzugriff</param>
        /// <returns>Erfolg des Vorgangs</returns>
        public Boolean Patch(string path)
        {
            var fs = new FileStream(path, FileMode.Open);
            //var br = new BinaryReader(fs);
            //byte[] copyData = br.ReadBytes((int) fs.Length);
            if (fs.Length != _rawdata.Length)
            {
                fs.Close();
                throw new ArgumentException();
            }
            //var ms = new MemoryStream(copyData);
            //var bs = new BufferedStream(ms);
            var bw = new BinaryWriter(fs);
            try
            {
                foreach (PatchEntry entry in _patchentries)
                {
                    bw.BaseStream.Position = entry.Offset;
                    bw.Write(entry.Data);
                }
                fs.Close();
            }
            catch
            {
                fs.Close();
                throw;
            }
            _patchentries.Clear();
            return true;
        }

        /// <summary>
        ///     Ließt eine ASCII Zeichenkette von Lenght an der aktuellen Position und erhöht die Position um Length
        /// </summary>
        /// <param name="length">Länge der Zeichenkette</param>
        /// <returns>ASCII Zeichenkette</returns>
        public String ReadString(int length)
        {
            string output = "";
            for (int i = 0; i < length; i++)
            {
                output += _romReader.ReadChar();
            }
            return output;
        }

        /// <summary>
        ///     Ließt ein Word ohne Vorzeichen an der aktuellen Position und erhöht die Position um 4
        /// </summary>
        /// <returns>Eingelesenes Word</returns>
        public UInt32 ReadUInt32()
        {
            return _romReader.ReadUInt32();
        }

        /// <summary>
        ///     Ließt ein Half-Word ohne Vorzeichen an der aktuellen Position und erhöht die Position um 2
        /// </summary>
        /// <returns>Eingelesenes Half-Word</returns>
        public UInt16 ReadUInt16()
        {
            return _romReader.ReadUInt16();
        }

        /// <summary>
        ///     Ließt ein Byte ohne Vorzeichen an der aktuellen Position und erhöht die Position um 1
        /// </summary>
        /// <returns>Eingelesenes Byte</returns>
        public Byte ReadByte()
        {
            return _romReader.ReadByte();
        }

        /// <summary>
        ///     Ließt ein Array aus Bytes mit Länge Count an der aktuellen Position und erhöht die Position um Count
        /// </summary>
        /// <param name="count">Anzahl der Bytes im Array</param>
        /// <returns>Liste mit eingelesenen Werten</returns>
        public List<Byte> ReadByteArray(int count)
        {
            var output = new List<Byte>();
            output.AddRange(_romReader.ReadBytes(count));
            return output;
        }

        /// <summary>
        ///     Ließt ein Array aus Half-Words ohne Vorzeichen mit Länge Count an der aktuellen Position und erhöht die Position um
        ///     Count.
        /// </summary>
        /// <param name="count">Anzahl der Half-Words</param>
        /// <returns>Liste mit den eingelesenen UInt16 Werten</returns>
        public List<UInt16> ReadUShortArray(int count)
        {
            var output = new List<UInt16>();
            for (int i = 0; i < count; i++)
            {
                output.Add(_romReader.ReadUInt16());
            }
            return output;
        }

        /// <summary>
        ///     Schreibt ein Byte an die aktuelle Position und erhöht die aktuelle Position um 1
        /// </summary>
        /// <param name="b">Zu schreibendes Byte</param>
        /// <returns>Erfolg des Vorgangs</returns>
        public bool WriteByte(Byte b)
        {
            return WriteByteArray(new[]{b});
        }

        /// <summary>
        ///     Schreibt ein Half-Word an die aktuelle Position und erhöht die aktuelle Position um 2
        /// </summary>
        /// <param name="u16">Zu schreibendes Half-Word</param>
        /// <returns>Erfolg des Vorgangs</returns>
        public bool WriteUInt16(UInt16 u16)
        {
            return WriteByteArray(BitConverter.GetBytes(u16));
        }

        /// <summary>
        ///     Schreibt ein Word an die aktuelle Position und erhöht die aktuelle Position um 4
        /// </summary>
        /// <param name="u32">Zu schreibendes Word</param>
        /// <returns>Erfolg des Vorgangs</returns>
        public bool WriteUInt32(UInt32 u32)
        {
            return WriteByteArray(BitConverter.GetBytes(u32));
        }

        /// <summary>
        ///     Schreibt das angegebene Byte Array an die aktuelle Position und erhöht die aktuelle Position um die Länge des
        ///     Arrays
        /// </summary>
        /// <param name="bytes">Das zu schreibende Byte Array</param>
        /// <returns>Erfolg des Vorgangs</returns>
        public bool WriteByteArray(byte[] bytes)
        {
            PatchEntry entry;
            entry.Offset = (UInt32) _romstream.Position;
            entry.Data = bytes;
            _patchentries.Add(entry);
            _romWriter.Write(bytes);
            return true;
        }

        /// <summary>
        ///     Schreibt das angegebene Half-Word Array an die aktuelle Position und erhöht die aktuelle Position um die Länge des
        ///     Arrays
        /// </summary>
        /// <param name="shorts">Das zu schreibende Half-Word Array</param>
        /// <returns>Erfolg des Vorgangs</returns>
        public bool WriteU16Array(UInt16[] shorts)
        {
            var bytes = new List<byte>();
            {
                foreach (UInt16 u in shorts)
                {
                    bytes.AddRange(BitConverter.GetBytes(u));
                }
            }
            return WriteByteArray(bytes.ToArray());
        }

        /// <summary>
        ///     Sucht nach einem "freihen" Platz in der Rom, der durch d
        /// </summary>
        /// <param name="length"></param>
        /// <param name="freespace"></param>
        /// <param name="align"></param>
        /// <returns></returns>
        public UInt32 GetFreeSpaceOffset(int length, byte freespace, byte align)
        {
            int counter = 0;
            _romstream.Position -= _romstream.Position%align;
            var offset = (UInt32) _romstream.Position;
            while (counter < length && _romstream.Position < _romstream.Length)
            {
                if (_romReader.ReadByte() != freespace)
                {
                    counter = 0;
                    offset = (UInt32) ((_romstream.Position - _romstream.Position%align) + align);
                }
                else
                {
                    counter++;
                }
            }
            if (counter != length)
            {
                throw new Exception(
                    "An der angegebenen Stelle wurde kein freihes Offset mit den angegebenen Kriterien gefunden!");
            }
            return offset;
        }

        /// <summary>
        ///     Durchsucht das Rom Objekt nach der angegebenen Byte Sequenz und gibt ein IEnumerable mit allen Positionen zurück
        /// </summary>
        /// <param name="sequence">Die zu suchende Sequenz</param>
        /// <returns>Byte Array mit allen Positionen an denen Sequence vorkommt</returns>
        public IEnumerable<int> FindByteSequence(byte[] sequence)
        {
            return ArrayHelper.FindAll(_rawdata, sequence);
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Schreibt Das IRomWritable Objekt an aktueller Position ins Rom und erhöht die Position um seine Länge.
        /// </summary>
        /// <param name="data">Ein Obejekt welches IRomWritable implementiert.</param>
        public void WriteToRom(IRomWritable data)
        {
            WriteByteArray(data.GetRawData());
        }

        public void WriteToRom(IRepointable data)
        {
            data.SetCurrentOffset((uint) _romstream.Position);
            WriteByteArray(data.GetRawData());
        }

        /// <summary>
        ///     Sucht alle Pointer auf das IRepointable Objekt, ändert sie, sodass sie auf das neue Offset zeigen und schreibt das
        ///     Objekt an die neue Stelle
        /// </summary>
        /// <param name="data">Ein Objekt welches IRepointable implementiert</param>
        /// <param name="newOffset">Die neue Position des Objekts</param>
        public void Repoint(IRepointable data, UInt32 newOffset)
        {
            newOffset |= 0x8000000;
            UInt32 oldOffset = data.GetCurrentOffset() | 0x8000000;
            var oldOffsets = new List<int>();
            oldOffsets.AddRange(FindByteSequence(BitConverter.GetBytes(oldOffset)));

            foreach (int offset in oldOffsets)
            {
                SetStreamOffset(offset);
                WriteUInt32(newOffset);
            }
            SetStreamOffset(data.GetCurrentOffset());
            WriteByteArray(Enumerable.Repeat((byte) 0xFF, data.GetOriginalSize()).ToArray());
            SetStreamOffset(newOffset & 0x1FFFFFF);
            WriteToRom(data);
        }


        private void DisposeStream()
        {
            _romstream.Close();
            _romstream.Dispose();
        }

        /// <summary>
        ///     Setzt die aktuelle Position manuell auf Offset
        /// </summary>
        /// <param name="offset">Neue Carretposition, muss kleiner als die Länge des Roms sein</param>
        public void SetStreamOffset(long offset)
        {
            if (offset > _romstream.Length)
            {
                throw new ArgumentOutOfRangeException("offset",
                    "Die angegebene Position befindet sich nicht im Bereich des ROMs");
            }
            _romstream.Position = offset;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisposeStream();
                    _romReader.Close();
                    _romWriter.Close();
                }
                _rawdata = null;
                _patchentries = null;
                _disposed = true;
            }
        }

        #endregion
    }
}