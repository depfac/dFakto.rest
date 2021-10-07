using System;
using Microsoft.AspNetCore.Mvc;

namespace dFakto.Rest.AspNetCore.Mvc
{
    public class ResourceRequest
    {
        /// <summary>
        /// List of links that need to be expanded.
        /// The format can be a link name or an embedded resource name followed by the link name (embedded.link)
        /// </summary>
        [FromQuery(Name = "expand")]
        public string[] Expand { get; set; } = Array.Empty<string>();
    }
}