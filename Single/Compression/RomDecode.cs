﻿using System;
using System.Collections.Generic;
using System.IO;
using Single.Core;

namespace Single.Compression
{
    public static class RomDecode
    {
        #region Constants

        private const int SLIDING_WINDOW_SIZE = 4096;
        private const int READ_AHEAD_BUFFER_SIZE = 18;
        private const int BLOCK_SIZE = 8;

        private const int SCAN_DEEPNESS = 4;

        #endregion

        #region Functions

        /// <summary>
        ///     Komprimiert input mit dem NLZ.gba LZ77 Algorithmus
        /// </summary>
        /// <param name="input">Zu komrpimierendes Byte Array</param>
        /// <returns>Komprimiertes Byte Array</returns>
        public static byte[] LzCompressData(byte[] input)
        {
            var ms = new MemoryStream(input);
            var br = new BinaryReader(ms);
            return Compress(br, 0, input.Length);
        }

        /// <summary>
        ///     Scant das angegebene Rom auf LZ77 komprimierte Daten
        /// </summary>
        /// <param name="input">Zu verwendendes Rom</param>
        /// <returns></returns>
        public static List<uint> Scan(Rom input)
        {
            var ms = new MemoryStream(input.RawData);
            var output = new List<uint>();
            while (ms.Position < ms.Length - 4)
            {
                var position = (UInt32) ms.Position;
                if (CanBeUnCompressed(ms, (int) ms.Position))
                    output.Add(position);
                ms.Position = position + SCAN_DEEPNESS;
            }
            return output;
        }

        /// <summary>
        ///     Dekomprimiert ein LZ77 Objekt im Rom input, an Position Offset
        /// </summary>
        /// <param name="input">Rom Objekt in welchem sich die zu dekomprimierenden Daten befinden</param>
        /// <param name="offset">Position der Daten im Rom Objekt</param>
        /// <returns>Liste mit dekomprimierten Byte Werten</returns>
        [Obsolete("Use the new Method LzUncompress and its overloads")]
        public static byte[] UnlzFromOffset(Rom input, UInt32 offset)
        {
            var ms = new MemoryStream(input.RawData) {Position = offset};
            return Unlz(ms);
        }

        /// <summary>
        ///     Dekomprimiert ein LZ77 Objekt aus dem byte input
        /// </summary>
        /// <param name="input">Komprimiertes LZ77 Objekt</param>
        /// <returns>Liste mit dekomprimierten Byte Werten</returns>
        [Obsolete("Use the new Method LzUncompress and its overloads")]
        public static byte[] UnlzData(byte[] input)
        {
            var ms = new MemoryStream(input);
            return Unlz(ms);
        }

        private static unsafe byte[] Compress(BinaryReader br, int offset, int lenght)
        {
            br.BaseStream.Position = offset;
            if (br.BaseStream.Length < offset + lenght)
            {
                return null;
            }
            byte[] uncompressedData = br.ReadBytes(lenght);

            fixed (byte* uncomp = &uncompressedData[0])
            {
                return Compress(uncomp, lenght);
            }
        }

        private static unsafe byte[] Compress(byte* source, int lenght)
        {
            int position = 0;

            var compressedData = new List<byte> {0x10};

            {
                var pointer = (byte*) &lenght;
                for (int i = 0; i < 3; i++)
                {
                    compressedData.Add(*(pointer++));
                }
            }

            while (position < lenght)
            {
                byte isCompressed = 0;
                var tempList = new List<byte>();

                for (int i = 0; i < BLOCK_SIZE; i++)
                {
                    int[] searchResult = Search(source, position, lenght);

                    if (searchResult[0] > 2)
                    {
                        var add = (byte) ((((searchResult[0] - 3) & 0xF) << 4) + (((searchResult[1] - 1) >> 8) & 0xF));
                        tempList.Add(add);
                        add = (byte) ((searchResult[1] - 1) & 0xFF);
                        tempList.Add(add);
                        position += searchResult[0];
                        isCompressed |= (byte) (1 << (8 - i - 1));
                    }
                    else if (searchResult[0] >= 0)
                        tempList.Add(*(source + position++));
                    else
                        break;
                }
                compressedData.Add(isCompressed);
                compressedData.AddRange(tempList);
            }
            while (compressedData.Count%4 != 0)
                compressedData.Add(0);

            return compressedData.ToArray();
        }

