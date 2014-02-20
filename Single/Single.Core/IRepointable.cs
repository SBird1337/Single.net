using System;

namespace Single.Core
{
    public interface IRepointable : IRomWritable
    {
        int GetSize();
        UInt32 GetCurrentOffset();
        void SetCurrentOffset(UInt32 offset);
        int GetOriginalSize();
    }
}