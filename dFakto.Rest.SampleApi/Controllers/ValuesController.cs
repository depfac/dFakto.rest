using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dFakto.Rest.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace dFakto.Rest.SampleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : RestController
    {
        public class MySampleValue
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }
        
        // GET api/values
        [HttpGet()]
        public ActionResult Get([FromQuery] CollectionRequest request)
        {
            //retrieve data
            var total = 100;
            var r = GetValues(total).Skip(request.Index).TakeWhile((x,y) => y < request.Limit);

            return Ok(CreateResourceCollection(GetCurrentUri(), request, total).AddEmbedded("values",r.Select(x => CreateResource(GetUriFromRoute("getbyid",new {id=x.Id})).Merge(x))));
        }

        // GET api/values/5
        [HttpGet("{id}",Name = "getbyid")]
        public Resource Get(int id)
        {
            return CreateResource(GetCurrentUri())
                .Merge(new MySampleValue{Id = id, Value = "Value"+id});
        }

        // POST api/values
        [HttpPost]
        public CreatedResult Post()
        {
            var uri = GetUriFromRoute("getbyid", new {id = 12});
            return Created(uri,CreateResource(uri).Add("test","value"));
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