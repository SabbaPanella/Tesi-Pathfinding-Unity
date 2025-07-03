using System;
using System.Collections.Generic;

/// <summary>Min-heap basato su array dinamico. O(log n) push / pop.</summary>
public class MinPriorityQueue<T> where T : IComparable<T>
{
    readonly List<T> _heap = new();

    public int Count => _heap.Count;

    public void Enqueue(T item)
    {
        _heap.Add(item);
        SiftUp(_heap.Count - 1);
    }

    public T Dequeue()
    {
        if (_heap.Count == 0) throw new InvalidOperationException("PQ empty");
        T root = _heap[0];
        int last = _heap.Count - 1;
        _heap[0] = _heap[last];
        _heap.RemoveAt(last);
        SiftDown(0);
        return root;
    }

    public T Peek() => _heap[0];

    void SiftUp(int i)
    {
        while (i > 0)
        {
            int p = (i - 1) >> 1;
            if (_heap[i].CompareTo(_heap[p]) >= 0) break;
            (_heap[i], _heap[p]) = (_heap[p], _heap[i]);
            i = p;
        }
    }

    void SiftDown(int i)
    {
        int n = _heap.Count;
        while (true)
        {
            int l = (i << 1) + 1;
            int r = l + 1;
            if (l >= n) break;
            int smallest = (r < n && _heap[r].CompareTo(_heap[l]) < 0) ? r : l;
            if (_heap[i].CompareTo(_heap[smallest]) <= 0) break;
            (_heap[i], _heap[smallest]) = (_heap[smallest], _heap[i]);
            i = smallest;
        }
    }
}