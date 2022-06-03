using System;
using dFakto.Rest.Abstractions;

namespace dFakto.Rest.System.Text.Json;

public class ResourceFactory : IResourceFactory
{
    private readonly ResourceSerializerOptions _jsonSerializerOptions;

    public ResourceFactory(ResourceSerializerOptions jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
        _jsonSerializerOptions.JsonSerializerOptions.Converters.Add(new ResourceConverterFactory());
    }
        
    public IResource Create(Uri self)
    {
        var r = new Resource(_jsonSerializerOptions.JsonSerializerOptions);
        r.AddLink(Constants.Self, new Link(self));
        return r;
    }

    public IResourceSerializer CreateSerializer()
    {
        return new ResourceSerializer(_jsonSerializerOptions);
    }
}