/* Empty base class is used as a target for a custom property drawer. To use, RandomArray<T> must be trivially
 * specialized to eliminate the generic parameter. The property drawer will be pushed down to the subclass. */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji.AtlasGeneration
{
    [Serializable]
    public abstract class RandomArray { }

    [Serializable]
    public abstract class RandomArray<T> : RandomArray
    {
        /// <summary>
        /// Returns all pairs of the form (value, weight) in order of the values.
        /// </summary>
        public IEnumerable<KeyValuePair<T, int>> WeightedValues
        {
            get
            {
                return Enumerable.Range(0, values.Length).Select(i => new KeyValuePair<T, int>(values[i], weights[i]));
            }
        }

        public IEnumerable<T> Values { get { return values; } }
        [SerializeField] T[] values;

        public IEnumerable<int> Weights { get { return weights; } }
        [SerializeField] int[] weights;
        
        /// <summary>
        /// Fetches a randomly chosen value, with probability distribution given by the weights array.
        /// </summary>
        public T RandomValue
        {
            get
            {
                if (values.Length == 0)
                    throw new InvalidOperationException("Array is empty.");

                int targetWeight = UnityEngine.Random.Range(0, weights.Sum());
                int runningWeight = 0;
                for (int i = 0; i < weights.Length; i++)
                {
                    runningWeight += weights[i];
                    if (runningWeight > targetWeight)
                    {
                        return values[i];
                    }
                }
                throw new Exception("Bug - target weight never reached."); // should not be possible under any circumstance
            }
        }
    }
}