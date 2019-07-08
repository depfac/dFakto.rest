using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace dFakto.Rest.AspNetCore.Mvc
{
    public class RestController : ControllerBase
    {
        protected Resource CreateResource()
        {
            return HttpContext.RequestServices.GetService<ResourceBuilder>().Create();
        }
        
        protected Resource CreateResource(string uri)
        {
            return CreateResource().Self(uri);
        }
        
        protected Resource CreateResourceCollection(string uri, CollectionRequest request, long? total = null)
        {
            var r = CreateResource(uri).Merge(request);
                
            if (request.Index > 0)
            {
                int i = request.Index - request.Limit;

                r.AddLink("prev", uri + $"?index={(i < 0 ? 0 : i)}&limit={request.Limit}");
            }

            if (total == null || request.Index + request.Limit <= total)
            {
                r.AddLink("next",uri + $"?index={(request.Index+request.Limit)}&limit={request.Limit}");
            }

            r.Add("total", total);
            
            return r;
        }

        protected string GetUriFromRoute(string routeName, object parameters = null)
        {

            string routeUrl = Url.RouteUrl(routeName, parameters);
            if (string.IsNullOrWhiteSpace(routeUrl))
                return string.Empty;
            
            var r = Url.ActionContext.HttpContext.Request;
            return $"{r.Scheme}://{r.Host.ToString()}{routeUrl.ToLower()}";
        }

        protected string GetCurrentUri()
        {
            var request = Url.ActionContext.HttpContext.Request;

            var host = request.Host.Value;
            var pathBase = request.PathBase.Value;
            var path = request.Path.Value;

            var sb = new StringBuilder(request.Scheme.Length + "://".Length + host.Length + pathBase.Length +
                                       path.Length);
            sb.Append(request.Scheme);
            sb.Append("://");
            sb.Append(host);
            sb.Append(pathBase);
            sb.Append(path);

            return sb.ToString();
        }
    }
}