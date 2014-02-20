using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Single.Core.Text
{
    public class HexEncoder
    {

        private IParseInformationProvider parser;

        #region Properties

        public Table EncodingTable
        { get; set; }

        #endregion

        #region Contructors

        public HexEncoder(Table tbl, IParseInformationProvider parser)
        {
            this.EncodingTable = tbl;
            this.parser = parser;
        }

        #endregion

        #region Functions

        /// <summary>
        /// Gibt eine Zeichenkette zurück, die anhand des IParseInformationProviders und des Tables konvertiert werden
        /// </summary>
        /// <param name="stringData">Kodierte Hex-Daten eines Strings wie er im Rom stehen würde</param>
        /// <returns>Lesbare Zeichenkette</returns>
        public string getParsedString(byte[] stringData)
        {
            return this.parser.getReadableFormat(this.EncodingTable.Decode(stringData));
        }

        /// <summary>
        /// Gibt ein Byte Array zurück, welches die Zeichenkette input repräsentiert
        /// </summary>
        /// <param name="input">Lesbare Zeichenkette</param>
        /// <returns>Kodierte Hex-Daten des Strings wie er im Rom stehen würde</returns>
        public byte[] getParsedBytes(string input)
        {
            return this.EncodingTable.Encode(this.parser.getTableFormat(input));
        }

        #endregion

    }
}
