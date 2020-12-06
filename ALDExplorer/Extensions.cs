using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace ALDExplorer
{
    public static partial class Extensions
    {
        public static void WriteToStream(this Stream streamToReadFrom, Stream streamToWriteTo)
        {
            WriteToStream(streamToReadFrom, streamToWriteTo, streamToReadFrom.Length - streamToReadFrom.Position);
        }

        public static void WriteToStream(this Stream streamToReadFrom, Stream streamToWriteTo, long byteCount)
        {
            int bufferSize = 65536;
            byte[] buffer = new byte[bufferSize];

            long bytesRemaining = byteCount;
            while (bytesRemaining > 0)
            {
                int bytesToRead = bufferSize;
                if (bytesToRead > bytesRemaining)
                {
                    bytesToRead = (int)bytesRemaining;
                }
                int bytesRead = streamToReadFrom.Read(buffer, 0, bytesToRead);
                if (bytesRead != bytesToRead)
                {
                    //???
                    if (bytesRead == 0)
                    {
                        throw new IOException("Failed to read from the stream");
                    }
                }
                streamToWriteTo.Write(buffer, 0, bytesRead);
                bytesRemaining -= bytesRead;
            }

        }

        public static void WriteFromStream(this Stream streamToWriteTo, Stream streamToReadFrom)
        {
            WriteToStream(streamToReadFrom, streamToWriteTo, streamToReadFrom.Length - streamToReadFrom.Position);
        }

        public static void WriteFromStream(this Stream streamToWriteTo, Stream streamToReadFrom, long byteCount)
        {
            WriteToStream(streamToReadFrom, streamToWriteTo, byteCount);
        }

        public static int PeekByte(this Stream stream)
        {
            long position = stream.Position;
            int b = stream.ReadByte();
            stream.Position = position;
            return b;
        }


        /// <summary>
        /// Adds the elements of the specified sequence to the collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="collection">The collection to add items to.</param>
        /// <param name="sequence">
        /// The sequence whose elements should be added to the end of the collection.
        /// </param>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> sequence)
        {
            foreach (var item in sequence)
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// Returns a string array that contains the substrings in this string that are
        /// delimited by a specified string.
        /// </summary>
        /// <param name="str">The string to split.</param>
        /// <param name="separator">A string that delimits the substrings in this string.</param>
        /// <returns>
        /// An array whose elements contain the substrings in this string that are delimited
        /// by the separator string.
        /// </returns>
        public static string[] Split(this string str, string separator)
        {
            return str.Split(new string[] { separator }, StringSplitOptions.None);
        }

        /// <summary>
        /// Performs the specified action on each element of the sequence.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="sequence">The sequence to perform the actions on.</param>
        /// <param name="action">The delegate to perform on each element of the sequence.</param>
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            foreach (var element in sequence)
            {
                action(element);
            }
        }

        /// <summary>
        /// Performs the specified action on each element of the sequence.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="sequence">The sequence to perform the actions on.</param>
        /// <param name="action">The delegate to perform on each element of the sequence.
        /// The second int parameter is the position of the element.</param>
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T, int> action)
        {
            int i = 0;
            foreach (var element in sequence)
            {
                action(element, i);
                i++;
            }
        }

        /// <summary>
        /// Joins substrings together with separator strings.
        /// </summary>
        /// <param name="strArr">The sequence of strings to join together.</param>
        /// <param name="joinString">The string to separate the strings by</param>
        /// <returns>A string which is the result of joining the strings together.</returns>
        public static string Join(this IEnumerable<string> strArr, string joinString)
        {
            StringBuilder sb = new StringBuilder();
            int count = strArr.Count();
            int i = 0;
            foreach (var str in strArr)
            {
                sb.Append(str);
                if (i < count - 1)
                {
                    sb.Append(joinString);
                }
                i++;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Joins substrings together without separator strings.
        /// </summary>
        /// <param name="strArr">The sequence of strings to join together.</param>
        /// <returns>A string which is the result of joining the strings together.</returns>
        public static string Join(this IList<string> strArr)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var str in strArr)
            {
                sb.Append(str);
            }
            return sb.ToString();
        }

        public static TValue GetOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.GetOrDefault(key, default(TValue));
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            else
            {
                return defaultValue;
            }
        }

        public static T GetOrDefault<T>(this IList<T> list, int index, T defaultValue)
        {
            if (index >= 0 && index < list.Count)
            {
                return list[index];
            }
            else
            {
                return defaultValue;
            }
        }

        public static bool Contains(this string str, string lookFor, StringComparison comparisonType)
        {
            return str.IndexOf(lookFor, comparisonType) >= 0;
        }

        static Encoding shiftJis = Encoding.GetEncoding("shift-jis");

        public static int IndexOf(this IList<byte> array, string lookForString, int startIndex)
        {
            var lookFor = shiftJis.GetBytes(lookForString);
            for (int i = startIndex; i < array.Count - lookFor.Length; i++)
            {
                if (array[i] == lookFor[0])
                {
                    int i2;
                    for (i2 = 1; i2 < lookFor.Length; i2++)
                    {
                        if (array[i + i2] != lookFor[i2])
                        {
                            break;
                        }
                    }
                    if (i2 == lookFor.Length)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public static void Set<T>(this HashSet<T> set, T value)
        {
            if (set.Contains(value))
            {

            }
            else
            {
                set.Add(value);
            }
        }

        public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] = value;
            }
            else
            {
                dic.Add(key, value);
            }
        }

        public static void Set<TKey, TValue>(this IDictionary<TKey, HashSet<TValue>> dic, TKey key, TValue value)
        {
            var set = dic.GetOrNew(key);
            set.Set(value);
            dic.Set(key, set);
        }

        public static void SetBit<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (dic.ContainsKey(key))
            {
                int oldValue = (int)((object)dic[key]);
                oldValue |= (int)(object)value;
                var newValue = (TValue)(object)oldValue;
                dic[key] = newValue;
            }
            else
            {
                dic.Add(key, value);
            }
        }

        public static string ReadStringNullTerminated(this BinaryReader br)
        {
            List<byte> bytes = new List<byte>();
            //try
            //{
            while (true)
            {
                var b = br.ReadByte();
                if (b == 0)
                {
                    return shiftJis.GetString(bytes.ToArray());
                }
                bytes.Add(b);
            }
            //}
            //catch (EndOfStreamException)
            //{
            //    return shiftJis.GetString(bytes.ToArray());
            //}
        }

        public static string ReadStringFixedSize(this BinaryReader br, int length)
        {
            byte[] bytes = br.ReadBytes(length);
            int zeroIndex = Array.IndexOf<byte>(bytes, 0);
            if (zeroIndex == -1)
            {
                zeroIndex = bytes.Length;
            }
            return shiftJis.GetString(bytes, 0, zeroIndex);
        }

        public static void WriteStringNullTerminated(this BinaryWriter bw, string str)
        {
            byte[] bytes = shiftJis.GetBytes(str);
            bw.Write(bytes);
            bw.Write((byte)0);
        }

        public static void WriteStringFixedSize(this BinaryWriter bw, string str, int length)
        {
            byte[] bytes = new byte[length];
            shiftJis.GetBytes(str, 0, str.Length, bytes, 0);
            bw.Write(bytes);
        }

        public static TValue GetOrAddNew<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : class, new()
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            else
            {
                var newValue = new TValue();
                dictionary.Add(key, newValue);
                return newValue;
            }
        }

        public static TValue GetOrNew<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            else
            {
                return new TValue();
            }
        }

        public static T GetOrNull<T>(this IList<T> list, int index)
        {
            if (index >= list.Count)
            {
                return default(T);
            }
            else
            {
                var value = list[index];
                return value;
            }
        }

        public static T GetOrNew<T>(this IList<T> list, int index) where T : new()
        {
            if (index >= list.Count)
            {
                return new T();
            }
            else
            {
                var value = list[index];
                if (value == null)
                {
                    return new T();
                }
                else
                {
                    return value;
                }
            }
        }

        public static void SetOrAdd<T>(this IList<T> list, int index, T value)
        {
            if (index >= list.Count)
            {
                while (index > list.Count)
                {
                    list.Add(default(T));
                }
                list.Add(value);
            }
            else
            {
                list[index] = value;
            }
        }

        public static bool AllEqualTo<T>(this IEnumerable<T> sequence, T valueToCompareTo) where T : IEquatable<T>
        {
            return sequence.All(b => b.Equals(valueToCompareTo));
        }

        public static bool AnyEqualTo<T>(this IEnumerable<T> sequence, T valueToCompareTo) where T : IEquatable<T>
        {
            return sequence.Any(b => b.Equals(valueToCompareTo));
        }

        public static int Max(this IEnumerable<int> sequence, out int index)
        {
            int maxIndex = 0;
            int maxValue = sequence.FirstOrDefault();

            int i = 0;

            foreach (int current in sequence)
            {
                if (current > maxValue)
                {
                    maxIndex = i;
                    maxValue = current;
                }
                i++;
            }
            index = maxIndex;
            return maxValue;
        }

        public static int Min(this IEnumerable<int> sequence, out int index)
        {
            int minIndex = 0;
            int minValue = sequence.FirstOrDefault();

            int i = 0;

            foreach (int current in sequence)
            {
                if (current < minValue)
                {
                    minIndex = i;
                    minValue = current;
                }
                i++;
            }
            index = minIndex;
            return minValue;
        }

        public static string ToHexString(this byte[] byteArray)
        {
            StringBuilder sb = new StringBuilder(byteArray.Length * 2);
            for (int i = 0; i < byteArray.Length; i++)
            {
                sb.Append(byteArray[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
