using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace dFakto.Rest
{
    public class ResourceUriBuilder
    {
        private readonly IUrlHelper _helper;

        public ResourceUriBuilder(IUrlHelper urlHelper)
        {
            _helper = urlHelper;
        }
        
        public string GetUriFromRoute(string routeName, object parameters)
        {
            var r = _helper.ActionContext.HttpContext.Request;
            return $"{r.Scheme}://{r.Host.ToString()}{_helper.RouteUrl(routeName, parameters).ToLower()}";
        }

        public string GetCurrentRouteUri()
        {
            var request = _helper.ActionContext.HttpContext.Request;
                
            string host = request.Host.Value;
            string pathBase = request.PathBase.Value;
            string path = request.Path.Value;
            
            var sb = new StringBuilder(request.Scheme.Length + "://".Length + host.Length + pathBase.Length + path.Length);
            sb.Append(request.Scheme);
            sb.Append("://");
            sb.Append(host);
            sb.Append(pathBase);
            sb.Append(path);

            return sb.ToString();
        }
    }
}