// <copyright file="DictionaryExtensions.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    public static class DictionaryExtensions
    {
        public static void Add<TKey, TValue>([NotNull] this IDictionary<TKey, List<TValue>> dictionary, [NotNull] TKey key, [NotNull] TValue value)
        {
            dictionary.GetOrAdd(key).Add(value);
        }

        public static bool Contains<TKey, TValue>([NotNull] this IDictionary<TKey, List<TValue>> dictionary, [NotNull] TKey key, [NotNull] TValue value, [NotNull] IEqualityComparer<TValue> valueComparer)
        {
            return dictionary.TryGetValue(key, out List<TValue> values) && values.Contains(value, valueComparer);
        }

        [NotNull]
        public static TValue GetOrAdd<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary, [NotNull] TKey key)
            where TValue : new()
        {
            return dictionary.GetOrAdd(key, () => new TValue());
        }

        [NotNull]
        public static TValue GetOrAdd<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary, [NotNull] TKey key, [NotNull] Func<TValue> newValue)
        {
            if (dictionary.TryGetValue(key, out TValue result))
            {
                return result;
            }

            result = newValue();
            dictionary[key] = result;

            return result;
        }

        [NotNull]
        public static Dictionary<TKey, TValue> ToDictionaryIgnoringDuplicateKeys<TKey, TValue>(
            [NotNull][ItemNotNull] this IEnumerable<TValue> values,
            [NotNull] Func<TValue, TKey> keySelector,
            [CanBeNull] IEqualityComparer<TKey> comparer = null)
            => ToDictionaryIgnoringDuplicateKeys(values, keySelector, x => x, comparer);

        [NotNull]
        public static Dictionary<TKey, TValue> ToDictionaryIgnoringDuplicateKeys<TInput, TKey, TValue>(
            [NotNull][ItemNotNull] this IEnumerable<TInput> inputValues,
            [NotNull] Func<TInput, TKey> keySelector,
            [NotNull] Func<TInput, TValue> valueSelector,
            [CanBeNull] IEqualityComparer<TKey> comparer = null)
        {
            var result = new Dictionary<TKey, TValue>(comparer);

            foreach (var inputValue in inputValues)
            {
                var key = keySelector(inputValue);
                if (!result.ContainsKey(key))
                {
                    result.Add(key, valueSelector(inputValue));
                }
            }

            return result;
        }
    }
}