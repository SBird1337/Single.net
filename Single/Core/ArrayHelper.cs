using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Single.Core
{
    public static class ArrayHelper
    {
        #region Functions

        /// <summary>
        ///     Gibt alle Vorkommen von Sequence in SearchArray zurück
        /// </summary>
        /// <param name="searchArray">Das zu durchsuchende Array</param>
        /// <param name="sequence">Die zu suchende Byte Sequenz</param>
        /// <returns></returns>
        [Obsolete("Use the extension method of byte[]")]
        public static IEnumerable<int> FindAll(byte[] searchArray, byte[] sequence)
        {
            Encoding latin1 = Encoding.GetEncoding("iso-8859-1");
            string sHaystack = latin1.GetString(searchArray);
            string sNeedle = latin1.GetString(sequence);
            for (Match m = Regex.Match(sHaystack, Regex.Escape(sNeedle)); m.Success; m = m.NextMatch())
            {
                yield return m.Index;
            }
        }

        

        #endregion
    }
}