using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Single.Core
{
    public interface IRepointable : IRomWritable
    {
        int GetSize();
        UInt32 GetCurrentOffset();
        void SetCurrentOffset(UInt32 Offset);
    }
}
