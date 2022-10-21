using System.Collections.Generic;

namespace dFakto.Rest.Abstractions;

public interface ISingleOrList<T>
{
    public int Count { get; }
    public IEnumerable<T> Values { get; }
    public T Value { get; }
    bool SingleValued { get; }    
}