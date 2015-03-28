using System;
using System.IO;
using System.Linq;

namespace Single.Core.Text
{
    public class TableEntry
    {
        public byte[] Representation { get; set; }
        public string Display { get; set; }

        public TableEntry(byte[] representation, string display)
        {
            Representation = representation;
            Display = display;
        }

        public static TableEntry FromString(string input, IEscapeSequenceProvider sequenceProvider)
        {
            IEscapeSequence[] sequences = sequenceProvider.Sequences;
            string[] split = input.Split('=');
            string hex = split[0];
            string disp = split[1];

            var sr = new StringReader(hex);
            var c = (char)sr.Peek();

            if (c != '\\')
            {
                return new TableEntry(new[] { Convert.ToByte(sr.ReadToEnd(), 16)}, disp);
            }
            sr.Read();
            var s = (char)sr.Read();
            if (sequences.All(sequence => sequence.Sequence != s))
                throw new Exception(string.Format("Escape Sequenz {0} wurde nicht gefunden", s));
            IEscapeSequence foundSequence = sequences.First(sequence => sequence.Sequence == s);
            return new TableEntry(foundSequence.Resolve(sr), disp);
        }
    }
}
