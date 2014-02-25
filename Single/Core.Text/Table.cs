using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Single.Core.Text
{
    public class Table
    {
        #region Fields

        private readonly List<IPokehexChar> _chars; 

        #endregion

        #region Constructors

        /// <summary>
        ///     Erstellt einen Table aus einer Tablefile Datei
        /// </summary>
        /// <param name="path">Pfad zur Datei, benötigt Leserecht</param>
        public Table(string path)
        {
            _chars = new List<IPokehexChar>();
            var fs = new FileStream(path, FileMode.Open);
            var sr = new StreamReader(fs, Encoding.UTF8);
            string table = sr.ReadToEnd();
            fs.Close();
            sr.Dispose();
            foreach (string line in table.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None))
            {
                string[] parts = line.Split('=');
                if (parts[0].Length < 3)
                    _chars.Add(new NormalChar(Convert.ToByte(parts[0], 16), parts[1]));
                else
                    _chars.Add(new SpecialChar(Convert.ToUInt16(parts[0], 16), parts[1]));
            }
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Dekodiert ein Bytearray zu einem string wie im Tablefile angegeben
        /// </summary>
        /// <param name="rawData">Daten, die einen string im entsprechenden Hex-Format darstellen</param>
        /// <returns>Lesbare ASCII Zeichenkette(string) der Daten</returns>
        public string Decode(byte[] rawData)
        {
            var sb = new StringBuilder();
            BinaryReader br = new BinaryReader(new MemoryStream(rawData));
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                byte b = br.ReadByte();
                IEnumerable<IPokehexChar> foundCharsByte =
                    _chars.Where(value => value.Type == CharType.Byte && Convert.ToByte(value.Value) == b);
                var bytePokehexChar = foundCharsByte as IPokehexChar[] ?? foundCharsByte.ToArray();
                if (bytePokehexChar.Count() > 1)
                    throw new Exception("Doppelter Eintrag im Table File");
                if(bytePokehexChar.Any())
                    sb.Append(bytePokehexChar.First().ReadableValue);
                else
                {
                    br.BaseStream.Position--;
                    if (br.BaseStream.Position + 1 >= br.BaseStream.Length)
                    {
                        sb.Append("#");
                        break;
                    }
                    ushort s = br.ReadUInt16();
                    IEnumerable<IPokehexChar> foundCharsShort =
                        _chars.Where(value => value.Type == CharType.Halfword && (ushort) value.Value == s);
                    var shortPokehexChar = foundCharsShort as IPokehexChar[] ?? foundCharsShort.ToArray();
                    if(shortPokehexChar.Count() > 1)
                        throw new Exception("Doppelter Eintrag im Table File");
                    sb.Append(shortPokehexChar.Any() ? shortPokehexChar.First().ReadableValue : "#");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        ///     Erzeugt ein kodiertes Byte Array nach den Vorschriften des Tables aus dem input
        /// </summary>
        /// <param name="input">ASCII Zeichenkette, die zu kodieren ist</param>
        /// <returns>Byte Array, im entsprechenden Hex-Format, welches input repräsentiert</returns>
        public byte[] Encode(string input)
        {
            int inLenght = 1;
            int i = 0;
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            while (i < input.Length)
            {
                if (i + inLenght > input.Length)
                {
                    throw new Exception("Der angegebene String konnte nicht im TableFile gefunden werden.");
                }

                int lenght = inLenght;
                IPokehexChar[] foundChars =
                    _chars.Where(value => value.ReadableValue == input.Substring(i, lenght)).ToArray();
                if(foundChars.Count() > 1)
                    throw new Exception("Doppelter Eintrag im Table File");
                if (foundChars.Any())
                {
                    IPokehexChar foundChar = foundChars.First();
                    if(foundChar.Type == CharType.Byte)
                        bw.Write(Convert.ToByte(foundChar.Value));
                    else
                        bw.Write((ushort)foundChar.Value);
                    i += inLenght;
                    inLenght = 1;
                }
                    //if (_dict.ContainsKey1(input.Substring(i, inLenght)))
                    //{
                    //    bw.Write(_dict.GetValueByKey1(input.Substring(i, inLenght)));
                    //    i += inLenght;
                    //    inLenght = 1;
                    //}
                else
                {
                    inLenght++;
                }
            }
            byte[] output = ms.ToArray();
            ms.Close();
            return output;
        }

        #endregion
    }
}