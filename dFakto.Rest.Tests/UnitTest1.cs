using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using dFakto.Rest.Abstractions;
using dFakto.Rest.System.Text.Json;
using Xunit;

namespace dFakto.Rest.Tests
{
     public interface IMyModel
     {
         string Test { get; set; }
         int Integer { get; set; }
     }
     public class MyModel : IMyModel
     {
         public string Test { get; set; }
         public DateTime Date { get; set; }
         public decimal Decimal { get; set; }
         public int Integer { get; set; }
         public double Double { get; set; }
         public float Float { get; set; }
         public Uri Uri { get; set; }
         
         public Uri Null { get; set; }
     }

     public class UnitTest1
     {
         private readonly IResourceFactory _factory;
         private readonly IResourceSerializer _serializer;
         public UnitTest1()
         {
             var options = new JsonSerializerOptions
             {
                 WriteIndented = true,
                 PropertyNamingPolicy = JsonNamingPolicy.CamelCase
             };
             options.Converters.Add(new JsonStringEnumConverter());
             _factory = new ResourceFactory(new ResourceSerializerOptions
             {
                 JsonSerializerOptions = options
             });
             _serializer = _factory.CreateSerializer();
         }
         
         private MyModel GetModel()
         {
             return new MyModel
             {
                 Test = "coucou", Date = DateTime.Now, Decimal = 22.33m, Uri = new Uri("http://someuri"),
                 Double = 123.232, Float = 12323334.34343f, Integer = 100, Null = null
             };
         }

         [Fact]
         public void Test_Self_Exists()
         {
             var selfUri = new Uri("http://someuri");
             
             var r = _factory.Create(selfUri);

             var l = r.GetLink("self");
             Assert.NotEmpty(l.Values);
             Assert.Equal(selfUri,l.Value.Href);
             Assert.Equal(selfUri, r.Self);
         }
         
         [Fact]
         public void Test_Add_Fields()
         { 
             var selfUri = new Uri("http://someuri");

             var r = _factory.Create(selfUri).Add(new {Test= 33}).Add(new {test2= "hello"});

             var d = r.Bind(new {test2 = "",Test= 0});

             Assert.Equal(33, d.Test);
             Assert.Equal("hello", d.test2);
         }
         
         [Fact]
         public void Test_Add_Null()
         {
             var selfUri = new Uri("http://someuri");

             var r = _factory.Create(selfUri).Add<object>(null);

             var m = r.As<MyModel>();
             
             Assert.Null(m.Test);         
         }

         [Fact]
         public void Test_Merge_Only()
         {
             var selfUri = new Uri("http://someuri");

             var r = _factory.Create(selfUri).Add(new MyModel
             {
                 Integer = 123456789
             });

             var m = r.As<MyModel>();
             
             Assert.Null(m.Test);
             Assert.Equal(123456789, m.Integer);
         }
         
         [Fact]
         public void Test_Override_Field()
         {
             var selfUri = new Uri("http://someuri");

             var r = _factory.Create(selfUri)
                 .Add(new {F1 = 123})
                 .Add(new {F1 = "hello"});

             var m = r.Bind(new {F1 = ""});
             
             Assert.Equal("hello",m.F1);
         }

         [Fact]
         public void Test_Add_Dictionary_As_Property()
         {
             var selfUri = new Uri("http://someuri");

             var r = _factory.Create(selfUri).Add(new Dictionary<string, string>() {{"1", "A"}, {"2", "B"}});
             
             var f = r.As<Dictionary<string, string>>();
             
             Assert.Equal("A", f["1"]);
             Assert.Equal("B", f["2"]);
         }
         
         
         [Fact]
         public void Test_Add_List_As_Property()
         {
             var selfUri = new Uri("http://someuri");

             Assert.Throws<ArgumentException>(() => _factory.Create(selfUri).Add(new List<string>() {"A", "B", "C"}));
         }

         [Fact]
         public void Test_Add_Link()
         {
             var l = new Link(new Uri("http://mylinkuri"),
                 deprecation: "http://seedeprecationhowto",
                 hrefLang: "fr_FR",
                 name: "other_name",
                 profile: "http://myprofile",
                 templated:  true,
                 title: "Sample link",
                 type: "application/pdf",
                 methods: new []{HttpMethod.Delete, HttpMethod.Head});
             
             var selfUri = new Uri("http://someuri");

             var r = _factory.Create(selfUri).AddLink("sample",l);

             var json = _factory.CreateSerializer().Serialize(r).Result;

             Assert.Contains("sample", r.GetLinks().Keys);
             
             var links = r.GetLinks()["sample"];
             
             Assert.Single(links.Values);
             Assert.Equal(l.Deprecation, links.Value.Deprecation);
             Assert.Equal(l.Href, links.Value.Href);
             Assert.Equal(l.Hreflang, links.Value.Hreflang);
             Assert.Equal(l.Name, links.Value.Name);
             Assert.Equal(l.Profile, links.Value.Profile);
             Assert.Equal(l.Templated, links.Value.Templated);
             Assert.Equal(l.Title, links.Value.Title);
             Assert.Equal(l.Type, links.Value.Type);
             Assert.Equal(l.Methods, links.Value.Methods);
         }

         
         [Fact]
         public void Fields_cannot_be_named_links_or_embedded()
         {
             var selfUri = new Uri("http://someuri");

             var r = _factory.Create(selfUri);

             Assert.Throws<ArgumentException>(() => r.Add(new {_links = "teet"}));
             Assert.Throws<ArgumentException>(() => r.Add(new {_embedded = "teet"}));
         }

