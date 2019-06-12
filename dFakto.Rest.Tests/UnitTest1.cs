using System;
using Newtonsoft.Json;
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
        public void Test1()
        {
            Resource.JsonSerializer.DateFormatHandling = DateFormatHandling.IsoDateFormat;

            Resource embedde = new Resource("https://dfdfdfdfdf/self");

            Resource r = new Resource(new Uri("http://sdsdsdsd"));
            r.AddLink("prev", new Uri("http://ddfdfdfddf/prev"))
                .AddLink("next", new Uri("http://ddfdfdfddf/next"))
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
            var j = r.ToString(true);
        }
    }
}