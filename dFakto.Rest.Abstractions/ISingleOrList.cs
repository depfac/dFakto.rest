using System;
using System.Collections.Generic;
using System.Linq;

namespace dFakto.Rest.Abstractions;

public interface ISingleOrList<out T>
{
    int Count { get; } 
    IEnumerable<T> Values { get; }
    T Value { get; }
    bool SingleValued { get; }    
}

public static class SingleOrListExtensions
{
    public static IResource? FirstOrDefault<T>(
        this ISingleOrList<IResource> resources,
        Func<T,bool> filter)
    {
        return resources.Values.FirstOrDefault(x => filter(x.As<T>()));
    }
    public static IResource First<T>(
        this ISingleOrList<IResource> resources,
        Func<T,bool> filter)
    {
        return resources.Values.First(x => filter(x.As<T>()));
    }
    public static IEnumerable<IResource> Where<T>(
        this ISingleOrList<IResource> resources,
        Func<T,bool> filter)
    {
        return resources.Values.Where(x => filter(x.As<T>()));
    }
}