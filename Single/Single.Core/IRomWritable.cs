using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Single.Core
{
    public interface IRomWritable
    {
        byte[] GetRawData();
    }
}
