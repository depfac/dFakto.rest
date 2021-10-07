using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using dFakto.Rest.Abstractions;

namespace dFakto.Rest.System.Text.Json
{
    internal class ResourceSerializer : IResourceSerializer
    {
        private readonly JsonSerializerOptions _jsonSerializerSettings;

        public ResourceSerializer(JsonSerializerOptions jsonSerializerSettings)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        public Task<string> Serialize(IResource resource)
        {
            return Task.FromResult<string>(JsonSerializer.Serialize(resource, _jsonSerializerSettings));
        }

        public async Task Serialize(Stream stream, IResource resource)
        {
            await JsonSerializer.SerializeAsync(stream, resource, _jsonSerializerSettings);
        }

        public async Task<IResource> Deserialize(Stream stream)
        {
            return await JsonSerializer.DeserializeAsync<IResource>(stream, _jsonSerializerSettings);
        }

        public Task<IResource> Deserialize(string json)
        {
            return Task.FromResult(JsonSerializer.Deserialize<IResource>(json, _jsonSerializerSettings));
        }
    }
}