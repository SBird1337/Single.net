using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
            public UInt32 offset;
            public byte[] data;
        }

        #endregion

        #region Constructors
        /// <summary>
        /// Erstellt ein Rom Objekt mit angegebener Länge aus dem angegebenen Stream
        /// </summary>
        /// <param name="Input">Der Eingangsstream mit festgelegter Leseposition</param>
        /// <param name="romlenght">Die Länge des Rom Objekts, als Hilfe dienen die statischen Konstanten in der Rom Klasse</param>
        public Rom(Stream Input, UInt32 romlenght)
        {
            if(!this.Load(Input, romlenght))
                throw new ArgumentException();
        }
        /// <summary>
        /// Erstellt einen Filestream der angegebenen Datei und bildet aus der Datei und ihrer Länge ein Rom Objekt
        /// </summary>
        /// <param name="path">Der Pfad zur Rom Datei</param>
        public Rom(String path)
        {
            Stream instream = new FileStream(path, FileMode.Open);
            if (!this.Load(instream, (UInt32)instream.Length))
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
        private BinaryReader romReader;
        private BinaryWriter romWriter;
        private MemoryStream romstream;
        private bool disposed = false;
        private byte[] rawdata;
        private List<PatchEntry> patchentries = new List<PatchEntry>();
        #endregion

        #region Properties

        public Romheader Header
        { get; set; }

        public byte[] RawData
        {
            get
            {
                return rawdata;
            }
        }

        public long CurrentPosition
        {
            get
            {
                return this.romstream.Position;
            }
        }
        #endregion

        #region Functions

        private Boolean Load(Stream instream, UInt32 romlenght)
        {
            if (instream != null && instream.Position + romlenght <= instream.Length)
            {
                BinaryReader br = new BinaryReader(instream);
                rawdata = br.ReadBytes((int)romlenght);
                romstream = new MemoryStream(rawdata);
                romReader = new BinaryReader(romstream);
                romWriter = new BinaryWriter(romstream);
                this.SetStreamOffset(0);
                this.Header = new Romheader(this.ReadByteArray(192).ToArray());
                this.SetStreamOffset(0x0);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Speichert das komplette Byte Array des Roms in die angegebene Datei
        /// </summary>
        /// <param name="path">Pfad zur Datei in welche die Daten geschrieben werden sollen, benötigt Schreibzugriff</param>
        /// <returns>Erfolg des Vorgangs</returns>
        public Boolean Save(string path)
        {
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(rawdata);
            fs.Close();
            return true;
        }

        /// <summary>
        /// Patcht die angegebene Rom Datei mit allen im Rom Objekt vorgenommenen Änderungen, überschreibt dabei keine anderen Daten
        /// </summary>
        /// <param name="path">Pfad zur Datei in welche die Daten geschrieben werden sollen, benötigt Schreibzugriff</param>
        /// <returns>Erfolg des Vorgangs</returns>
        public Boolean Patch(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            byte[] copyData = br.ReadBytes((int)fs.Length);
            if (copyData.Length != this.rawdata.Length)
            {
                fs.Close();
                throw new ArgumentException();
            }
            MemoryStream ms = new MemoryStream(copyData);
            BufferedStream bs = new BufferedStream(ms);
            BinaryWriter bw = new BinaryWriter(bs);
            try
            {
                foreach (PatchEntry entry in this.patchentries)
                {
                    bw.BaseStream.Position = entry.offset;
                    bw.Write(entry.data);
                }
                fs.Position = 0;
                bw = new BinaryWriter(fs);
                bw.Write(copyData);
                ms.Close();
                fs.Close();
            }
            catch
            {
                ms.Close();
                bs.Close();
                fs.Close();
                throw;
            }
            return true;
        }

        /// <summary>
        /// Ließt eine ASCII Zeichenkette von Lenght an der aktuellen Position und erhöht die Position um Length
        /// </summary>
        /// <param name="Length">Länge der Zeichenkette</param>
        /// <returns>ASCII Zeichenkette</returns>
        public String ReadString(int Length)
        {
            string output = "";
            for (int i = 0; i < Length; i++)
            {
                output += romReader.ReadChar();
            }
            return output;
        }
        /// <summary>
        /// Ließt ein Word ohne Vorzeichen an der aktuellen Position und erhöht die Position um 4
        /// </summary>
        /// <returns>Eingelesenes Word</returns>
        public UInt32 ReadUInt32()
        {
            return romReader.ReadUInt32();
        }
        /// <summary>
        /// Ließt ein Half-Word ohne Vorzeichen an der aktuellen Position und erhöht die Position um 2
        /// </summary>
        /// <returns>Eingelesenes Half-Word</returns>
        public UInt16 ReadUInt16()
        {
            return romReader.ReadUInt16();
        }
        /// <summary>
        /// Ließt ein Byte ohne Vorzeichen an der aktuellen Position und erhöht die Position um 1
        /// </summary>
        /// <returns>Eingelesenes Byte</returns>
        public Byte ReadByte()
        {
            return romReader.ReadByte();
        }
        /// <summary>
        /// Ließt ein Array aus Bytes mit Länge Count an der aktuellen Position und erhöht die Position um Count
        /// </summary>
        /// <param name="Count">Anzahl der Bytes im Array</param>
        /// <returns>Liste mit eingelesenen Werten</returns>
        public List<Byte> ReadByteArray(int Count)
        {
            List<Byte> output = new List<Byte>();
            output.AddRange(romReader.ReadBytes(Count));
            return output;
        }
        /// <summary>
        /// Ließt ein Array aus Half-Words ohne Vorzeichen mit Länge Count an der aktuellen Position und erhöht die Position um Count.
        /// </summary>
        /// <param name="Count">Anzahl der Half-Words</param>
        /// <returns>Liste mit den eingelesenen UInt16 Werten</returns>
        public List<UInt16> ReadUShortArray(int Count)
        {
            List<UInt16> output = new List<UInt16>();
            for (int i = 0; i < Count; i++)
            {
                output.Add(romReader.ReadUInt16());
            }
            return output;
        }

        /// <summary>
        /// Schreibt ein Byte an die aktuelle Position und erhöht die aktuelle Position um 1
        /// </summary>
        /// <param name="b">Zu schreibendes Byte</param>
        /// <returns>Erfolg des Vorgangs</returns>
        public bool WriteByte(Byte b)
        {
            return WriteByteArray(BitConverter.GetBytes(b));
        }

        /// <summary>
        /// Schreibt ein Half-Word an die aktuelle Position und erhöht die aktuelle Position um 2
        /// </summary>
        /// <param name="u16">Zu schreibendes Half-Word</param>
        /// <returns>Erfolg des Vorgangs</returns>
        public bool WriteUInt16(UInt16 u16)
        {
            return WriteByteArray(BitConverter.GetBytes(u16));
        }

        /// <summary>
        /// Schreibt ein Word an die aktuelle Position und erhöht die aktuelle Position um 4
        /// </summary>
        /// <param name="u32">Zu schreibendes Word</param>
        /// <returns>Erfolg des Vorgangs</returns>
        public bool WriteUInt32(UInt32 u32)
        {
            return WriteByteArray(BitConverter.GetBytes(u32));
        }

        /// <summary>
        /// Schreibt das angegebene Byte Array an die aktuelle Position und erhöht die aktuelle Position um die Länge des Arrays
        /// </summary>
        /// <param name="bytes">Das zu schreibende Byte Array</param>
        /// <returns>Erfolg des Vorgangs</returns>
        public bool WriteByteArray(byte[] bytes)
        {
            PatchEntry entry;
            entry.offset = (UInt32)romstream.Position;
            entry.data = bytes;
            patchentries.Add(entry);
            romWriter.Write(bytes);
            return true;
        }

        /// <summary>
        /// Schreibt das angegebene Half-Word Array an die aktuelle Position und erhöht die aktuelle Position um die Länge des Arrays
        /// </summary>
        /// <param name="bytes">Das zu schreibende Half-Word Array</param>
        /// <returns>Erfolg des Vorgangs</returns>
        public bool WriteU16Array(UInt16[] shorts)
        {
            List<byte> bytes = new List<byte>();
            {
                foreach (UInt16 u in shorts)
                {
                    bytes.AddRange(BitConverter.GetBytes(u));
                }
            }
            return WriteByteArray(bytes.ToArray());
        }

        /// <summary>
        /// Sucht nach einem "freihen" Platz in der Rom, der durch d
        /// </summary>
        /// <param name="Length"></param>
        /// <param name="freespace"></param>
        /// <param name="align"></param>
        /// <returns></returns>
        public UInt32 GetFreeSpaceOffset(int Length, byte freespace, byte align)
        {
            int counter = 0;
            romstream.Position -= romstream.Position % align;
            UInt32 Offset = (UInt32)romstream.Position;
            while (counter < Length && romstream.Position < romstream.Length)
            {
                if (romReader.ReadByte() != freespace)
                {
                    counter = 0;
                    Offset = (UInt32)((romstream.Position - romstream.Position % align) + align);
                }
                else
                {
                    counter++;
                }
            }
            if (counter != Length) { throw new Exception("An der angegebenen Stelle wurde kein freihes Offset mit den angegebenen Kriterien gefunden!"); }
            return Offset;
        }

        /// <summary>
        /// Durchsucht das Rom Objekt nach der angegebenen Byte Sequenz und gibt ein IEnumerable mit allen Positionen zurück
        /// </summary>
        /// <param name="Sequence">Die zu suchende Sequenz</param>
        /// <returns>Byte Array mit allen Positionen an denen Sequence vorkommt</returns>
        public IEnumerable<int> FindByteSequence(byte[] Sequence)
        {
            return ArrayHelper.FindAll(rawdata, Sequence);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Schreibt Das IRomWritable Objekt an aktueller Position ins Rom und erhöht die Position um seine Länge.
        /// </summary>
        /// <param name="data">Ein Obejekt welches IRomWritable implementiert.</param>
        public void WriteToRom(IRomWritable data)
        {
            this.WriteByteArray(data.GetRawData());
        }

        public void WriteToRom(IRepointable data)
        {
            data.SetCurrentOffset((uint)romstream.Position);
            this.WriteByteArray(data.GetRawData());
        }

        /// <summary>
        /// Sucht alle Pointer auf das IRepointable Objekt, ändert sie, sodass sie auf das neue Offset zeigen und schreibt das Objekt an die neue Stelle
        /// </summary>
        /// <param name="data">Ein Objekt welches IRepointable implementiert</param>
        /// <param name="newOffset">Die neue Position des Objekts</param>
        public void Repoint(IRepointable data, UInt32 newOffset)
        {
            newOffset |= 0x8000000;
            UInt32 oldOffset = data.GetCurrentOffset() | 0x8000000;
            List<int> oldOffsets = new List<int>();
            oldOffsets.AddRange(this.FindByteSequence(BitConverter.GetBytes(oldOffset)));

            foreach (int offset in oldOffsets)
            {
                this.SetStreamOffset(offset);
                this.WriteUInt32((UInt32)newOffset);
            }
            this.SetStreamOffset(data.GetCurrentOffset());
            this.WriteByteArray(Enumerable.Repeat((byte)0xFF, data.GetSize()).ToArray());
            this.SetStreamOffset(newOffset & 0x1FFFFFF);
            this.WriteToRom(data);
        }


        private void DisposeStream()
        {
            romstream.Close();
            romstream.Dispose();
        }

        /// <summary>
        /// Setzt die aktuelle Position manuell auf Offset
        /// </summary>
        /// <param name="Offset">Neue Carretposition, muss kleiner als die Länge des Roms sein</param>
        public void SetStreamOffset(long Offset)
        {
            if (Offset > romstream.Length) { throw new ArgumentOutOfRangeException("Offset", "Die angegebene Position befindet sich nicht im Bereich des ROMs"); }
            romstream.Position = Offset;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    DisposeStream();
                    romReader.Dispose();
                    romWriter.Dispose();
                }
                rawdata = null;
                patchentries = null;
                disposed = true;
            }
        }

        

        #endregion
    }
}
