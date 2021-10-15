using System;
using Microsoft.AspNetCore.Mvc;

namespace dFakto.Rest.AspNetCore.Mvc.Requests
{
    /// <summary>
    /// Base Request while Requesting collection of Resources
    /// </summary>
    public class ResourceCollectionRequest : ResourceRequest
    {
        /// <summary>
        /// Comma separated list of fields to sort. If field name starts with a '-', the sort must be descending
        /// </summary>
        [FromQuery(Name = "sort")]
        public string[] Sort { get; set; } = Array.Empty<string>();
        
        /// <summary>
        /// Page document index
        /// </summary>
        [FromQuery(Name = "index")]
        public int? Index { get; set; }

        /// <summary>
        /// Maximum number of resources to return in the result
        /// </summary>
        [FromQuery(Name = "limit")]
        public int? Limit { get; set; }

        /// <summary>
        /// Simple Filter that needs to be applied on the collection items.
        /// Format is [PropertyName] [Operator] [Value]. For a complete list of available operators, see <see cref="Filter"/>
        /// <example>Name = Bob</example>
        /// <example>BirthDate gt 1990-10-01</example>
        /// </summary>
        [FromQuery(Name = "filter")]
        [ModelBinder(typeof(FilterModelBinder))]
        public Filter Filter { get; set; }
    }
}