using System.Text.Json;
using System.Text.Json.Serialization;
using dFakto.Rest.AspNetCore.Mvc;
using dFakto.Rest.SampleApi.Tools;
using dFakto.Rest.System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace dFakto.Rest.SampleApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            services.AddCors(options =>
            {
                options.AddPolicy("EnableCORS",
                    builder =>
                    {
                        builder.WithOrigins("http://example.com", "http://www.contoso.com");
                        builder.AllowAnyHeader();
                        builder.AllowCredentials();
                        builder.WithMethods("POST");
                    });
            });
            
            services.AddControllers(x =>
            {
                x.InputFormatters.Add(new ResourceInputFormatter());
                x.OutputFormatters.Add(new ResourceOutputFormatter());
            });
            
            services.AddSingleton<SampleRepository>();
            
            services.AddExpandMiddleware(o => o.RequestTimeout = 10);
            
            services.AddRest(new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseRouting();
            
            app.UseCors("EnableCORS");

            app.UseExpandMiddleware();
            
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}