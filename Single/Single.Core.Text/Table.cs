using System;
using System.IO;
using System.Text;

namespace Single.Core.Text
{
    public class Table
    {
        #region Fields

        private readonly BiDictionary<string, byte> dict;

        #endregion

        #region Constructors

        /// <summary>
        ///     Erstellt einen Table aus einer Tablefile Datei
        /// </summary>
        /// <param name="path">Pfad zur Datei, benötigt Leserecht</param>
        public Table(string path)
        {
            dict = new BiDictionary<string, byte>();
            var fs = new FileStream(path, FileMode.Open);
            var sr = new StreamReader(fs, Encoding.UTF8);
            string table = sr.ReadToEnd();
            fs.Close();
            sr.Dispose();
            foreach (string line in table.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None))
            {
                string[] parts = line.Split('=');
                dict.Add(parts[1], Convert.ToByte(parts[0], 16));
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
            foreach (byte b in rawData)
            {
                if (dict.ContainsKey2(b))
                {
                    sb.Append(dict.GetValueByKey2(b));
                }
                else
                {
                    sb.Append("#");
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
                if (dict.ContainsKey1(input.Substring(i, inLenght)))
                {
                    bw.Write(dict.GetValueByKey1(input.Substring(i, inLenght)));
                    i += inLenght;
                    inLenght = 1;
                }
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