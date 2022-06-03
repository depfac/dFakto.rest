using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using dFakto.Rest.Abstractions;

namespace dFakto.Rest.System.Text.Json;

internal class ResourceSerializer : IResourceSerializer
{
    private readonly ResourceSerializerOptions _resourceSerializerOptions;

    public ResourceSerializer(ResourceSerializerOptions resourceSerializerOptions)
    {
        _resourceSerializerOptions = resourceSerializerOptions;
    }

    public Task<string> Serialize(IResource resource)
    {
        return Task.FromResult(JsonSerializer.Serialize(resource, _resourceSerializerOptions.JsonSerializerOptions));
    }

    public async Task Serialize(Stream stream, IResource resource)
    {
        await JsonSerializer.SerializeAsync(stream, resource, _resourceSerializerOptions.JsonSerializerOptions);
    }

    public async Task<IResource?> Deserialize(Stream stream)
    {
        return await JsonSerializer.DeserializeAsync<IResource>(stream, _resourceSerializerOptions.JsonSerializerOptions);
    }

    public Task<IResource?> Deserialize(string json)
    {
        return Task.FromResult(JsonSerializer.Deserialize<IResource>(json, _resourceSerializerOptions.JsonSerializerOptions));
    }
}