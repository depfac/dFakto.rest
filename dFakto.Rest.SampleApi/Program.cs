using dFakto.Rest.Abstractions;
using dFakto.Rest.AspNetCore.Mvc;
using dFakto.Rest.SampleApi;
using dFakto.Rest.SampleApi.ResourceFactories;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(x => 
    // To support application/hal+json Content Type
    x.AddHypermediaApplicationLanguageFormatters());
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHypermediaApplicationLanguage(x => { x.SupportedMediaTypes.Add("application/json"); });
builder.Services.AddTransient<AuthorResourceFactory>();
builder.Services.AddTransient<BookResourceFactory>();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All,
});
app.Use(async (context, next) =>
{
    if (context.Request.Headers.TryGetValue("X-Forwarded-Prefix", out var prefix))
    {
        context.Request.PathBase = prefix.Last();

        if (context.Request.Path.StartsWithSegments(context.Request.PathBase, out var path))
        {
            context.Request.Path = path;
        }
    }

    await next(context);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseHeaderPropagation();

app.MapControllers();

app.Run();