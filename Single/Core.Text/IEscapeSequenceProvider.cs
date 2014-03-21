using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Single.Core.Text
{
    public interface IEscapeSequenceProvider
    {
        IEscapeSequence[] Sequences { get; }
    }
}
