namespace dFakto.Rest.AspNetCore.Mvc
{
    public class CollectionRequest : ResourceRequest
    {
        /// <summary>
        /// Comma separated list of fields to sort. If field name starts with a '-', the sort must be descending
        /// </summary>
        public string[] Sort { get; set; } = new string[0];
        
        /// <summary>
        /// Page document index
        /// </summary>
        public int Index { get; set; } = 0;

        /// <summary>
        /// Maximum number of resources to return in the result
        /// </summary>
        public int Limit { get; set; } = 10;
    }
}