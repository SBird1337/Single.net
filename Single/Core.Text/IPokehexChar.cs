using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Single.Core.Text
{
    public enum CharType
    {
        Byte, Halfword
    }

    public interface IPokehexChar
    {
        CharType Type { get; }
        object Value { get; }
        string ReadableValue { get; }
    }
}
