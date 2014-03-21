using System;
using System.Linq;

namespace Single.Core.Text
{
    public class LongEscape : IEscapeSequence
    {
        public char Sequence
        {
            get { return 's'; }
        }

        //Format: /s [11, 22, 33, 44, 55, 66, 77, 88]

        public byte[] Resolve(System.IO.StringReader reader)
        {
// ReSharper disable once PossibleNullReferenceException
            string[] bytes = reader.ReadToEnd().Replace(" ", string.Empty).Replace("[", "").Replace("]", "").Split(',');
            return bytes.Select(s => Convert.ToByte(s, 16)).ToArray();
        }
    }
}
