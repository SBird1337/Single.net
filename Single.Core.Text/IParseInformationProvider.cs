using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Single.Core.Text
{
    public interface IParseInformationProvider
    {
        string getReadableFormat(string input);
        string getTableFormat(string input);
    }
}
