using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Single.Core.Text
{
    public class SpecialChar : PokehexCharBase
    {
        public SpecialChar(object value, string readable) : base(value, readable)
        {}

        public override CharType Type
        {
            get { return CharType.Halfword; }
        }
    }
}
