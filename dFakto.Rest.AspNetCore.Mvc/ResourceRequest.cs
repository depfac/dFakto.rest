using System.Linq;

namespace dFakto.Rest.AspNetCore.Mvc
{
    public class ResourceRequest
    {
        public string[] Expand { get; set; } = new string[0];
        public string[] Fields { get; set; } = new string[0];
    }
}