using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace dFakto.Rest.Tests
{
    public class MyModel
    {
        public string Test { get; set; }
        public DateTime Date { get; set; }
        public decimal Decimal { get; set; }
        public int Integer { get; set; }
        public double Double { get; set; }
        public float Float { get; set; }
        public Uri Uri { get; set; }
    }

    public class UnitTest1
    {
        private MyModel GetModel()
        {
            return new MyModel
            {
                Test = "coucou", Date = DateTime.Now, Decimal = 22.33m, Uri = new Uri("http://someuri"),
                Double = 123.232, Float = 12323334.34343f, Integer = 100
            };
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
                .Remove("Double")
                .Add(GetModel(), new[] {"Double"})
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


        [Fact]
        public void Test1()
        {
            Resource embedde = new Resource("https://dfdfdfdfdf/self");

            Resource r = new Resource("http://sdsdsdsd");
            r.AddLink("prev", "http://ddfdfdfddf/prev")
                .AddLink("next", "http://ddfdfdfddf/next")
                .Add("testint", 33)
                .Remove("Double")
                .Add(GetModel(), new[] {"Double"})
                .Add(GetModel())
                .Add("testurl", new Uri("http://dfdfdfdf"))
                .Add(new MyModel {Test = "toto"}, new[] {"test"})
                .AddEmbedded("same", embedde)
                .AddEmbedded("same", embedde)
                .AddEmbedded("same", embedde)
                .AddLink("same", new Link(embedde.Self) {Name = "coucou"})
                .AddLink("same", new Link(embedde.Self) {Name = "toto"});
        }
    }
}