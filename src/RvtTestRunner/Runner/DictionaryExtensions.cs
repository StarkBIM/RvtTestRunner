﻿// <copyright file="DictionaryExtensions.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class DictionaryExtensions
    {
        public static void Add<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
        {
            dictionary.GetOrAdd(key).Add(value);
        }

        public static bool Contains<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key, TValue value, IEqualityComparer<TValue> valueComparer)
        {
            return dictionary.TryGetValue(key, out List<TValue> values) && values.Contains(value, valueComparer);
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : new()
        {
            return dictionary.GetOrAdd(key, () => new TValue());
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> newValue)
        {
            if (dictionary.TryGetValue(key, out TValue result))
            {
                return result;
            }

            result = newValue();
            dictionary[key] = result;

            return result;
        }

        public static Dictionary<TKey, TValue> ToDictionaryIgnoringDuplicateKeys<TKey, TValue>(this IEnumerable<TValue> values,
                                                                                               Func<TValue, TKey> keySelector,
                                                                                               IEqualityComparer<TKey> comparer = null)
            => ToDictionaryIgnoringDuplicateKeys(values, keySelector, x => x, comparer);

        public static Dictionary<TKey, TValue> ToDictionaryIgnoringDuplicateKeys<TInput, TKey, TValue>(this IEnumerable<TInput> inputValues,
                                                                                                       Func<TInput, TKey> keySelector,
                                                                                                       Func<TInput, TValue> valueSelector,
                                                                                                       IEqualityComparer<TKey> comparer = null)
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