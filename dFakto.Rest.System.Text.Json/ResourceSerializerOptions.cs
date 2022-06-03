using System.Text.Json;
using dFakto.Rest.Abstractions;

namespace dFakto.Rest.System.Text.Json;

public class ResourceSerializerOptions
{
    public ResourceSerializerOptions()
    {
        JsonSerializerOptions = new JsonSerializerOptions();
    }
        
    public JsonSerializerOptions JsonSerializerOptions { get; set; }
}