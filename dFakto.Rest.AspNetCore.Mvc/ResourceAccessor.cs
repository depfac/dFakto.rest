using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using dFakto.Rest.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace dFakto.Rest.AspNetCore.Mvc;



internal class ResourceAccessor : IResourceAccessor
{
    private readonly HttpContext _context;
    
    public ResourceAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _context = httpContextAccessor.HttpContext;
    }

    /// <summary>
    /// Retrieve a Resource at the given Uri.
    /// The current Cookies and Authentication Header will be transferred to the request 
    /// </summary>
    /// <param name="context">HttpContext</param>
    /// <param name="uri">Uri of the resource to retrieve</param>
    /// <param name="requestTimeout"></param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>The Stream with the resource</returns>
    public async Task<IResource> GetResource(
        Uri uri, 
        CancellationToken cancellationToken = new CancellationToken())
    {
        var factory = _context.RequestServices.GetService<IResourceFactory>()
                      ?? throw new ApplicationException("Unable to resolve IResourceFactory");
        var options = _context.RequestServices.GetService<IOptions<HypermediaApplicationLanguageExpandMiddlewareOptions>>()
                      ?? throw new ApplicationException("Unable to resolve HypermediaApplicationLanguageExpandMiddlewareOptions");
            
        HttpClientHandler handler = new HttpClientHandler();
        handler.CookieContainer = new CookieContainer();
        foreach (var cookie in _context.Request.Cookies)
        {
            handler.CookieContainer.Add(new Cookie(cookie.Key, cookie.Value) {Domain = uri.Host});
        }

        using (HttpClient client = new HttpClient(handler))
        {
            client.Timeout = TimeSpan.FromSeconds(options.Value.RequestTimeout);
                
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = uri;
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HypertextApplicationLanguageMediaType));
            if (_context.Request.Headers.TryGetValue("Authorization", out var auth))
            {
                request.Headers.TryAddWithoutValidation("Authorization", auth.First());
            }

            HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using (var stream = await response.Content.ReadAsStreamAsync())
            {
                return await factory.CreateSerializer().Deserialize(stream);
            }
        }
    }
}