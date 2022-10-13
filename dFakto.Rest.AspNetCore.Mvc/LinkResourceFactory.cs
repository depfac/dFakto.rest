using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace dFakto.Rest.AspNetCore.Mvc;

internal class LinkResourceFactory : ILinkResourceFactory
{
    private readonly LinkGenerator _linkGenerator;
    private readonly HttpContext _httpContext;

    public LinkResourceFactory(LinkGenerator linkGenerator, IHttpContextAccessor accessor)
    {
        _linkGenerator = linkGenerator;
        _httpContext = accessor.HttpContext ?? throw new Exception("HttpContextAccessor not accessible in DI");
    }

    public Uri GetUriByName(string name, object? parameters = null)
    {
        return new Uri(_linkGenerator.GetUriByName(_httpContext, name, parameters) ?? throw new Exception("Invalid Url"));
    }
}