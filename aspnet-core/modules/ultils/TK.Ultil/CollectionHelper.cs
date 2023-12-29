using System.Collections.Generic;
using System.Linq;

namespace System
{
    public static class CollectionHelper
    {
        public static bool IsEmpty<T>(this ICollection<T> source)
        {
            return source == null || source.Count == 0;
        }

        public static bool IsNotEmpty<T>(this ICollection<T> source)
        {
            return !IsEmpty(source);
        }

        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }

        public static bool IsNotEmpty<T>(this IEnumerable<T> source)
        {
            return !IsEmpty(source);
        }

        public static bool In<T>(this T source, IEnumerable<T> enumerable)
        {
            return enumerable.Contains(source);
        }

        public static bool In(this string source, params string[] enumerable)
        {
            return enumerable.Contains(source);
        }

        public static bool NotIn<T>(this T source, IEnumerable<T> enumerable)
        {
            return !In(source, enumerable);
        }

        /// <summary>
        /// Thực hiện Add or update value cho key trong dictionary
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOrUpdateValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict == null)
            {
                return;
            }

            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }

        /// <summary>
        /// Chia danh sách thành các batch theo kích thước batch được truyền từ param
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        public static List<List<T>> GetBatches<T>(this List<T> source, int batchSize)
        {
            if (batchSize == 0)
            {
                throw new ArgumentException(nameof(batchSize));
            }

            var batchs = new List<List<T>>();
            int index = 0;
            while (true)
            {
                var batch = new List<T>();
                var arrayIdx = index * batchSize;
                if (arrayIdx >= source.Count)
                {
                    break;
                }

                var endIdx = (index + 1) * batchSize;
                for (int i = arrayIdx; i < endIdx; i++)
                {
                    if (i >= source.Count)
                    {
                        break;
                    }
                    batch.Add(source[i]);
                }
                batchs.Add(batch);
                index++;
            }

            return batchs;
        }
    }
}
