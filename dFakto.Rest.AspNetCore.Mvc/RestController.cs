using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace dFakto.Rest.AspNetCore.Mvc
{
    public class RestController : ControllerBase
    {
        protected Resource CreateResource(string uri)
        {
            return HttpContext.RequestServices.GetService<ResourceBuilder>().Create().Self(uri);
        }
        
        protected Resource CreateResourceCollection(string uri, CollectionRequest request, long? total = null)
        {
            var b = HttpContext.RequestServices.GetService<ResourceBuilder>();
            var r = b.Create()
                .Self(uri)
                .Add(request);
            
            if(total.HasValue)
            {
                r.Add("total", total.Value);
            }

            return r;
        }
        
        protected string GetUriFromRoute(string routeName, object parameters)
        {
            var r = Url.ActionContext.HttpContext.Request;
            return $"{r.Scheme}://{r.Host.ToString()}{Url.RouteUrl(routeName, parameters).ToLower()}";
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