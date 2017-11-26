// <copyright file="DictionaryExtensions.cs" company="StarkBIM Inc">
// Copyright (c) StarkBIM Inc. All rights reserved.
// </copyright>

namespace RvtTestRunner.Runner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using JetBrains.Annotations;

    /// <summary>
    ///     Extension methods for dictionaries
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        ///     Adds the given value to the list of values associated with the key
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <param name="dictionary">The dictionary</param>
        /// <param name="key">The key</param>
        /// <param name="value">The value to add</param>
        public static void Add<TKey, TValue>([NotNull] this IDictionary<TKey, List<TValue>> dictionary, [NotNull] TKey key, [NotNull] TValue value) =>
            dictionary.GetOrAdd(key).Add(value);

        /// <summary>
        ///     Checks whether the value is contained in the list of values associated with the key
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <param name="dictionary">The dictionary</param>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        /// <param name="valueComparer">The equality comparer</param>
        /// <returns>True if the value is contained in the list of values associated with the key, otherwise false</returns>
        public static bool Contains<TKey, TValue>(
            [NotNull] this IDictionary<TKey, List<TValue>> dictionary,
            [NotNull] TKey key,
            [NotNull] TValue value,
            [NotNull] IEqualityComparer<TValue> valueComparer) => dictionary.TryGetValue(key, out List<TValue> values) && values.Contains(value, valueComparer);

        /// <summary>
        ///     Adds the given key and creates a new instance of the given value type using its default constructor
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <param name="dictionary">The dictionary</param>
        /// <param name="key">The key</param>
        /// <returns>The created value instance</returns>
        [NotNull]
        public static TValue GetOrAdd<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary, [NotNull] TKey key)
            where TValue : new() => dictionary.GetOrAdd(key, () => new TValue());

        /// <summary>
        ///     Gets the value associated with the given key, or adds a new key-value pair to the dictionary with the value
        ///     determined by the creation function
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <param name="dictionary">The dictionary</param>
        /// <param name="key">The key</param>
        /// <param name="newValue">A creation function for the new value</param>
        /// <returns>The value associated with the key, or the newly added value</returns>
        [NotNull]
        public static TValue GetOrAdd<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary, [NotNull] TKey key, [NotNull] Func<TValue> newValue)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (dictionary.TryGetValue(key, out TValue result))
            {
                return result;
            }

            result = newValue();
            dictionary[key] = result;

            return result;
        }

        /// <summary>
        ///     Creates a dictionary with the given key selector and the default value selector
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <param name="values">The values</param>
        /// <param name="keySelector">The key selector</param>
        /// <param name="comparer">The equality comparer, optionally</param>
        /// <returns>The created dictionary</returns>
        [NotNull]
        public static Dictionary<TKey, TValue> ToDictionaryIgnoringDuplicateKeys<TKey, TValue>(
            [NotNull] [ItemNotNull] this IEnumerable<TValue> values,
            [NotNull] Func<TValue, TKey> keySelector,
            [CanBeNull] IEqualityComparer<TKey> comparer = null)
            => ToDictionaryIgnoringDuplicateKeys(values, keySelector, x => x, comparer);

        /// <summary>
        ///     Creates a dictionary with the given key selector and the given value selector
        /// </summary>
        /// <typeparam name="TInput">The type of the input values</typeparam>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <param name="inputValues">The input values</param>
        /// <param name="keySelector">The key selector</param>
        /// <param name="valueSelector">The value selector</param>
        /// <param name="comparer">The equality comparer, optionally</param>
        /// <returns>The created dictionary</returns>
        [NotNull]
        public static Dictionary<TKey, TValue> ToDictionaryIgnoringDuplicateKeys<TInput, TKey, TValue>(
            [NotNull] [ItemNotNull] this IEnumerable<TInput> inputValues,
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