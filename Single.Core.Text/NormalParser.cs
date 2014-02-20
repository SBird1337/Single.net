using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Single.Core.Text
{
    internal class NormalParser : IParseInformationProvider
    {
        #region Functions

        public string getReadableFormat(string input)
        {
            input = input.Replace("¶", "\r\n").Replace("<AE>", "Ä").Replace("<OE>", "Ö").Replace("<UE>", "Ü").Replace("<ae>", "ä").Replace("<oe>", "ö").Replace("<ue>", "ü").Replace("_", " ");
            return input;
        }

        public string getTableFormat(string input)
        {
            input = input.Replace("\r\n", "¶").Replace("Ä", "<AE>").Replace("Ö", "<OE>").Replace("Ü", "<UE>").Replace("ä", "<ae>").Replace("ö", "<oe>").Replace("ü", "<ue>").Replace(" ", "_");
            return input;
        }

        #endregion
    }
}
