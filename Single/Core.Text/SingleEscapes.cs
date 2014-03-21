using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Single.Core.Text
{
    public class SingleEscapes : IEscapeSequenceProvider
    {
        public IEscapeSequence[] Sequences
        {
            get { return new IEscapeSequence[]{new LongEscape()}; }
        }
    }
}
