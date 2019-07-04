using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dFakto.Rest.AspNetCore.Mvc;
using dFakto.Rest.SampleApi.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace dFakto.Rest.SampleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : RestController
    {
        private readonly SampleRepository _repository;

        public ValuesController(SampleRepository repository)
        {
            _repository = repository;
        }

        private Resource GetSampleResource(MySampleValue sampleValue, IEnumerable<string> onlyFields = null)
        {
            return CreateResource(GetUriFromRoute("getbyid",new {id=sampleValue.Id}))
                .AddLink("parent",GetUriFromRoute("getallvalues"))
                .Merge(sampleValue,onlyFields);
        }

        private Resource GetSampleResourceCollection(
            CollectionRequest request,
            IEnumerable<MySampleValue> values)
        {
            var r = CreateResourceCollection(GetUriFromRoute("getallvalues"), request)
                .AddEmbedded("values", values.Select(x => GetSampleResource(x, request.Fields)));
            return r;
        }


        // GET api/values
        [HttpGet()]
        [Route("",Name="getallvalues")]
        public ActionResult Get([FromQuery] CollectionRequest request)
        {
            return Ok(GetSampleResourceCollection(request,_repository.GetValues(request.Index,request.Limit, request.Sort)));
        }

        // GET api/values/5
        [HttpGet("{id}",Name = "getbyid")]
        public Resource Get(int id, [FromQuery] ResourceRequest request)
        {
            return GetSampleResource(_repository.GetById(id), request.Fields);
        }

        // POST api/values
        [HttpPost]
        public ActionResult Post(Resource resource)
        {
            var r = GetSampleResource(_repository.Create(resource.As<MySampleValue>()));
            return Created(r.GetSelf().Href, r);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public OkResult Put(int id, [FromBody] Resource value)
        {
            _repository.Update(id, value.As<MySampleValue>());
            return Ok();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public OkResult Delete(int id)
        {
            _repository.DeleteById(id);
            return Ok();
        }
    }
}