using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
        public void Test()
        {
            var r = CreateResource().Self("http://someuri").Add(new {Field1 = "dfdf", Field2 = "test"}, new []{"Field2"});
            
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
                .Add((IMyModel)GetModel(), new[] {"Double"})
                .Add(GetModel())
                .Add("testurl", "http://dfdfdfdf")
                .Add(new MyModel {Test = "toto"}, new[] {"test"})
                .AddEmbedded("same", embedde)
                .AddEmbedded("same", embedde)
                .AddEmbedded("same", embedde)
                .AddLink("same", new Link(embedde.GetSelf().Href) {Name = "coucou"})
                .AddLink("same", new Link(embedde.GetSelf().Href) {Name = "toto"});
            
            string json = JsonConvert.SerializeObject(r,Formatting.Indented, ser);

            var rr = JsonConvert.DeserializeObject<Resource>(json);
            
            string json2 = JsonConvert.SerializeObject(rr,Formatting.Indented, ser);
            
            Assert.Equal(json,json2);

        }

        public Resource CreateResource()
        {   
            var ser = new JsonSerializerSettings();
            ser.ContractResolver = new CamelCasePropertyNamesContractResolver();
            ser.NullValueHandling = NullValueHandling.Ignore;

            ResourceBuilder builder = new ResourceBuilder(ser);
            return builder.Create();
        }
    }
}