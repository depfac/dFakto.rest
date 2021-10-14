using System.IO;
using System.Threading.Tasks;

namespace dFakto.Rest.Abstractions
{
    /// <summary>
    /// Json Resource Serializer
    /// </summary>
    public interface IResourceSerializer
    {
        /// <summary>
        /// Deserialize Json into a IResource
        /// </summary>
        /// <param name="json">application/hal+json Json document</param>
        /// <returns></returns>
        Task<IResource> Deserialize(string json);
        
        /// <summary>
        /// Deserialize Json Stream into a IResource
        /// </summary>
        /// <param name="stream">Stream containing application/hal+json Json document</param>
        /// <returns></returns>
        Task<IResource> Deserialize(Stream stream);
        
        /// <summary>
        /// Serialize an IResource into a application/hal+json Json document
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <returns>application/hal+json Json document</returns>
        Task<string> Serialize(IResource resource);

        /// <summary>
        /// Serialize an IResource into a application/hal+json Json document and write it into a Stream.
        /// </summary>
        /// <param name="stream">The Output Stream</param>
        /// <param name="resource">The resource</param>
        /// <returns></returns>
        Task Serialize(Stream stream, IResource resource);
    }
}