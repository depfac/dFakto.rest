using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using dFakto.Rest.Abstractions;
using Microsoft.Extensions.Options;

namespace dFakto.Rest.AspNetCore.Mvc;

internal class ResourceAccessor : IResourceAccessor
{
    public const string ResourceAccessorHttpClientName = "ResourceAccessor";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IResourceFactory _resourceFactory;
    private readonly IOptions<HypermediaApplicationLanguageExpandMiddlewareOptions> _options;

    public ResourceAccessor(
        IHttpClientFactory httpClientFactory,
        IResourceFactory resourceFactory,
        IOptions<HypermediaApplicationLanguageExpandMiddlewareOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _resourceFactory = resourceFactory;
        _options = options;
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
        using (var client = _httpClientFactory.CreateClient(ResourceAccessorHttpClientName))
        {
            client.Timeout = TimeSpan.FromSeconds(_options.Value.RequestTimeout);
            
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HypertextApplicationLanguageMediaType));

                using (var response = await client.SendAsync(request, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    await using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken))
                    {
                        return await _resourceFactory.CreateSerializer().Deserialize(stream);
                    }
                }
            }
        }
    }
}