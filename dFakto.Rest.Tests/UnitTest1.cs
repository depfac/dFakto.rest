using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;
using Xunit.Sdk;

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
            var r = CreateResource().Self("http://someuri");
            
            Assert.Equal("http://someuri", r.GetSelf().Href);

            var l = r.GetLinks("self");
            Assert.NotEmpty(l);
            Assert.Equal("http://someuri",l.First().Href);

            var ul = r.GetLink("self");
            Assert.Equal("http://someuri",ul.Href);
        }
        [Fact]
        public void Test_Add_Fields()
        {
            var r = CreateResource().Self("http://someuri").Add("test", 33).Add("test2", "hello");
            
            Assert.Equal(33,r.GetField<int>("test"));
            Assert.Equal("hello",r.GetField<string>("test2"));
        }
        
        [Fact]
        public void Test_Add_Null()
        {
            var r = CreateResource().Self("http://someuri").Add("test", null,null);
            
            Assert.Null(r.GetField<string>("test"));
            Assert.False(r.ContainsField("test2"));
            Assert.Null(r.GetField<string>("test2"));
        }

        [Fact]
        public void Test_Merge_Only()
        {
            var r = CreateResource().Self("http://someuri").Merge(new {Field1 = "dfdf", Field2 = "test"}, new []{"Field2"});
            
            Assert.Null(r.GetField<string>("field1"));
            Assert.False(r.ContainsField("field2"));
            Assert.Null(r.GetField<string>("field2"));
        }
        
        [Fact]
        public void Test_Override_Field()
        {
            var r = CreateResource().Self("http://someuri")
                .Add("test", "value")
                .Add("test", 10);
            
            Assert.Equal(10,r.GetField<int>("test"));
        }

        [Fact]
        public void Test_Add_Dictionary_As_Property()
        {
            var r = CreateResource().Self("http://someuri").Add("testdic",
                new Dictionary<string, string>() {{"1", "A"}, {"2", "B"}});


            var f = r.GetField<Dictionary<string, string>>("testdic");
            
            Assert.Equal("A", f["1"]);
            Assert.Equal("B", f["2"]);
        }
        
        
        [Fact]
        public void Test_Add_List_As_Property()
        {
            var r = CreateResource().Self("http://someuri").Add("testdic",new List<string>(){"A", "B","C"});


            var f = r.GetField<string[]>("testdic");
            
            Assert.Equal("A", f[0]);
            Assert.Equal("B", f[1]);
            Assert.Equal("C", f[2]);
            
            
            var f2 = r.GetField<List<string>>("testdic");
            
            Assert.Equal("A", f2[0]);
            Assert.Equal("B", f2[1]);
            Assert.Equal("C", f2[2]);
        }

        [Fact]
        public void Test_Add_Link()
        {
            var l = new Link("http://mylinkuri")
            {
                Deprecation = "http://seedeprecationhowto",
                Href = "http://myhref",
                Hreflang = "fr_FR",
                Name = "other_name",
                Profile = "http://myprofile",
                Templated = true,
                Title = "Sample link",
                Type = "application/pdf",
                Rights = Right.Delete|Right.Put
            };

            var r = CreateResource().Self("http://dfdfdfd").AddLink("sample",l);

            Assert.Contains("sample", r.LinkNames);
            
            var links = r.GetLinks("sample").ToList();
            
            Assert.Single(links);
            Assert.Equal(l.Deprecation, links[0].Deprecation);
            Assert.Equal(l.Href, links[0].Href);
            Assert.Equal(l.Hreflang, links[0].Hreflang);
            Assert.Equal(l.Name, links[0].Name);
            Assert.Equal(l.Profile, links[0].Profile);
            Assert.Equal(l.Templated, links[0].Templated);
            Assert.Equal(l.Title, links[0].Title);
            Assert.Equal(l.Type, links[0].Type);
            Assert.Equal(l.Rights, links[0].Rights);
        }

        [Fact]
        public void Test_Get_Link_Return_Empty_if_no_link()
        {
            var r = CreateResource();
            
            Assert.Empty(r.GetLinks("test"));
        }
        
        [Fact]
        public void Fields_cannot_be_named_links_or_embedded()
        {
            var r = CreateResource().Self("hello").AddEmbeddedResource("test", CreateResource());

            Assert.False(r.ContainsField("_links"));
            Assert.False(r.ContainsField("_embedded"));
            Assert.DoesNotContain("_links",r.FieldsNames);
            Assert.DoesNotContain("_embedded",r.FieldsNames);

            Assert.Throws<ArgumentException>(() => r.Add("_links", "test"));
            Assert.Throws<ArgumentException>(() => r.Add("_embedded", "test"));
        }

        [Fact]
        public void Test_null_arguments()
        {
            var r = CreateResource();
            
            Assert.Throws<ArgumentNullException>(() => r.Add(null, "test"));
            Assert.Throws<ArgumentNullException>(() => r.Add(string.Empty, "test"));
            Assert.Throws<ArgumentNullException>(() => r.AddLink(null,"href"));
            Assert.Throws<ArgumentNullException>(() => r.AddLink("testnull",(Link)null));
            Assert.Throws<ArgumentNullException>(() => r.AddLink("testnull",(string)null));
            Assert.Throws<ArgumentNullException>(() => r.AddLink(null,(Link)null));
            Assert.Throws<ArgumentNullException>(() => r.AddLink(null,(string)null));
            Assert.Throws<ArgumentNullException>(() => r.AddLinks(null,null));

            Assert.Throws<ArgumentNullException>(() => r.AddEmbeddedResource(null,CreateResource()));
            Assert.Throws<ArgumentNullException>(() => r.AddEmbeddedResources(null,(IEnumerable<Resource>) null));
            Assert.Throws<ArgumentNullException>(() => r.AddEmbeddedResource(string.Empty,CreateResource()));
            Assert.Throws<ArgumentNullException>(() => r.AddEmbeddedResources("test", (IEnumerable<Resource>) null));
            Assert.Throws<ArgumentNullException>(() => r.AddEmbeddedResources("test", (Resource[]) null));
            
            Assert.Throws<ArgumentNullException>(() => r.AddLink("testnull",string.Empty));
            Assert.Throws<ArgumentNullException>(() => r.AddLinks("testnull",null));
            Assert.Throws<ArgumentNullException>(() => r.GetEmbeddedResources(null).ToArray());
            Assert.Throws<ArgumentNullException>(() => r.GetEmbeddedResources(string.Empty).ToArray());
            Assert.Throws<ArgumentNullException>(() => r.GetLinks(null).ToArray());
            Assert.Throws<ArgumentNullException>(() => r.GetLinks(string.Empty).ToArray());
            Assert.Throws<ArgumentNullException>(() => r.GetField<string>(null));
            
            
            Assert.Throws<ArgumentNullException>(() => r.ContainsEmbeddedResource(null));
            Assert.Throws<ArgumentNullException>(() => r.ContainsEmbeddedResource(string.Empty));
            Assert.Throws<ArgumentNullException>(() => r.ContainsLink(null));
            Assert.Throws<ArgumentNullException>(() => r.ContainsLink(string.Empty));
            Assert.Throws<ArgumentNullException>(() => r.ContainsField(null));
            Assert.Throws<ArgumentNullException>(() => r.ContainsField(string.Empty));
            Assert.Throws<ArgumentNullException>(() => r.Self(string.Empty));
            Assert.Throws<ArgumentNullException>(() => r.Self(string.Empty));
            Assert.Throws<ArgumentNullException>(() => r.Merge<object>(null));
            Assert.Throws<ArgumentNullException>(() => r.Merge<object>(null,new string[]{"test"}));
        }

        [Fact]
        public void Test_Add_Multiple_Links()
        {
            var l = new Link("http://mylinkuri1");
            var l2 = new Link("http://mylinkuri2");
            var l3 = new Link("http://mylinkuri3");

            var r = CreateResource().Self("http://testcom");

            r.AddLinks("sample", l, l2);
            
            var links = r.GetLinks("sample").ToArray();
            
            Assert.Equal(2, links.Length);
            Assert.Equal("http://mylinkuri1",links[0].Href);
            Assert.Equal("http://mylinkuri2",links[1].Href);
            
            //Override previous links
            r.AddLink("sample", l3);
            
            links = r.GetLinks("sample").ToArray();
            
            Assert.Single(links);
            Assert.Equal("http://mylinkuri3",links[0].Href);

            r.AddLinks("sample", l, l2);
            links = r.GetLinks("sample").ToArray();
            
            Assert.Equal(2, links.Length);
            Assert.Equal("http://mylinkuri1",links[0].Href);
            Assert.Equal("http://mylinkuri2",links[1].Href);
            
            r.AddLinks("sample", l, null, l2);
            links = r.GetLinks("sample").ToArray();
            
            Assert.Equal(2, links.Length);
            Assert.Equal("http://mylinkuri1",links[0].Href);
            Assert.Equal("http://mylinkuri2",links[1].Href);
        }
        
        [Fact]
        public void Test_Add_Embedded()
        {
            var e = CreateResource().Self("embedded");
            Assert.Empty(e.EmbeddedResourceNames);
            Assert.Empty(e.GetEmbeddedResources("not_exists"));
            
            var r = CreateResource().Self("root")
                .AddEmbeddedResource("test", e);
            
            Assert.Contains("test",r.EmbeddedResourceNames);
            Assert.DoesNotContain("test_not_exists",r.EmbeddedResourceNames);
            Assert.Single(r.EmbeddedResourceNames);
            Assert.True(r.ContainsEmbeddedResource("test"));
            Assert.False(r.ContainsEmbeddedResource("test_not_exists"));
            Assert.Equal("embedded",r.GetEmbeddedResources("test").First().GetSelf().Href);
            Assert.Empty(r.GetEmbeddedResources("test_dot_not_exists"));

            r.AddEmbeddedResources("test", e, e);
            
            Assert.Contains("test",r.EmbeddedResourceNames);
            Assert.DoesNotContain("test_not_exists",r.EmbeddedResourceNames);
            Assert.Single(r.EmbeddedResourceNames);
            Assert.True(r.ContainsEmbeddedResource("test"));
            Assert.False(r.ContainsEmbeddedResource("test_not_exists"));

            var emm = r.GetEmbeddedResources("test").ToArray();
            
            Assert.Equal(2,emm.Length);
            Assert.Equal("embedded", emm[0].GetSelf().Href);
            Assert.Equal("embedded", emm[1].GetSelf().Href);
            
            Assert.Empty(r.GetEmbeddedResources("test_dot_not_exists"));
            
            r.AddEmbeddedResources("test", e,e,e);
            
            emm = r.GetEmbeddedResources("test").ToArray();

            Assert.Equal(3, emm.Length);
            
            r.AddEmbeddedResources("test", e,null,e);
            
            emm = r.GetEmbeddedResources("test").ToArray();

            Assert.Equal(2, emm.Length);

            var rr = CreateResource();
            rr.AddEmbeddedResources("test", emm);


            e.AddEmbeddedResource("testsingle",CreateResource());
            
            Assert.NotNull(e.GetEmbeddedResource("testsingle")); 
        }

        [Fact]
        public void Test_As_Method()
        {
            var m = GetModel();
            var m2 = CreateResource().Merge(m).As<MyModel>();
            
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
        public void TestSerializer()
        {
            var ser = new JsonSerializerSettings();
            ser.ContractResolver = new CamelCasePropertyNamesContractResolver();
            ser.NullValueHandling = NullValueHandling.Ignore;

            var embedde = CreateResource().Self("https://dfdfdfdfdf/self");

            var r = CreateResource().Self("http://sdsdsdsd");
            r.AddLink("prev", "http://ddfdfdfddf/prev")
                .AddLink("next", "http://ddfdfdfddf/next")
                .Add("testint", 33)
                .Merge((IMyModel)GetModel(), new[] {"Double"})
                .Merge(GetModel())
                .Add("testurl", "http://dfdfdfdf")
                .Merge(new MyModel {Test = "toto"}, new[] {"test"})
                .AddEmbeddedResource("same", embedde)
                .AddEmbeddedResource("same", embedde)
                .AddEmbeddedResource("same", embedde)
                .AddLink("same", new Link(embedde.GetSelf().Href) {Name = "coucou"})
                .AddLink("same", new Link(embedde.GetSelf().Href) {Name = "toto"});
            
            string json = JsonConvert.SerializeObject(r,Formatting.Indented, ser);

            var rr = JsonConvert.DeserializeObject<Resource>(json);
            
            string json2 = JsonConvert.SerializeObject(rr,Formatting.Indented, ser);
            
            Assert.Equal(json,json2);

        }

        [Fact]
        public void Test_NamingConvention()
        {
            var r = CreateResource().Add("NAME", "hello");
            
            Assert.Equal("hello",r.GetField<string>("NAME"));
            Assert.True(r.ContainsField("NAME"));
            Assert.True(r.ContainsField("name"));
            Assert.False(r.ContainsField("nAME"));

            r.AddLink("LINK", "http://sss");
            Assert.True(r.ContainsLink("link"));
            Assert.True(r.ContainsLink("Link"));
            Assert.False(r.ContainsLink("lINk"));

            r.AddEmbeddedResource("MyResource", CreateResource());
            Assert.True(r.ContainsEmbeddedResource("MyResource"));
            Assert.True(r.ContainsEmbeddedResource("myResource"));

            r = CreateResource(false).Add("NAME", "hello");
            Assert.Equal("hello",r.GetField<string>("NAME"));
            Assert.True(r.ContainsField("NAME"));
            Assert.False(r.ContainsField("name"));
            Assert.False(r.ContainsField("nAME"));
        }

        public Resource CreateResource(bool overrideSpecificName = true)
        {   
            var ccpncr = new CamelCasePropertyNamesContractResolver();
            ccpncr.NamingStrategy.OverrideSpecifiedNames = overrideSpecificName;
            
            var ser = new JsonSerializerSettings();
            ser.ContractResolver = ccpncr;
            ser.NullValueHandling = NullValueHandling.Ignore;
            

            ResourceBuilder builder = new ResourceBuilder(ser);
            return builder.Create();
        }
    }
}