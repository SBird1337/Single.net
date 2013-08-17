using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Single.Core;

namespace Single.Core.Text
{
    public static class RomStringHelper
    {
        #region Fields

        public static IParseInformationProvider ParseNone = new NoneParser();
        public static IParseInformationProvider ParseNormal = new NormalParser();

        #endregion

        #region Functions

        /// <summary>
        /// Ließt ein Array aus Bytes bis das endByte(falls gesetzt, ansonsten 0xFF) erreicht ist am angegebenen Offset
        /// </summary>
        /// <param name="input">Zu verwendendes Rom Objekt</param>
        /// <param name="offset">Startoffset</param>
        /// <param name="endByte">Letztes Byte eines Strings im Rom, meist 0xFF</param>
        /// <returns>Ein Byte Array mit Daten, die einen String im Rom beschreiben</returns>
        public static byte[] ReadRomString(Rom input, long offset, byte endByte = 0xFF)
        {
            input.SetStreamOffset(offset);
            List<byte> output = new List<byte>();
            byte current = input.ReadByte();
            while (current != endByte)
            {
                output.Add(current);
                current = input.ReadByte();
            }
            return output.ToArray();
        }

        public static byte[] ReadRomString(Rom input, byte endByte = 0xFF)
        {
            List<byte> output = new List<byte>();
            byte current = input.ReadByte();
            while (current != endByte)
            {
                output.Add(current);
                current = input.ReadByte();
            }
            return output.ToArray();
        }

        #endregion
    }
}
