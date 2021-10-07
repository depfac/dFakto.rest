using System;
using Microsoft.AspNetCore.Mvc;

namespace dFakto.Rest.AspNetCore.Mvc
{
    public class CollectionRequest : ResourceRequest
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

        [FromQuery(Name = "filter")]
        [ModelBinder(typeof(FilterModelBinder))]
        public Filter Filter { get; set; }
    }
}