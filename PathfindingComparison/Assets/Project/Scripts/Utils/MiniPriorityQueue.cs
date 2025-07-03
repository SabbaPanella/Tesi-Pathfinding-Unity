using System;
using System.Collections.Generic;

/// Min-heap molto semplice sufficiente per Dijkstra.
/// Non usa generic priority comparer: TPriority deve implementare IComparable.
internal class MiniPriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
{
    readonly List<(TElement elem, TPriority pri)> _data = new();

    public int Count => _data.Count;

    public void Enqueue(TElement element, TPriority priority)
    {
        _data.Add((element, priority));
        HeapUp(_data.Count - 1);
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
            HeapDown(0);
        }
        return true;
    }

    /*────────── heap internals ──────────*/
    void HeapUp(int i)
    {
        while (i > 0)
        {
            int p = (i - 1) >> 1;
            if (_data[i].pri.CompareTo(_data[p].pri) >= 0) break;
            (_data[i], _data[p]) = (_data[p], _data[i]);
            i = p;
        }
    }
    void HeapDown(int i)
    {
        while (true)
        {
            int l = (i << 1) + 1;
            if (l >= _data.Count) break;
            int r = l + 1;
            int s = (r < _data.Count && _data[r].pri.CompareTo(_data[l].pri) < 0) ? r : l;
            if (_data[s].pri.CompareTo(_data[i].pri) >= 0) break;
            (_data[i], _data[s]) = (_data[s], _data[i]);
            i = s;
        }
    }
}