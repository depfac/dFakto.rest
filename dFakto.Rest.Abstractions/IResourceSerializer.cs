using System.IO;
using System.Threading.Tasks;

namespace dFakto.Rest.Abstractions
{
    public interface IResourceSerializer
    {
        Task<IResource> Deserialize(string json);
        Task<IResource> Deserialize(Stream stream);
        
        Task<string> Serialize(IResource resource);
        Task Serialize(Stream stream, IResource resource);
    }
}