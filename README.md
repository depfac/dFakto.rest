# dFakto.Rest

According to Roy Fielding, [you may call your API a REST API only if you make use of hypertext](https://roy.gbiv.com/untangled/2008/rest-apis-must-be-hypertext-driven)

The dFakto.Rest project contains components to ease the creation of REST API following the best practices described in the 
[HAL](https://en.wikipedia.org/wiki/Hypertext_Application_Language) specification.

Quick example to generate the Resource

```c#
 var builder = new ResourceBuilder();
 
 var author = builder.Create("http://example.com/api/users/12345")
    .Add("name","Marcel Proust");
 
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
         "name" : "Marcel Proust"
      }
   },
   "title": "In Search of Lost Time"
}
```

