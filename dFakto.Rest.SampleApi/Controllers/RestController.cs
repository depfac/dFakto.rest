using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace dFakto.Rest.SampleApi.Controllers
{
    public class RestController : Controller
    {
        public Resource CreateResource(string uri)
        {
            return HttpContext.RequestServices.GetService<ResourceBuilder>().Create().Self(uri);
        }
        
        public Resource CreateResourceCollection(string uri, CollectionRequest request, long total)
        {
            var b = HttpContext.RequestServices.GetService<ResourceBuilder>();
            return b.Create()
                .Self(uri)
                .Add(request)
                .Add("total", total);
        }
        
        public string GetUriFromRoute(string routeName, object parameters)
        {
            var r = Url.ActionContext.HttpContext.Request;
            return $"{r.Scheme}://{r.Host.ToString()}{Url.RouteUrl(routeName, parameters).ToLower()}";
        }

        public string GetCurrentRouteUri()
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