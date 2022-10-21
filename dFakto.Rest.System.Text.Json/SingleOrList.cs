using System;
using System.Collections.Generic;
using System.Linq;
using dFakto.Rest.Abstractions;

namespace dFakto.Rest.System.Text.Json;

public class SingleOrList<T> : ISingleOrList<T>
{
    private readonly List<T> _values = new List<T>();

    public SingleOrList(IEnumerable<T> items)
    {
        SingleValued = false;
        _values.AddRange(items.Where(x => x != null));
    }

    public SingleOrList(T single)
    {
        SingleValued = true;
        _values.Add(single);
    }

    public void AddRange(IEnumerable<T> items)
    {
        if (SingleValued)
            throw new ArgumentException("SingleOrList is Single Valued");
        _values.AddRange(items);
    }

    public int Count => _values.Count;

    public IEnumerable<T> Values => _values;

    public T Value => _values.First();

    public bool SingleValued { get; }
}