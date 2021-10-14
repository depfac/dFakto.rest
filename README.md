# dFakto.Rest

According to Roy Fielding, [you may call your API a REST API only if you make use of hypertext](https://roy.gbiv.com/untangled/2008/rest-apis-must-be-hypertext-driven)

The dFakto.Rest project contains components to ease the creation of REST API following the best practices described in the 
[Hypertext Application Language](https://en.wikipedia.org/wiki/Hypertext_Application_Language) specification and in books like ["Rest In Practice"](https://www.amazon.com/gp/product/0596805829?ie=UTF8&tag=martinfowlerc-20&linkCode=as2&camp=1789&creative=9325&creativeASIN=0596805829) 
and ["REST API Design Cookbook"](https://www.amazon.com/REST-Design-Rulebook-Mark-Masse/dp/1449310508/).

The project is composed of 3 components
* dFakto.Rest.Abstractions project contains base classes and interfaces
* dFakto.Rest.System.text.Json an implementation based on System.Text.Json
* dFakto.Rest.AspNetCore.Mvc contains component to ease integration of dFakto.Rest into ASPNET Core projects.

## Quick example to crete a Resource :

```c#

using dFakto.Rest;
using dFakto.Rest.System.text.Json;

var factory = new ResourceFactory(options);

var author = factory.Create(new Uri("http://example.com/api/authors/12345"))
     .Add(new
     {
         Name = "Marcel Proust",
         BirthDate = new DateTime(1871, 7, 10)
     });

var result = factory.Create(new Uri("http://example.com/api/books/in-search-of-lost-time"))
     .Add(new {Title = "In Search of Lost Time"})
     .AddEmbedded("author",author);

var resourceJsonSerializer = _factory.CreateSerializer();
var json = await resourceJsonSerializer.Serialize(result);
```
Will return the following Json
```json
{
   "_links": {
     "self": {
       "href": "http://example.com/api/book/in-search-of-lost-time"
     }
   },
   "_embedded": {
      "author": {
         "_links": {
           "self": {
             "href": "http://example.com/api/users/12345"
           }
         },
         "name" : "Marcel Proust",
         "birthdate": "1871-07-10T00:00:00.00"
      }
   },
   "title": "In Search of Lost Time"
}
```

## How to integrate into ASP.NET Core project


### First add the reference to your project
```
Install-Package dFakto.Rest.Asbtractions
Install-Package dFakto.Rest.System.Text.Json
Install-Package dFakto.Rest.AspNetCore.Mvc
```

### Update your Startup.cs file

```c#
public void ConfigureServices(IServiceCollection services)
{
 ...
    services.AddRest(new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    });
    
    // Register Formatter to use application/hal+json MediaType
    services.AddControllers(x =>
    {
        x.InputFormatters.Add(new ResourceInputFormatter());
        x.OutputFormatters.Add(new ResourceOutputFormatter());
    });
...
}
```

#### Example of Controller

```c#
class  MyController : Controller
{
    private IResourceFactory _factory;
    
    // Inject Resource Factory 
    public MyController(IResourceFactory resourceFactory){
        _factory = resourceFactory;
    }
    
    [HttpGet("/{id}",Name = "getbyid")]
    public ActionResult<IResource> Get(int id, [FromQuery] ResourceRequest request)
    {
        var domainEntity = GetDomainEntity();
        
        var result = _resourceFactory.Create(Url.LinkUri("getbyid", new {id = sampleValue.Id}))
            .AddLink("somelink", new Link(Url.LinkUri("getallvalues")))
            .Add(sampleValue);
         
        return Ok(result);
    }
}
```
For more information, look at the sample project

## Expand Middleware
A special middleware can be integrated to load linked ressource as embedded automtically based on the "expand" parameter

To enable this middleware, you have to update your Startup.cs file

### Register and configure the Middleware in Dependency Injection
```c#
public void ConfigureServices(IServiceCollection services)
{
 ...
    services.AddExpandMiddleware(o => o.RequestTimeout = 10);
 ...
}
```
### Insert the Middleware
```c#
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
...
    app.UseExpandMiddleware(); // Register Expand Middleware before UseMvc() call.
    
    app.UseEndpoints(endpoints => {
        endpoints.MapControllers();
    });
...
}

```

Once the Middleware integrated, you can pass the name of the link you want to retrieve
as embedded resource using the expand query string parameter

Example: 

```json
GET http://myapi/resource

{
   "_links": {
     "self": {"href": "http://myapi/resource"},
     "other": {"href":  "http://myapi/other/2323"}
   },
   "field1": "1"
}
```
Adding the "expand" query string parameter and specify the link we want to expand
```json
GET http://myapi/resource?expand=other

{
   "_links": {
     "self": {"href": "http://myapi/resource"},
     "other": {"href":  "http://myapi/other/2323"}
   },
   "_embedded": {
      "other": {
        "_links": {
           "self": {"href": "http://myapi/other/2323"}
         },
         "field1":4,
         "field2":10,
         "field3":"hello",
      }
  },
  "field1": "1"
}
```
The returned resource will contains the embedded without changing anything in the Controller.

It is possible to specify the name of an embedded resource link by using the format "embedded.link" as value for the expand parameter.

As the middleware retrieve the resources using HTTP GET calls, it may be more efficient to let the controller
retrieve the resource if it can be retrieve locally (in the same controller or in another Controller). If the controller process the "expand" parameter
and add the embedded resource, the middleware will not process it again.

## Delimited values parameters
ASP.NET Core MVC does not support delimited values for query string parameters because it is not standard.

For example, the uri http://someuri/api?param=val1,val2,val3 cannot be mapped as a string[] in your request object.
(To do so, you have to repeat the attribute name "?param=val1&param=val2...")

To support the delimited values parameters, you can register the following ProviderFactory

```c#
services.AddMvc(options => options.ValueProviderFactories.AddDelimitedValueProviderFactory(','))
```
