using System.Linq;

namespace dFakto.Rest.AspNetCore.Mvc
{
    public class ResourceRequest
    {
        /// <summary>
        ///  When true, the requested resource will be embedded into another resource, the api can therefore decide to not return the complete list of fields
        /// </summary>
        public bool Embedding { get; set; } = false;
        
        /// <summary>
        /// List of links that need to be expanded
        /// </summary>
        public string[] Expand { get; set; } = new string[0];
        
        /// <summary>
        /// List of fields that need to be returned by api.
        /// null value means all fields.
        /// </summary>
        public string[] Fields { get; set; } = null;
    }
}