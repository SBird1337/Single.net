using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Single.Core.Text
{
    public abstract class PokehexCharBase : IPokehexChar
    {
        private readonly byte _byteValue;
        private readonly ushort _hwValue;
        private readonly string _readable;

        protected PokehexCharBase(object value, string readable)
        {
            _readable = readable;
            if (Type == CharType.Byte)
                _byteValue = (byte)value;
            else
            {
                _hwValue = (ushort)value;
            }
        }

        public abstract CharType Type { get; }

        public object Value
        {
            get
            {
                return Type == CharType.Byte ? _byteValue : _hwValue;
            }
        }

        public string ReadableValue {
            get { return _readable; }
        }
    }
}
