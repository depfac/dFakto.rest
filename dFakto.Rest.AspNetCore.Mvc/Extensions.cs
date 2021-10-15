using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using dFakto.Rest.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace dFakto.Rest.AspNetCore.Mvc
{
    public static class Extensions
    {
        public static IServiceCollection AddExpandMiddleware(this IServiceCollection services, Action<ExpandMiddlewareOptions> optionAction = null)
        {
            ExpandMiddlewareOptions options = new ExpandMiddlewareOptions();

            optionAction?.Invoke(options);
            services.AddSingleton(options);
            
            return services;
        }

        public static IApplicationBuilder UseExpandMiddleware(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<ExpandMiddleware>();
        }
        
        public static Uri LinkUri(this IUrlHelper uri, string name, object p = null)
        {
            return new Uri(uri.Link(name, p));
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
        public static async Task<Stream> GetResourceStream(
            this HttpContext context, 
            Uri uri, 
            TimeSpan requestTimeout, 
            CancellationToken cancellationToken = new CancellationToken())
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();
            foreach (var cookie in context.Request.Cookies)
            {
                handler.CookieContainer.Add(new Cookie(cookie.Key, cookie.Value) {Domain = uri.Host});
            }

            using (HttpClient client = new HttpClient(handler))
            {
                client.Timeout = requestTimeout;
                
                HttpRequestMessage request = new HttpRequestMessage();
                request.Method = HttpMethod.Get;
                request.RequestUri = uri;
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HypertextApplicationLanguageMediaType));
                if (context.Request.Headers.TryGetValue("Authorization", out var auth))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", auth.First());
                }

                HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStreamAsync();
            }
        }
    }
}