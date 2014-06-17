using System;
using System.Collections.Generic;
using System.Linq;

namespace Single.Extensions
{
    public static class ArrayExtensions
    {
        public static List<int> IndexOfSequence(this byte[] buffer, byte[] pattern, int startIndex)
        {
            List<int> positions = new List<int>();
            int i = Array.IndexOf(buffer, pattern[0], startIndex);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual(pattern))
                    positions.Add(i);
                i = Array.IndexOf(buffer, pattern[0], i + pattern.Length);
            }
            return positions;
        }
    }
}
