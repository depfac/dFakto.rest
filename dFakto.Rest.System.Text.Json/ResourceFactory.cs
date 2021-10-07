using System;
using System.IO;
using System.Text.Json;
using dFakto.Rest.Abstractions;

namespace dFakto.Rest.System.Text.Json
{
    public class ResourceFactory : IResourceFactory
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ResourceFactory(JsonSerializerOptions jsonSerializerOptions = null)
        {
            _jsonSerializerOptions = jsonSerializerOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.General);
            _jsonSerializerOptions.Converters.Add(new ResourceConverterFactory());
        }
        
        public IResource Create(Uri self)
        {
            var r = new Resource(_jsonSerializerOptions);
            r.AddLink(Constants.Self, new Link(self));
            return r;
        }

        public IResourceSerializer CreateSerializer()
        {
            return new ResourceSerializer(_jsonSerializerOptions);
        }
    }
}