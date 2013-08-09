using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Single.Core.Text
{
    internal class NoneParser : IParseInformationProvider
    {
        #region Functions

        public string getReadableFormat(string input)
        {
            return input;
        }

        public string getTableFormat(string input)
        {
            return input;
        }

        #endregion

    }
}
