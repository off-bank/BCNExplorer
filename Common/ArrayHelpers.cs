﻿using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public static class ArrayUtils
    {
        public static uint ParseuInt(this IList<byte> data, int offest = 0)
        {
            return (uint)(data[offest++] + data[offest++] * 256 + data[offest++] * 65536 + data[offest] * 16777216);
        }

        public static ushort ParseWord(this IList<byte> data, int offest = 0)
        {
            return (ushort)(data[offest++] + data[offest] *256);
        }

        public static uint ParseByte(this IList<byte> data, int offest = 0)
        {
            return data[offest];
        }


        public static void WriteuInt(this byte[] data, uint value, int offset = 0)
        {
            data[offset++] = (byte)value;
            data[offset++] = (byte)(value >> 8);
            data[offset++] = (byte)(value >> 16);
            data[offset] = (byte)(value >> 24);

        }

        public static void WriteWord(this byte[] data, ushort value, int offset = 0)
        {
            data[offset++] = (byte)value;
            data[offset] = (byte)(value >> 8);
        }


        public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(
                  this IEnumerable<TSource> source, int size)
        {
            TSource[] bucket = null;
            var count = 0;

            foreach (var item in source)
            {
                if (bucket == null)
                    bucket = new TSource[size];

                bucket[count++] = item;
                if (count != size)
                    continue;

                yield return bucket;

                bucket = null;
                count = 0;
            }

            if (bucket != null && count > 0)
                yield return bucket.Take(count);
        }
    }
}
