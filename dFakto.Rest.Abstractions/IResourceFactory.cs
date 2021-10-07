using System;

namespace dFakto.Rest.Abstractions
{
    public interface IResourceFactory
    {
        IResource Create(Uri self);
        IResourceSerializer CreateSerializer();
    }
}