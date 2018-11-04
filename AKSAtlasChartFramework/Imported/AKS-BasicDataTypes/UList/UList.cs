using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji
{
    [Serializable]
    public abstract class UList
    {

    }

    [Serializable]
    public abstract class UList<T> : UList, IList<T>
    {
        [SerializeField] List<T> elements;

        public T this[int index]
        {
            get
            {
                return elements[index];
            }
            set
            {
                elements[index] = value;
            }
        }

        public int Count { get { return elements.Count; } }

        public bool IsReadOnly { get { return false; } }

        public void Add(T item)
        {
            elements.Add(item);
        }

        public void Clear()
        {
            elements.Clear();
        }

        public bool Contains(T item)
        {
            return elements.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            elements.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return elements.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            elements.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return elements.Remove(item);
        }

        public void RemoveAt(int index)
        {
            elements.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return elements.GetEnumerator();
        }
    }
}