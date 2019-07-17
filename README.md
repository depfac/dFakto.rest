# dFakto.Rest

According to Roy Fielding, [you may call your API a REST API only if you make use of hypertext](https://roy.gbiv.com/untangled/2008/rest-apis-must-be-hypertext-driven)

The dFakto.Rest project contains components to ease the creation of REST API following the best practices described in the 
[HAL](https://en.wikipedia.org/wiki/Hypertext_Application_Language) specification and in books like ["Rest In Practice"](https://www.amazon.com/gp/product/0596805829?ie=UTF8&tag=martinfowlerc-20&linkCode=as2&camp=1789&creative=9325&creativeASIN=0596805829) 
and ["REST API Design Cookbook"](https://www.amazon.com/REST-Design-Rulebook-Mark-Masse/dp/1449310508/).

The dFakto.Rest projects contains base Resource classes management, the dFakto.Rest.AspNetCore.Mvc contains component to integrate
dFakto.Rest into ASPNET Core MVC projects.

## Quick example or Resource :

```c#
 var builder = new ResourceBuilder();
 
 var author = builder.Create("http://example.com/api/authors/12345")
                     .Add("name","Marcel Proust")
                     .Add("birthdate",new DateTime(1871,7,10));
 
 var r = builder.Create("http://example.com/api/book/in-search-of-lost-time")
                .Add("title","In Search of Lost Time")
                .AddEmbeddedResource("author",author);
 var json = JObject.FromObject(r);
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

## How to integrate into ASP.NET Core MV project


### First add the reference to your project
```
Install-Package dFakto.Rest -Version 0.2.0
Install-Package dFakto.Rest.AspNetCore.Mvc -Version 0.2.0
```

### Update your Startup.cs file

```c#
public void ConfigureServices(IServiceCollection services)
{
...
    services.AddRest(); // Register dFakto.Rest services
...
}
```

### Make your controllers inherit from RestController

the RestController gives you access to some helper methods to creates Resources
```c#
Resource CreateResource(string uri)
Resource CreateResourceCollection(string uri, 
                                  CollectionRequest request,
                                  long? total = null)
```
and to retrieve absolute Uri easily
```c#
string GetUriFromRoute(string routeName, object parameters = null)
string GetCurrentUri()
```

#### Example of Controller

```c#
class  MyController : RestController

[HttpGet("{id}",Name = "getbyid")]
public Resource Get(int id, [FromQuery] ResourceRequest request)
{
    return CreateResource(GetUriFromRoute("getbyid",new {id=sampleValue.Id}))
                    .AddLink("parent",GetUriFromRoute("getallvalues"))
                    .Merge(GetValue(id),onlyFields);
}
```
For more information, look at the sample project

## Delimited values parameters
ASP.NET Core MVC does not support delimited values for query string parameters because it is not standard.

For example, the uri http://someuri/api?param=val1,val2,val3 cannot be mapped as a string[] in your request object.
(To do so, you have to repeat the attribute name "?param=val1&param=val2...")

To support the delimited values parameters, you can register the following ProviderFactory

```c#
services.AddMvc(options => options.ValueProviderFactories.AddDelimitedValueProviderFactory(','))
```


## Autoexpand middleware

A special middleware can be integrated to load linked ressource automtically based on the "expand" parameter

To enable this middleware, you have to update your Startup.cs file

```c#
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
...
    app.UseMiddleware<ExpandMiddleware>(); // Register Expand Middleware before UseMvc() call.
    app.UseMvc();
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

As the middleware retrieve the resources using HTTP GET calls, it may be more efficient to let the controller
retrieve the resource if it can be retrieve locally (in the same controller or in another Controller). If the controller process the "expand" parameter
and add the embedded resource, the middleware will not process it again.
