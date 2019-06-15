using System;
using System.Linq;
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
            var r = new Resource("http://someuri");
            
            Assert.Equal("http://someuri", r.Self);

            var l = r.GetLinks("self");
            Assert.NotEmpty(l);
            Assert.Equal("http://someuri",l.First().Href);
        }
        [Fact]
        public void Test_Add_Fields()
        {
            var r = new Resource("http://someuri");
            r.Add("test", 33);
            r.Add("test2", "hello");
            
            Assert.Equal(33,r.GetField<int>("test"));
            Assert.Equal("hello",r.GetField<string>("test2"));
        }
        
        [Fact]
        public void Test_Add_Null()
        {
            var r = new Resource("http://someuri");
            r.Add("test", null);
            
            Assert.Null(r.GetField<string>("test"));
            Assert.False(r.ContainsField("test2"));
            Assert.Null(r.GetField<string>("test2"));
        }
        
        [Fact]
        public void Test_Override_Field()
        {
            var r = new Resource("http://someuri");
            r.Add("test", "value");
            r.Add("test", 10);
            
            Assert.Equal(10,r.GetField<int>("test"));

            r.Add("test", null);
            
            Assert.False(r.ContainsField("test"));
            Assert.Null(r.GetField<string>("test"));
        }
        
        [Fact]
        public void Test_Remove_Field()
        {
            var r = new Resource("http://someuri");
            r.Add("test", 10);
            
            Assert.Equal(10,r.GetField<int>("test"));

            r.RemoveField("test");
            
            Assert.False(r.ContainsField("test"));
            Assert.Null(r.GetField<string>("test"));
        }

        [Fact]
        public void TestSerializer()
        {
            var ser = new JsonSerializerSettings();
            ser.ContractResolver = new CamelCasePropertyNamesContractResolver();
            ser.NullValueHandling = NullValueHandling.Ignore;

            Resource embedde = new Resource("https://dfdfdfdfdf/self");

            Resource r = new Resource("http://sdsdsdsd");
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
                .AddLink("same", new Link(embedde.Self) {Name = "coucou"})
                .AddLink("same", new Link(embedde.Self) {Name = "toto"});
            
            string json = JsonConvert.SerializeObject(r,Formatting.Indented, ser);

            Resource rr = JsonConvert.DeserializeObject<Resource>(json);
            
            string json2 = JsonConvert.SerializeObject(rr,Formatting.Indented, ser);
            
            Assert.Equal(json,json2);

        }
    }
}