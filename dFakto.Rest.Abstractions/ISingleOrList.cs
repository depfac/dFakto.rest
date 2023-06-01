using System.Collections.Generic;

namespace dFakto.Rest.Abstractions;

public interface ISingleOrList<out T>
{
    int Count { get; } 
    IEnumerable<T> Values { get; }
    T Value { get; }
    bool SingleValued { get; }    
}
