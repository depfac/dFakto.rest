using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using dFakto.Rest.Abstractions;

namespace dFakto.Rest.System.Text.Json
{
    internal class ResourceSerializer : IResourceSerializer
    {
        private readonly ResourceSerializerOptions _jsonSerializerSettings;

        public ResourceSerializer(ResourceSerializerOptions jsonSerializerSettings)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        public Task<string> Serialize(IResource resource)
        {
            return Task.FromResult<string>(JsonSerializer.Serialize(resource, _jsonSerializerSettings.JsonSerializerOptions));
        }

        public async Task Serialize(Stream stream, IResource resource)
        {
            await JsonSerializer.SerializeAsync(stream, resource, _jsonSerializerSettings.JsonSerializerOptions);
        }

        public async Task<IResource?> Deserialize(Stream stream)
        {
            return await JsonSerializer.DeserializeAsync<IResource>(stream, _jsonSerializerSettings.JsonSerializerOptions);
        }

        public Task<IResource?> Deserialize(string json)
        {
            return Task.FromResult(JsonSerializer.Deserialize<IResource>(json, _jsonSerializerSettings.JsonSerializerOptions));
        }
    }
}