using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;

namespace dFakto.Rest.Abstractions
{
    public interface IResource
    {
        Uri Self
        {
            get => Links.ContainsKey(Constants.Self) ? Links[Constants.Self].FirstOrDefault()?.Href : null;
        }

        IReadOnlyDictionary<string, IReadOnlyList<Link>> Links { get; }
        IReadOnlyDictionary<string,  IReadOnlyList<IResource>> Embedded  { get; }
        IResource AddLink(string name, params Link[] links);
        IResource AddLink(string name, IEnumerable<Link> links);
        IResource AddEmbedded(string name,params IResource[] embedded);
        IResource AddEmbedded(string name, IEnumerable<IResource> embedded);
        IResource Add<T>(T values, Func<T,object> onlyFields = null)  where T : class;
        T As<T>();
        T Bind<T>(T type);
    }
}