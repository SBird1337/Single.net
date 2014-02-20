using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Single.Core
{
    public class Romheader : IRomWritable
    {
        #region Constants

        public const int BOOT_LOGO_SIZE = 156;

        #endregion

        #region Fields

        private byte[] bootlogo;

        #endregion

        #region Properties

        public UInt32 EntryOPCode
        { get; set; }
        public string Title
        { get; set; }
        public string GameCode
        { get; set; }
        public string MakerCode
        { get; set; }
        public byte DeviceType
        { get; set; }
        public byte SoftwareVersion
        { get; set; }
        public byte MainUnit
        { get; set; }

        public byte[] BootLogo
        {
            get
            {
                return bootlogo;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Erstellt einen Header aus dem angegebenen Byte Array, welches die Daten dazu enthält
        /// </summary>
        /// <param name="input">Rohdaten des Romheaders, muss 192 Bytes lang sein</param>
        public Romheader(byte[] input)
        {
            if (!(input.Length == 192))
                throw new ArgumentException("Die zur verfügung gestellten Daten sind fehlerhaft");
            MemoryStream ms = new MemoryStream(input);
            BinaryReader br = new BinaryReader(ms);
            this.EntryOPCode = br.ReadUInt32();
            this.bootlogo = br.ReadBytes(BOOT_LOGO_SIZE);
            this.Title = ASCIIEncoding.Default.GetString(br.ReadBytes(12));
            this.GameCode = ASCIIEncoding.Default.GetString(br.ReadBytes(4));
            this.MakerCode = ASCIIEncoding.Default.GetString(br.ReadBytes(2));
            br.ReadByte();
            this.MainUnit = br.ReadByte();
            this.DeviceType = br.ReadByte();
            br.ReadBytes(7);
            this.SoftwareVersion = br.ReadByte();
            br.ReadBytes(3);
        }

        #endregion

        #region Functions

        /// <summary>
        /// Gibt die Daten es Headers zurück wie sie im Rom stehen
        /// </summary>
        /// <returns>Byte Array mit den Headerdaten</returns>
        public byte[] GetRawData()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(this.EntryOPCode);
            bw.Write(this.bootlogo);
            bw.Write(ASCIIEncoding.Default.GetBytes(this.Title));
            bw.Write(ASCIIEncoding.Default.GetBytes(this.GameCode));
            bw.Write(ASCIIEncoding.Default.GetBytes(this.MakerCode));
            bw.Write(0x96);
            bw.Write(this.MainUnit);
            bw.Write(this.DeviceType);
            bw.Write(new Byte[] { 0, 0, 0, 0, 0, 0, 0 });
            bw.Write(this.SoftwareVersion);

            UInt32 ComplementCheck = 0;
            BinaryReader br = new BinaryReader(ms);
            ms.Position = 0xA0;
            for (int i = 0xA0; i < 0xBC; ++i)
            {
                ComplementCheck = (UInt32)(ComplementCheck - br.ReadByte());
            }
            ms.Position++;
            bw.Write((byte)((ComplementCheck - 0x19) & 0xFF));
            bw.Write(new byte[] { 0, 0 });

            return ms.ToArray();
        }

        #endregion
    }
}
