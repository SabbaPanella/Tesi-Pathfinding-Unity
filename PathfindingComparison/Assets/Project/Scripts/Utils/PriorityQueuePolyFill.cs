#if !NET6_0 && !NETSTANDARD2_1 && !UNITY_2022_2_OR_NEWER
// Polyfill per Unity < 2022 che non espone System.Collections.Generic.PriorityQueue
using System;
using System.Collections.Generic;

namespace System.Collections.Generic
{
    /// Mini-implementazione (binary-heap) di PriorityQueue<TElement,TPriority>
    /// sufficiente per Dijkstra.  Complessità: Enqueue O(log n), Dequeue O(log n).
    public class PriorityQueue<TElement, TPriority>
    {
        readonly List<(TElement elem, TPriority pri)> _data = new();
        readonly IComparer<TPriority> _cmp;

        public PriorityQueue(IComparer<TPriority> cmp = null)
        {
            _cmp = cmp ?? Comparer<TPriority>.Default;
        }

        public int Count => _data.Count;

        public void Enqueue(TElement element, TPriority priority)
        {
            _data.Add((element, priority));
            HeapifyUp(_data.Count - 1);
        }

        public bool TryDequeue(out TElement element, out TPriority priority)
        {
            if (_data.Count == 0)
            {
                element = default;
                priority = default;
                return false;
            }

            (element, priority) = _data[0];

            var last = _data[^1];
            _data.RemoveAt(_data.Count - 1);

            if (_data.Count > 0)
            {
                _data[0] = last;
                HeapifyDown(0);
            }
            return true;
        }

        /*───────────── heap helpers ─────────────*/
        void HeapifyUp(int i)
        {
            while (i > 0)
            {
                int p = (i - 1) >> 1;
                if (_cmp.Compare(_data[i].pri, _data[p].pri) >= 0) break;
                (_data[i], _data[p]) = (_data[p], _data[i]);
                i = p;
            }
        }

        void HeapifyDown(int i)
        {
            while (true)
            {
                int l = (i << 1) + 1;
                if (l >= _data.Count) break;
                int r = l + 1;
                int s = (r < _data.Count && _cmp.Compare(_data[r].pri, _data[l].pri) < 0) ? r : l;
                if (_cmp.Compare(_data[s].pri, _data[i].pri) >= 0) break;
                (_data[i], _data[s]) = (_data[s], _data[i]);
                i = s;
            }
        }
    }
}
#endif