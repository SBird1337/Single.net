using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Single.Core
{
    public static class ArrayHelper
    {
        #region Functions
        /// <summary>
        /// Gibt alle Vorkommen von Sequence in SearchArray zurück
        /// </summary>
        /// <param name="SearchArray">Das zu durchsuchende Array</param>
        /// <param name="Sequence">Die zu suchende Byte Sequenz</param>
        /// <returns></returns>
        public static IEnumerable<int> FindAll(byte[] SearchArray, byte[] Sequence)
        {
            Encoding latin1 = Encoding.GetEncoding("iso-8859-1");
            string sHaystack = latin1.GetString(SearchArray);
            string sNeedle = latin1.GetString(Sequence);
            for (Match m = Regex.Match(sHaystack, Regex.Escape(sNeedle)); m.Success; m = m.NextMatch())
            {
                yield return m.Index;
            }
        }
        #endregion
    }
}