        private static unsafe int[] Search(byte* source, int position, int lenght)
        {
            var results = new List<int>();

            if ((position < 3) || ((lenght - position) < 3))
                return new[] {0, 0};
            if (!(position < lenght))
                return new[] {-1, 0};

            for (int i = 1; ((i < SLIDING_WINDOW_SIZE) && (i < position)); i++)
            {
                if (*(source + position - i - 1) == *(source + position))
                {
                    results.Add(i + 1);
                }
            }
            if (results.Count == 0)
                return new[] {0, 0};

            int amountOfBytes = 0;

            while (amountOfBytes < READ_AHEAD_BUFFER_SIZE)
            {
                amountOfBytes++;
                bool Break = false;
                for (int i = 0; i < results.Count; i++)
                {
                    if (*(source + position + amountOfBytes) !=
                        *(source + position - results[i] + (amountOfBytes%(results[i]))))
                    {
                        if (results.Count > 1)
                        {
                            results.RemoveAt(i);
                            i--;
                        }
                        else
                            Break = true;
                    }
                }
                if (Break)
                    break;
            }
            return new[] {amountOfBytes, results[0]}; //lenght of data is first, then position
        }

        private static bool CanBeUnCompressed(Stream input, int offset)
        {
            var br = new BinaryReader(input);
            br.BaseStream.Position = offset;
            uint size = br.ReadUInt32();
            if ((size & 0xFF) != 0x10)
                return false;

            size >>= 8;
            int uncompressedDataSize = 0;

            while (uncompressedDataSize < size)
            {
                if (br.BaseStream.Position + 1 > br.BaseStream.Length)
                    return false;
                byte isCompressed = br.ReadByte();

                for (int i = 0; i < BLOCK_SIZE; i++)
                {
                    if ((isCompressed & 0x80) != 0)
                    {
                        if (br.BaseStream.Position + 2 > br.BaseStream.Length)
                            return false;

                        byte first = br.ReadByte();
                        byte second = br.ReadByte();
                        int amountToCopy = 3 + ((first >> 4));
                        int copyFrom = 1 + ((first & 0xF) << 8) + second;

                        if (copyFrom > uncompressedDataSize)
                            return false;

                        uncompressedDataSize += amountToCopy;
                    }
                    else
                    {
                        if (br.BaseStream.Position + 1 > br.BaseStream.Length)
                            return false;

                        br.BaseStream.Position++;
                        uncompressedDataSize++;
                    }
                    isCompressed <<= 1;
                }
            }
            return true;
        }

        [Obsolete("Use the new Method LzUncompress and its overloads")]
        private static byte[] Unlz(Stream input)
        {
            long op = input.Position;
            if (!CanBeUnCompressed(input, (int) input.Position))
                throw new Exception(string.Format("Der angegebene Byte-Stream kann nicht dekomprimiert werden: 0x{0}", op.ToString("X")));
            var output = new List<Byte>();
            input.Position = op;
            {
                var bw = new BinaryReader(input);
                UInt32 lzhead = bw.ReadUInt32();
                UInt32 lenght = lzhead >> 8;

                do
                {
                    byte decoder = bw.ReadByte();
                    for (int i = 0; i < 8; i++)
                    {
                        bool usingbit = (decoder & (1 << 7 - i)) != 0;
                        if (usingbit == false)
                        {
                            output.Add(bw.ReadByte());
                        }
                        else
                        {
                            UInt16 b1 = Convert.ToUInt16(input.ReadByte() << 8);
                            UInt16 b2 = bw.ReadByte();
                            UInt16 codec = Convert.ToUInt16(b1 | b2);
                            UInt16 backsize = Convert.ToUInt16((codec & 4095) + 1);
                            UInt16 backlenght = Convert.ToUInt16((codec >> 12) + 3);
                            var backbuffer = new List<Byte>();

                            int index = output.Count - backsize;
                            while (backbuffer.Count < backlenght)
                            {
                                backbuffer.Add(output[index]);
                                index++;
                                if (index >= output.Count)
                                {
                                    index = output.Count - backsize;
                                }
                            }
                            output.AddRange(backbuffer);
                        }
                    }
                } while (output.Count < lenght);
                int removing = output.Count - (int) lenght;
                output.RemoveRange((int) lenght, removing);
            }
            return output.ToArray();
        }

        public static byte[] LzUncompress(Stream input, out long length)
        {
            BinaryReader reader = new BinaryReader(input);
            long originalPosition = input.Position;
            int size = (int) reader.ReadUInt32() >> 8;

            List<byte> outputList = new List<byte>();
            while (outputList.Count < size)
            {
                LzCompound compound = new LzCompound(reader);
                compound.Decode(outputList);
            }
            byte[] output = new byte[size];
            
            Buffer.BlockCopy(outputList.ToArray(), 0, output, 0, size);
            length = input.Position - originalPosition;
            return output;
        }

        public static byte[] LzUncompress(Rom input, uint offset, out long length)
        {
            input.SetStreamOffset(offset);
            return LzUncompress(input.Stream, out length);
        }



        #endregion
    }
}