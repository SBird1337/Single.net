using System;
using System.IO;
using System.Text;

namespace Single.Core
{
    public class Romheader : IRomWritable
    {
        #region Constants

        public const int BOOT_LOGO_SIZE = 156;

        #endregion

        #region Fields

        private readonly byte[] _bootlogo;

        #endregion

        #region Properties

        public UInt32 EntryOpCode { get; set; }
        public string Title { get; set; }
        public string GameCode { get; set; }
        public string MakerCode { get; set; }
        public byte DeviceType { get; set; }
        public byte SoftwareVersion { get; set; }
        public byte MainUnit { get; set; }

        public byte[] BootLogo
        {
            get { return _bootlogo; }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Erstellt einen Header aus dem angegebenen Byte Array, welches die Daten dazu enthält
        /// </summary>
        /// <param name="input">Rohdaten des Romheaders, muss 192 Bytes lang sein</param>
        public Romheader(byte[] input)
        {
            if (input.Length != 192)
                throw new ArgumentException("Die zur verfügung gestellten Daten sind fehlerhaft");
            var ms = new MemoryStream(input);
            var br = new BinaryReader(ms);
            EntryOpCode = br.ReadUInt32();
            _bootlogo = br.ReadBytes(BOOT_LOGO_SIZE);
            Title = Encoding.Default.GetString(br.ReadBytes(12));
            GameCode = Encoding.Default.GetString(br.ReadBytes(4));
            MakerCode = Encoding.Default.GetString(br.ReadBytes(2));
            br.ReadByte();
            MainUnit = br.ReadByte();
            DeviceType = br.ReadByte();
            br.ReadBytes(7);
            SoftwareVersion = br.ReadByte();
            br.ReadBytes(3);
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Gibt die Daten es Headers zurück wie sie im Rom stehen
        /// </summary>
        /// <returns>Byte Array mit den Headerdaten</returns>
        public byte[] GetRawData()
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);

            bw.Write(EntryOpCode);
            bw.Write(_bootlogo);
            bw.Write(Encoding.Default.GetBytes(Title));
            bw.Write(Encoding.Default.GetBytes(GameCode));
            bw.Write(Encoding.Default.GetBytes(MakerCode));
            bw.Write(0x96);
            bw.Write(MainUnit);
            bw.Write(DeviceType);
            bw.Write(new Byte[] {0, 0, 0, 0, 0, 0, 0});
            bw.Write(SoftwareVersion);

            UInt32 complementCheck = 0;
            var br = new BinaryReader(ms);
            ms.Position = 0xA0;
            for (int i = 0xA0; i < 0xBC; ++i)
            {
                complementCheck = complementCheck - br.ReadByte();
            }
            ms.Position++;
            bw.Write((byte) ((complementCheck - 0x19) & 0xFF));
            bw.Write(new byte[] {0, 0});

            return ms.ToArray();
        }

        #endregion
    }
}