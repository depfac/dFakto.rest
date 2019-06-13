using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace dFakto.Rest.SampleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        public class MySampleValue
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }
        
        // GET api/values
        [HttpGet()]
        public ResourceResult Get([FromQuery] CollectionRequest request)
        {
            //Create uri builder
            ResourceUriBuilder builder = new ResourceUriBuilder(Url);

            //retrieve data
            var total = 100;
            var r = GetValues(total).Skip(request.Index).TakeWhile((x,y) => y < request.Limit);
            
            //Compute Response
            return new ResourceResult(
                new CollectionResource(
                    builder.GetCurrentRouteUri(), 
                    request, 
                    total,
                    r.Select(x => new Resource(builder.GetUriFromRoute("getbyid",new {x.Id})).Add(x))));
        }

        // GET api/values/5
        [HttpGet("{id}",Name = "getbyid")]
        public ResourceResult Get(int id)
        {
            ResourceUriBuilder builder = new ResourceUriBuilder(Url);
            return new ResourceResult(
                new Resource(builder.GetCurrentRouteUri())
                    .Add(new MySampleValue{Id = id, Value = "Value"+id}));
        }

        // POST api/values
        [HttpPost]
        public CreatedResult Post()
        {
            //Create uri builder
            ResourceUriBuilder builder = new ResourceUriBuilder(Url);
            return Created(builder.GetUriFromRoute("getbyid", new {id = 12}),null);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public OkResult Put(int id, [FromBody] string value)
        {
            return Ok();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public OkResult Delete(int id)
        {
            return Ok();
        }
        
        
        private IEnumerable<MySampleValue> GetValues(int max)
        {
            int i = 0;
            while (i < max)
            {
                i++;
                yield return new MySampleValue{Id = i,Value = "Value" + i};
            }
        }
    }
}