         [Fact]
         public void Test_null_arguments()
         {
             var selfUri = new Uri("http://someuri");

             var r = _factory.Create(selfUri);

             Assert.Throws<ArgumentException>(() => r.AddLink(null, (Link) null));
             Assert.Throws<ArgumentException>(() => r.AddEmbeddedResource(null, _factory.Create(selfUri)));
             Assert.Throws<ArgumentException>(() => r.AddEmbeddedResource(null, (IEnumerable<IResource>) null));
             Assert.Throws<ArgumentException>(() => r.AddEmbeddedResource(string.Empty, _factory.Create(selfUri)));
             
             Assert.Throws<ArgumentNullException>(() => r.AddLink("test", (IEnumerable<Link>) null));
             Assert.Throws<ArgumentNullException>(() => r.AddLink("test", (Link[]) null));
             
             Assert.Throws<ArgumentNullException>(() => r.AddEmbeddedResource("test", (IEnumerable<IResource>) null));
             Assert.Throws<ArgumentNullException>(() => r.AddEmbeddedResource("test", (IResource[]) null));
         }

         [Fact]
         public void Test_Add_Multiple_Links()
         {
             var l = new Link("http://mylinkuri1");
             var l2 = new Link("http://mylinkuri2");
             var l3 = new Link("http://mylinkuri3");
             
             var selfUri = new Uri("http://someuri");

             var r = _factory.Create(selfUri);

             r.AddLink("sample", new[] {l, l2});
             
             var links = r.GetLink("sample");
             
             Assert.Equal(2, links.Values.Count());
             Assert.Equal(l.Href,links.Values.First().Href);
             Assert.Equal(l2.Href,links.Values.Last().Href);
             
             //Override previous links
             r.AddLink("sample", l3);
             
             links = r.GetLink("sample");
             
             Assert.Single(links.Values);
             Assert.Equal(l3.Href,links.Value.Href);
             
             r.AddLink("sample", new []{ l, null, l2});
             links = r.GetLink("sample");
             
             Assert.Equal(2, links.Values.Count());
             Assert.Equal(l.Href,links.Values.First().Href);
             Assert.Equal(l2.Href,links.Values.Last().Href);
         }
         
         [Fact]
         public void Test_Add_Embedded()
         {
             var selfUri = new Uri("http://embedded");
             var e = _factory.Create(selfUri);
             Assert.Empty(e.GetEmbeddedResources().Keys);
             
             var r = _factory.Create(new Uri("http://root"))
                 .AddEmbeddedResource("test", e);
             
             Assert.Contains("test",r.GetEmbeddedResources().Keys);
             Assert.Single(r.GetEmbeddedResources());
             Assert.Equal(selfUri,r.GetEmbeddedResource("test").Value.Self);

             r.AddEmbeddedResource("test", new []{e,e});
             
             Assert.Contains("test",r.GetEmbeddedResources().Keys);
             Assert.Single(r.GetEmbeddedResources());

             var emm = r.GetEmbeddedResource("test");
             
             Assert.Equal(2,emm.Count);
             Assert.Equal(selfUri, emm.Values.First().Self);
             Assert.Equal(selfUri, emm.Values.Last().Self);
         }

         [Fact]
         public void Test_As_Method()
         {
             var m = GetModel();
             var selfUri = new Uri("http://embedded");
             var m2 = _factory.Create(selfUri).Add(m).As<MyModel>();
             
             Assert.Equal(m.Date,m2.Date);
             Assert.Equal(m.Decimal,m2.Decimal);
             Assert.Equal(m.Double,m2.Double);
             Assert.Equal(m.Float,m2.Float);
             Assert.Equal(m.Integer,m2.Integer);
             Assert.Equal(m.Null,m2.Null);
             Assert.Equal(m.Test,m2.Test);
             Assert.Equal(m.Uri,m2.Uri);
         }
         
         [Fact]
         public void Test_As_Method_Case_Insensitive()
         {
             var m = GetModel();
             var selfUri = new Uri("http://embedded");
             var m2 = _factory.Create(selfUri).Add(m).Bind(new {date = DateTime.Now});
             
             Assert.Equal(m.Date,m2.date);
         }

         [Fact]
         public async Task TestSerializer()
         {
             var selfUri = new Uri("http://sdsdsdsd");
             var m2 = _factory.Create(selfUri);
             var embedde = _factory.Create(new Uri("https://dfdfdfdfdf/self"));

             var r = _factory.Create(new Uri("http://sdsdsdsd"));
             r.AddLink("prev", new Link("http://ddfdfdfddf/prev"))
                 .AddLink("next", new Link("http://ddfdfdfddf/next"))
                 .Add(new {testint=13})
                 .Add(GetModel())
                 .Add(new {testurl="http://dfdfdfdf"} )
                 .AddEmbeddedResource("same", embedde)
                 .AddEmbeddedResource("same", embedde)
                 .AddEmbeddedResource("same", embedde)
                 .AddLink("same", new Link(embedde.Self) {Name = "coucou"})
                 .AddLink("same", new Link(embedde.Self) {Name = "toto", Methods = new []{HttpMethod.Get, HttpMethod.Patch}});

             var json = await _serializer.Serialize(r);
             var r2 = await _serializer.Deserialize(json);
             Assert.Equal(json,await _serializer.Serialize(r2));

         }
     }
 }