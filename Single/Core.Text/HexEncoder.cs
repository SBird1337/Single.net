﻿namespace Single.Core.Text
{
    public class HexEncoder
    {
        private readonly IParseInformationProvider _parser;

        #region Properties

        public Table EncodingTable { get; set; }

        #endregion

        #region Contructors

        public HexEncoder(Table tbl, IParseInformationProvider parser)
        {
            EncodingTable = tbl;
            _parser = parser;
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Gibt eine Zeichenkette zurück, die anhand des IParseInformationProviders und des Tables konvertiert werden
        /// </summary>
        /// <param name="stringData">Kodierte Hex-Daten eines Strings wie er im Rom stehen würde</param>
        /// <returns>Lesbare Zeichenkette</returns>
        public string GetParsedString(byte[] stringData)
        {
            return _parser.GetReadableFormat(EncodingTable.Decode(stringData));
        }

        /// <summary>
        ///     Gibt ein Byte Array zurück, welches die Zeichenkette input repräsentiert
        /// </summary>
        /// <param name="input">Lesbare Zeichenkette</param>
        /// <returns>Kodierte Hex-Daten des Strings wie er im Rom stehen würde</returns>
        public byte[] GetParsedBytes(string input)
        {
            return EncodingTable.Encode(_parser.GetTableFormat(input));
        }

        /// <summary>
        ///     Überprüft die angegebene Zeichenkette auf Validität (Wird beim Parsen eine Ausnahme geworfen?)
        /// </summary>
        /// <param name="s">Zu prüfende Zeichenkette</param>
        /// <returns></returns>
        public bool IsValidPString(string s)
        {
            try
            {
                GetParsedBytes(s);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}