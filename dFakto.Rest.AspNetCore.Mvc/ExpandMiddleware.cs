using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace dFakto.Rest.AspNetCore.Mvc
{
    public class ExpandMiddleware
    {
        //Take up to three level of expands. Next levels will be ignored
        private readonly Regex _expandParts = new Regex(@"^(?<name>[^\.]+)\.?(?<subname>[^\.]+\.?[^\.]+\.?[^\.]+)?.*",RegexOptions.Compiled);
        
        private readonly RequestDelegate _next;
        private readonly ResourceBuilder _builder;
        private readonly ILogger<ExpandMiddleware> _logger;

        public ExpandMiddleware(RequestDelegate next, ResourceBuilder builder, ILogger<ExpandMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _builder = builder;
        }

        // IMyScopedService is injected into Invoke
        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Query.TryGetValue("expand",out var expands) || !(context.Request.ContentType?.Contains("json") ?? true))
            {
                await _next(context);
            }
            else
            {
                _logger.LogDebug("Expand parameter detected");
                var origin = context.Response.Body;

                using (var tmpBody = new MemoryStream())
                {
                    context.Response.Body = tmpBody;

                    //Call next middleware and reset stream
                    await _next(context);
                    tmpBody.Seek(0, SeekOrigin.Begin);

                    _logger.LogDebug("Loading Resource Response");
                    //Parse Json
                    var resource = await _builder.LoadAsync(tmpBody);

                    bool changed = false;
                    //Ensure it's a resource
                    if (resource.GetSelf() != null)
                    {
                        foreach (var names in expands)
                        {
                            foreach (var n in names.Split(','))
                            {
                                var reg = _expandParts.Match(n);
                                var name = reg.Groups["name"].Value;
                                var subName = reg.Groups["subname"].Value;
                                
                                if (string.IsNullOrWhiteSpace(name) || 
                                    name == Properties.Self || 
                                    !resource.ContainsLink(name) || 
                                    resource.ContainsEmbeddedResource(name))
                                {
                                    continue;
                                }
                                
                                //Load Resource

                                var url = new UriBuilder(resource.GetLink(name).Href);


                                if (string.IsNullOrWhiteSpace(url.Query))
                                {
                                    url.Query = "?Embedding=true";
                                }
                                else
                                {
                                    url.Query += "&Embedding=true";
                                }
                                
                                if (!string.IsNullOrWhiteSpace(subName))
                                {
                                    url.Query += "&expand=" + subName;
                                }

                                _logger.LogInformation($"Loading Resource at {url}");
                                
                                context.Request.Headers.TryGetValue("Authorization", out var auth);
                                var embeddedResource = await GetResourceAsync(url.Uri, context.Request.Cookies, auth);
                                if (embeddedResource != null)
                                {
                                    _logger.LogDebug("Resource retrieved, adding to response");
                                    resource.AddEmbeddedResource(name, embeddedResource);
                                    changed = true;
                                }
                            }
                        }
                    }

                    context.Response.Body = origin;
                    
                    if (changed)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await resource.WriteToAsync(ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            context.Response.ContentLength = ms.Length;
                            await ms.CopyToAsync(context.Response.Body);
                        }
                    }
                    else
                    {
                        tmpBody.Seek(0, SeekOrigin.Begin);
                        await tmpBody.CopyToAsync(context.Response.Body);
                    }
                }
            }
        }

        private async Task<Resource> GetResourceAsync(
            Uri uri, 
            IRequestCookieCollection requestCookies,
            string authorization = null)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = new CookieContainer();
                foreach (var cookie in requestCookies)
                {
                    _logger.LogDebug("Adding cookie " + cookie.Key + " and domain " + uri.Host);
                    handler.CookieContainer.Add(new Cookie(cookie.Key,cookie.Value){Domain = uri.Host});
                }
                
                using (HttpClient client = new HttpClient(handler))
                {
                    HttpRequestMessage request = new HttpRequestMessage();
                    request.Method = HttpMethod.Get;
                    request.RequestUri = uri;
                    request.Headers.Accept.Clear();
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    if (!string.IsNullOrEmpty(authorization))
                    {
                        _logger.LogDebug("Adding Authorization header");
                        request.Headers.TryAddWithoutValidation("Authorization", authorization);
                    }
                    
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        return await _builder.LoadAsync(await response.Content.ReadAsStreamAsync());
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(new EventId(), e, "Error while loading resource");
            }

            return null;
        }
    }
}