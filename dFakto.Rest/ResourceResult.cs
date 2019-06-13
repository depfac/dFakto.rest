using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace dFakto.Rest
{
    public class ResourceResult : JsonResult
    {
        public ResourceResult(Resource resource)
        :base(resource.Value)
        {
        }
    }
}